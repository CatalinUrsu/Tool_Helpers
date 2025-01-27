using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ObservableCollections;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.Localization.Settings;

namespace Helpers.UI
{
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("UI/Helpers/Dropdown Helper")]
public class DropdownHelper : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
#region Fields

    [field: SerializeField] public DropdownCaption Caption;
    [field: SerializeField] public DropdownPanel Panel;
    [field: SerializeField] public DropdownItem ItemTemplate;
    [field: SerializeField] public Transform ItemParent;

    [field: SerializeField] public EDropdownAnimation AnimationType;
    [field: SerializeField] public float PanelAnimSpeed = 0.25f;

    [field: SerializeField] public bool MultiSelect;
    [field: SerializeField] public bool AddNoneItem;
    [field: SerializeField] public bool AddEverythingItem;
    [field: SerializeField] public DropdownOption NothingOption;
    [field: SerializeField] public DropdownOption MixedOption;
    [field: SerializeField] public DropdownOption EverythingOption;
    [field: SerializeField] public List<DropdownOption> Options = new List<DropdownOption>();

    public event Action<int> OnSelectItem;
    public event Action<int, bool> OnMultipleSelectItem;
    public ObservableList<DropdownOption> ObservableOptions = new ObservableList<DropdownOption>();

    bool _isDropdownShown;
    DropdownItem _noneItem;
    DropdownItem _everythingItem;
    DropdownItem _previousSelectedItem;
    List<DropdownItem> _items = new List<DropdownItem>();

#endregion

