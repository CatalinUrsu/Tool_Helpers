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
    
    protected bool _tabIsSwapping;
    protected Tab _activeTab;
    protected List<UniTask> _tabsSwapTasks = new();
    protected CancellationTokenSource _cts = new();

#endregion

#region Public methods

    public virtual async UniTask Init(Action<float> onInitUpdate)
    {
        var tabPanelsInitedCount = 0f;
        var tabPanelInitTasks = new List<UniTask>();

        InitTabs();
        TabBtnClick_handler(_tabs[0], true);
        
        _activeTab = _tabs.Count > 0 ? _tabs[0] : _activeTab;
        await UniTask.WhenAll(tabPanelInitTasks);
        return;

        void InitTabs()
        {
            foreach (var tab in _tabs)
            {
                var tabBtn = tab.TabBtn;
                
                tabBtn.Init();
                tabBtn.BtnHelper.Btn.onClick.AddListener(() => TabBtnClick_handler(tab));

                tabPanelInitTasks.Add(tab.TabPanel.Init(_cts)
                                         .ContinueWith(UpdateTabPanelInited));
            }
        }

        void UpdateTabPanelInited()
        {
            tabPanelsInitedCount++;
            onInitUpdate?.Invoke(tabPanelsInitedCount / _tabs.Count);
        }
    }

#endregion

#region Private methods

    void TabBtnClick_handler(Tab selectedTab, bool skipAnimation = false)
    {
        if (!Equals(_activeTab, selectedTab)) 
            StartTabSwapping().Forget();
        return;

        async UniTaskVoid StartTabSwapping()
        {
            await CancelTabSwappingTask();

            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            SwapTabs(selectedTab, skipAnimation).Forget();
        }
    }

    protected virtual async UniTaskVoid SwapTabs(Tab selectedTab, bool skipAnimation)
    {
        SetTabsSwapTasks(selectedTab, skipAnimation);
        _tabIsSwapping = true;
        _activeTab = selectedTab;
        
        await UniTask.WhenAll(_tabsSwapTasks);
        await selectedTab.TabPanel.Show(skipAnimation, _cts);

        _tabIsSwapping = false;
    }

    protected virtual void SetTabsSwapTasks(Tab selectedTab, bool skipAnimation)
    {
        _tabsSwapTasks.Clear();
        
        _tabsSwapTasks.AddRange(new List<UniTask>
        {
            //Deselect previous TabBtn and hide previous TapPanel 
            _activeTab.TabBtn.Deselect(skipAnimation, _cts),
            _activeTab.TabPanel.Hide(skipAnimation, _cts),
            
            //Select new TabBtn
            selectedTab.TabBtn.Select(skipAnimation, _cts)
        });
    }

    async UniTask CancelTabSwappingTask()
    {
        if (!_tabIsSwapping || _cts.IsCancellationRequested) return;

        _cts.Cancel();
        while (_tabIsSwapping)
            await UniTask.Yield();
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