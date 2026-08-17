using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.UI
{
public class TabsGroup : MonoBehaviour
{
#region Fields

    [SerializeField] protected List<Tab> _tabs = new();

    protected Tab _activeTab;
    protected UniTask _currentSwapTask;
    protected List<UniTask> _tabsSwapTasks = new();
    protected CancellationTokenSource _initCts = new();
    protected CancellationTokenSource _swapCts = new();

#endregion

#region Public methods

    public virtual async UniTask Init(Action<float> onInitUpdate)
    {
        var tabPanelsInitedCount = 0f;
        var tabPanelInitTasks = new List<UniTask>();

        InitTabs();
        await UniTask.WhenAll(tabPanelInitTasks);

        StartTabSwapping(_tabs[0], true).Forget();
        return;

        void InitTabs()
        {
            foreach (var tab in _tabs)
            {
                var tabBtn = tab.TabBtn;

                tabBtn.Init();
                tabBtn.BtnHelper.Btn.onClick.AddListener(() => TabBtnClick_handler(tab));

                tabPanelInitTasks.Add(tab.TabPanel
                                         .Init(_initCts.Token)
                                         .ContinueWith(UpdateTabPanelInited));
            }
        }

        void UpdateTabPanelInited()
        {
            tabPanelsInitedCount++;
            onInitUpdate?.Invoke(tabPanelsInitedCount / _tabs.Count);
        }
    }

    public virtual async UniTask Deinit()
    {
        _initCts?.Dispose();
        _swapCts?.Dispose();
        _tabs.ForEach(tab => tab.TabPanel.Deinit());
        await UniTask.CompletedTask;
    }

#endregion

#region Private methods

    // TODO: Check for Race condition. Create UniTest and check if fast clicking doesn't broke logic
    void TabBtnClick_handler(Tab selectedTab)
    {
        if (Equals(_activeTab, selectedTab))
            return;

        StartTabSwapping(selectedTab).Forget();
    }

    async UniTaskVoid StartTabSwapping(Tab selectedTab, bool skipAnimation = false)
    {
        // Wait until the previous swap finishes its cancellation/cleanup.
        if (_currentSwapTask.Status == UniTaskStatus.Pending)
        {
            _swapCts?.Cancel();

            if (_currentSwapTask.Status == UniTaskStatus.Pending)
                await _currentSwapTask.SuppressCancellationThrow();
            
            _swapCts = new CancellationTokenSource();
        }
        
        _currentSwapTask = SwapTabs(selectedTab, skipAnimation);
        
        // Ignore cancellation - it's expected when another tab is clicked.
        await _currentSwapTask.SuppressCancellationThrow();
    }
    
    async UniTask SwapTabs(Tab selectedTab, bool skipAnimation)
    {
        SetTabsSwapTasks(selectedTab, skipAnimation);

        await UniTask.WhenAll(_tabsSwapTasks);
        
        _activeTab = selectedTab;
        await selectedTab.TabPanel.Show(skipAnimation, _swapCts.Token);
    }

    protected virtual void SetTabsSwapTasks(Tab selectedTab, bool skipAnimation)
    {
        _tabsSwapTasks.Clear();

        // If there is a valid active tab, add its deselect/hide tasks. Otherwise skip them (first init).
        if (_activeTab.TabBtn != null || _activeTab.TabPanel != null)
        {
            if (_activeTab.TabBtn != null)
                _tabsSwapTasks.Add(_activeTab.TabBtn.Deselect(skipAnimation, _swapCts.Token));

            if (_activeTab.TabPanel != null)
                _tabsSwapTasks.Add(_activeTab.TabPanel.Hide(skipAnimation, _swapCts.Token));
        }

        // Always select the new tab button
        if (selectedTab.TabBtn != null)
            _tabsSwapTasks.Add(selectedTab.TabBtn.Select(skipAnimation, _swapCts.Token));
    }
    
#endregion

#region External Data

    [Serializable]
    public struct Tab
    {
        public TabButton TabBtn;
        public TabPanel TabPanel;
    }

#endregion
}
}