#region Monobeh

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        Assert.IsNotNull(Caption, "Caption is not set");
        Assert.IsNotNull(Panel, "OptionsPanel is not set");
        Assert.IsNotNull(ItemParent, "ItemParent is not set");
        Assert.IsNotNull(ItemTemplate, "DropdownItem is not set");

        ObservableOptions.CollectionChanged += ItemsCollectionChanged_handler;
        Panel.Init();
        ItemTemplate.gameObject.SetActive(false);
        AddMultiSelectExtraItems();
        AddOptions(Options);
    }

    protected override void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        base.OnEnable();
        InitEventsHandlers();

        void InitEventsHandlers()
        {
            OnSelectItem += OnSelectedItem_handler;
            OnMultipleSelectItem += OnMultipleSelectItem_handler;
            LocalizationSettings.SelectedLocaleChanged += OnChangeLanguage_handler;
        }
    }

    protected override void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        base.OnDisable();

        Hide(true);
        OnSelectItem -= OnSelectedItem_handler;
        OnMultipleSelectItem -= OnMultipleSelectItem_handler;
        LocalizationSettings.SelectedLocaleChanged -= OnChangeLanguage_handler;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ObservableOptions.CollectionChanged -= ItemsCollectionChanged_handler;
    }

    /// <summary>
    /// Mouse Pointer click 
    /// </summary>
    public virtual void OnPointerClick(PointerEventData eventData) => ShowHidePanel(eventData);

    /// <summary>
    /// Keyvboard or Gamepad Click
    /// </summary>
    public virtual void OnSubmit(BaseEventData eventData) => ShowHidePanel(eventData);

    /// <summary>
    /// Check if dropdown is deselected using navigation and use AxisEventData to hide if don't Navigate down (to items)
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (eventData is AxisEventData axisEventData)
            if (!(axisEventData.moveVector.y < 0))
                Hide();
    }

    /// <summary>
    /// When user presses the "Cancel" button or hits the Escape key
    /// </summary>
    public virtual void OnCancel(BaseEventData eventData)
    {
        Hide();

        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

#endregion

#region Public methods

    /// <summary>
    /// Add Options and create Item for each option
    /// </summary>
    public void AddOptions(IEnumerable<DropdownOption> options) => ObservableOptions.AddRange(options);

    /// <summary>
    /// Remove all options and all created Items
    /// </summary>
    public void ClearOptions() => ObservableOptions.Clear();

    /// <summary>
    /// Add items None and Everything for MultipleSelect if it's needed
    /// </summary>
    public void AddMultiSelectExtraItems()
    {
        if (!MultiSelect) return;

        if (this.AddNoneItem)
            AddNoneItem();

        if (this.AddEverythingItem)
            AddEverythingItem();

        void AddNoneItem()
        {
            _noneItem = Instantiate(ItemTemplate, ItemParent);

            _noneItem.gameObject.name = "None";
            _noneItem.Init(this, NothingOption);
            _noneItem.Toggle.SetIsOnWithoutNotify(true);
            _noneItem.Toggle.onValueChanged.AddListener(OnValueChange_handler);

            var toggleNavigation = _noneItem.Toggle.navigation;
            toggleNavigation.mode = Navigation.Mode.Explicit;
            toggleNavigation.selectOnUp = this;
            _noneItem.Toggle.navigation = toggleNavigation;

            Panel.AddSelectable(_noneItem.Toggle);

            void OnValueChange_handler(bool value)
            {
                if (value)
                    ToggleAllItems(false);
                else
                    ToggleNoneItem(true);
            }
        }

        void AddEverythingItem()
        {
            _everythingItem = Instantiate(ItemTemplate, ItemParent);

            _everythingItem.gameObject.name = "Everything";
            _everythingItem.Init(this, EverythingOption);
            _everythingItem.Toggle.SetIsOnWithoutNotify(false);
            _everythingItem.Toggle.onValueChanged.AddListener(OnValueChange_handler);

            var toggleNavigation = _everythingItem.Toggle.navigation;
            toggleNavigation.mode = Navigation.Mode.Explicit;
            _everythingItem.Toggle.navigation = toggleNavigation;

            Panel.AddSelectable(_everythingItem.Toggle);

            void OnValueChange_handler(bool value)
            {
                if (value)
                    ToggleAllItems(true);
                else
                {
                    if (this.AddNoneItem)
                        _noneItem.Toggle.isOn = true;
                    else if (_items.Count > 0)
                        ToggleItem(0, true);
                }
            }
        }
    }

    public void ToggleItem(int index, bool value)
    {
        if (index < 0 || index >= ObservableOptions.Count)
            return;

        if (_items[index].Toggle.isOn != value)
            _items[index].Toggle.isOn = value;
    }

    public void ToggleAllItems(bool value)
    {
        if (!MultiSelect)
            return;

        for (var idx = 0; idx < _items.Count; idx++)
        {
            var item = _items[idx];
            if (item.Toggle.isOn != value)
            {
                OnMultipleSelectItem?.Invoke(idx, value);
                item.Toggle.SetIsOnWithoutNotify(value);
            }
        }

        SetNoneAndEverythingItems();
    }

    public void Show(bool instantly = false)
    {
        if (_isDropdownShown)
            return;

        _isDropdownShown = true;
        Caption.SetArrow(true);
        Panel.Show(instantly ? EDropdownAnimation.None : AnimationType, PanelAnimSpeed);
    }

    public void Hide(bool instantly = false)
    {
        if (!_isDropdownShown)
            return;

        SelectDropdown();

        _isDropdownShown = false;
        Caption.SetArrow(false);
        Panel.Hide(instantly ? EDropdownAnimation.None : AnimationType, PanelAnimSpeed);
    }

#endregion

#region Private methods

    void CreateItem(DropdownOption optionData)
    {
        DropdownItem item = Instantiate(ItemTemplate, ItemParent);

        item.Init(this, optionData);
        item.Toggle.SetIsOnWithoutNotify(optionData.IsOn);
        item.Toggle.onValueChanged.AddListener(value => OnToggleItem_handler(item, value));

        var toggleNavigation = item.Toggle.navigation;
        toggleNavigation.mode = Navigation.Mode.Explicit;
        item.Toggle.navigation = toggleNavigation;

        Panel.AddSelectable(item.Toggle);
        _items.Add(item);
    }

    void DestroyItem(DropdownOption optionData)
    {
        var itemToDestroy = _items.First(dropdownItem => dropdownItem.GUID.Equals(optionData.GUID));

        if (itemToDestroy == null) return;
        _items.Remove(itemToDestroy);

        Destroy(itemToDestroy);
    }

    void ShowHidePanel(BaseEventData eventData)
    {
        //Check if was clicked TargetGraphic, to prevent action when click on child Selectable
        if (eventData.selectedObject != targetGraphic.gameObject) return;

        if (_isDropdownShown)
            Hide();
        else
            Show();
    }

    void RefreshCaption()
    {
        DropdownOption data = NothingOption;

        if (ObservableOptions.Count > 0)
        {
            if (MultiSelect)
            {
                var selectedItems = _items.Where(item => item.Toggle.isOn).ToArray();
                int selectedItemsCount = selectedItems.Length;

                data = selectedItemsCount switch
                       {
                           0 => NothingOption,
                           1 => ObservableOptions[_items.IndexOf(selectedItems[0])],
                           _ when selectedItemsCount == _items.Count => EverythingOption,
                           _ => MixedOption
                       };
            }
            else
                data = _previousSelectedItem ? ObservableOptions.First(option => option.GUID.Equals(_previousSelectedItem.GUID)) : data;
        }

        Caption.UpdateContent(data);
    }

    void SetNoneAndEverythingItems()
    {
        var selectedItemsCount = _items.Where(item => item.Toggle.isOn).ToArray().Length;

        ToggleNoneItem(selectedItemsCount == 0);
        ToggleEverythingItem(selectedItemsCount == _items.Count);
    }

    void ToggleNoneItem(bool value)
    {
        if (!_noneItem) return;
        _noneItem.Toggle.SetIsOnWithoutNotify(value);
    }

    void ToggleEverythingItem(bool value)
    {
        if (!AddEverythingItem) return;
        _everythingItem.Toggle.SetIsOnWithoutNotify(value);
    }

    void SetItemsOrderAndNavigatino()
    {
        SetOrderForExtraItems();
        SetItemsNavigation();

        void SetOrderForExtraItems()
        {
            var lastChildIdx = ItemParent.childCount - 1;

            if (_noneItem && _noneItem.transform.GetSiblingIndex() != 0)
                _noneItem.transform.SetSiblingIndex(0);

            if (_everythingItem && _everythingItem.transform.GetSiblingIndex() != lastChildIdx)
                _everythingItem.transform.SetSiblingIndex(lastChildIdx);
        }

        void SetItemsNavigation()
        {
            if (_noneItem)
            {
                var toggleNavigation = _noneItem.Toggle.navigation;
                toggleNavigation.selectOnDown = _items.Count > 0 ? _items[0].Toggle : _everythingItem ? _everythingItem.Toggle : null;
                _noneItem.Toggle.navigation = toggleNavigation;
            }

            if (_everythingItem)
            {
                var toggleNavigation = _everythingItem.Toggle.navigation;
                toggleNavigation.selectOnUp = _items.Count > 0 ? _items[_items.Count - 1].Toggle : _noneItem ? _noneItem.Toggle : this;
                _everythingItem.Toggle.navigation = toggleNavigation;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var toggleNavigation = item.Toggle.navigation;
                toggleNavigation.selectOnUp = (i - 1) >= 0 ? _items[i - 1].Toggle : _noneItem ? _noneItem.Toggle : this;
                toggleNavigation.selectOnDown = (i + 1) < _items.Count ? _items[i + 1].Toggle : _everythingItem ? _everythingItem.Toggle : null;
                item.Toggle.navigation = toggleNavigation;
            }
        }
    }

    void SelectDropdown()
    {
        if (!EventSystem.current) return;

        var currentSelectedGO = EventSystem.current.currentSelectedGameObject;
        if (!currentSelectedGO) return;

        bool selectedIsItem = _items.Any(item => currentSelectedGO == item.gameObject) ||
                              currentSelectedGO == _noneItem?.gameObject ||
                              currentSelectedGO == _everythingItem?.gameObject;

        if (selectedIsItem)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    void OnToggleItem_handler(DropdownItem selectedItem, bool isOn)
    {
        if (MultiSelect)
        {
            var idx = _items.IndexOf(selectedItem);
            OnMultipleSelectItem?.Invoke(idx, isOn);
            SetNoneAndEverythingItems();
        }
        else
        {
            var sameItem = _previousSelectedItem ? _previousSelectedItem.GUID.Equals(selectedItem.GUID) : false;

            if (isOn)
            {
                if (_previousSelectedItem && !sameItem)
                    _previousSelectedItem.Toggle.SetIsOnWithoutNotify(false);

                _previousSelectedItem = selectedItem;
                OnSelectItem?.Invoke(_items.IndexOf(selectedItem));
            }
            else if (sameItem)
            {
                selectedItem.Toggle.SetIsOnWithoutNotify(true);
            }

            Hide();
        }
    }

    void OnChangeLanguage_handler(Locale obj)
    {
        if (_noneItem)
            _noneItem.UpdateContent(NothingOption);

        if (_everythingItem)
            _everythingItem.UpdateContent(EverythingOption);

        for (int i = 0; i < ObservableOptions.Count; i++)
            _items[i].UpdateContent(ObservableOptions[i]);

        RefreshCaption();
    }

    void OnSelectedItem_handler(int idx) => RefreshCaption();

    void OnMultipleSelectItem_handler(int idx, bool value) => RefreshCaption();

    void ItemsCollectionChanged_handler(in NotifyCollectionChangedEventArgs<DropdownOption> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                    CreateItem(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                    DestroyItem(item);
                break;
        }

        SetItemsOrderAndNavigatino();
        RefreshCaption();
    }

#endregion
}
}