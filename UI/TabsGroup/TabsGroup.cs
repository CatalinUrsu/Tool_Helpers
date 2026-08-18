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
    protected UniTask _currentSwapTask = UniTask.CompletedTask;
    protected List<UniTask> _tabsSwapTasks = new();
    protected CancellationTokenSource _initCts = new();
    protected CancellationTokenSource _swapCts = new();
    protected bool _isSwapInProgress;
    protected int _swapVersion;

#endregion

#region Public methods

    public virtual async UniTask Init(Action<float> onInitUpdate)
    {
        var tabPanelsInitedCount = 0f;
        await UniTask.WhenAll(GetInitTabsTasks());

        StartTabSwapping(_tabs[0], true).Forget();
        return;

        List<UniTask> GetInitTabsTasks()
        {
            var tabPanelInitTasks = new List<UniTask>();
            
            foreach (var tab in _tabs)
            {
                var tabBtn = tab.TabBtn;

                tabBtn.Init();
                tabBtn.BtnHelper.Btn.onClick.AddListener(() => TabBtnClick_handler(tab));

                tabPanelInitTasks.Add(InitItem(tab).ContinueWith(UpdateTabPanelInited));
            }
            
            return tabPanelInitTasks;
        }

        void UpdateTabPanelInited()
        {
            tabPanelsInitedCount++;
            onInitUpdate?.Invoke(tabPanelsInitedCount / _tabs.Count);
        }
    }

    public virtual async UniTask Deinit()
    {
        _initCts?.Cancel();
        _swapCts?.Cancel();
        _initCts?.Dispose();
        _swapCts?.Dispose();
        _tabs.ForEach(tab => tab.TabPanel.Deinit());
        
        await UniTask.CompletedTask;
    }

#endregion

#region Private methods

    protected virtual UniTask InitItem(Tab tab) => tab.TabPanel.Init(_initCts.Token);

    void TabBtnClick_handler(Tab selectedTab)
    {
        if (Equals(_activeTab, selectedTab))
            return;

        StartTabSwapping(selectedTab).Forget();
    }

    async UniTaskVoid StartTabSwapping(Tab selectedTab, bool skipAnimation = false)
    {
        await CancelPrevSwap();

        var swapVersion = ++_swapVersion;
        _isSwapInProgress = true;
        _currentSwapTask = SwapTabs(selectedTab, skipAnimation);
        
        try
        {
            await _currentSwapTask.SuppressCancellationThrow();
        }
        finally
        {
            // Only the latest swap request can clear the in-progress flag.
            if (swapVersion == _swapVersion)
            {
                _isSwapInProgress = false;
            }
        }
    }
    
    async UniTask SwapTabs(Tab selectedTab, bool skipAnimation)
    {
        // Snapshot the token BEFORE any await so that even if _swapCts is replaced
        // by a subsequent CancelPrevSwap, this task always uses its own cancellation token.
        var token = _swapCts.Token;
        SetTabsSwapTasks(selectedTab, skipAnimation);
        
        await UniTask.WhenAll(_tabsSwapTasks);

        _activeTab = selectedTab;
        await selectedTab.TabPanel.Show(skipAnimation, token);
    }

    async UniTask CancelPrevSwap()
    {
        if (!_isSwapInProgress)
            return;

        // 1. Signal cancellation to the running swap.
        _swapCts?.Cancel();

        // 2. Wait for the old SwapTabs to fully observe the cancellation and exit.
        //    Without this await the new SwapTabs starts before the old one finishes,
        //    causing both to reach TabPanel.Show concurrently with the same token.
        await _currentSwapTask.SuppressCancellationThrow();

        // 3. Only now replace the CTS so the new swap gets a clean token.
        _swapCts?.Dispose();
        _swapCts = new();
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