using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using EditorAttributes;
using ObservableCollections;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.Localization.Settings;

namespace Helpers.UI
{
[RequireComponent(typeof(RectTransform), typeof(Selectable))]
[AddComponentMenu("UI (Canvas)/Helpers/Dropdown Helper")]
public class DropdownHelper : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ICancelHandler, IDeselectHandler
{
#region Fields

    [SerializeField, VerticalGroup("BaseElements", true,
                                   nameof(_selectable),
                                   nameof(Caption),
                                   nameof(Panel),
                                   nameof(ItemTemplate),
                                   nameof(ItemParent),
                                   nameof(AnimationType),
                                   nameof(PanelAnimSpeed))]
    EditorAttributes.Void _groupBaseElements;
    [field: SerializeField, HideInInspector, Required] public Selectable _selectable;
    [field: SerializeField, HideInInspector, Required] public DropdownInfoCaption Caption;
    [field: SerializeField, HideInInspector, Required] public DropdownPanel Panel;
    [field: SerializeField, HideInInspector, Required] public DropdownInfoItem ItemTemplate;
    [field: SerializeField, HideInInspector, Required] public Transform ItemParent;

    [Header("Animation")]
    [field: SerializeField, HideInInspector, OnValueChanged(nameof(OnChangeAnimationType))] public EDropdownAnimation AnimationType;
    [field: SerializeField, HideInInspector, ShowField(nameof(_useAnimation))] public float PanelAnimSpeed = 0.25f;
    
    [Space(10)]
    [SerializeField, VerticalGroup(true, nameof(Options))] EditorAttributes.Void _groupOptions;
    [HideInInspector] public List<DropdownOption> Options = new();

    [Space(10)]
    [SerializeField, VerticalGroup("MultiSelect", true,
                                   nameof(MultiSelect),
                                   nameof(MixedOption),
                                   nameof(EnableNoneItem),
                                   nameof(NothingOption),
                                   nameof(EnableEverythingItem),
                                   nameof(EverythingOption))]
    EditorAttributes.Void _groupMultiSelect;
    [field: SerializeField, HideInInspector] public bool MultiSelect;
    [field: SerializeField, HideInInspector, ShowField(nameof(MultiSelect))] public DropdownOption MixedOption;
    
    [Space]
    [field: SerializeField, HideInInspector] public bool EnableNoneItem;
    [field: SerializeField, HideInInspector, ShowField(nameof(EnableNoneItem))] public DropdownOption NothingOption;
    
    [Space]
    [field: SerializeField, HideInInspector] public bool EnableEverythingItem;
    [field: SerializeField, HideInInspector, ShowField(nameof(EnableEverythingItem))] public DropdownOption EverythingOption;

    public event Action<int> OnSelectItem;
    public event Action<int, bool> OnMultipleSelectItem;
    public ObservableList<DropdownOption> ObservableOptions = new();

    bool _isDropdownShown;
    bool _useAnimation;
    DropdownInfoItem _noneItem;
    DropdownInfoItem _everythingItem;
    DropdownInfoItem _selectedItem;
    DropdownOption _originalMixedOption;
    DropdownOption _originalNoneOption;
    DropdownOption _originalEverythingOption;
    readonly List<DropdownInfoItem> _items = new();

#endregion

#region Monobeh

    void OnValidate()
    {
        _selectable ??= gameObject.GetComponent<Selectable>();
        OnChangeAnimationType();
    }

    protected void Awake()
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

        AddEventsListeners();
        SetMixedOption(MultiSelect);
        SetNoneOption(EnableNoneItem);
        SetEverythingOption(EnableEverythingItem);
        AddOptions(Options);
    }

    protected void OnEnable()
    {
        RemoveEventsListeners(); // Just to prevent listener duplicate
        AddEventsListeners();
    }

    protected void OnDisable()
    {
        Hide(true);
        RemoveEventsListeners();
    }

    protected void OnDestroy() => ObservableOptions.CollectionChanged -= ItemsCollectionChanged_handler;

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
    public void OnDeselect(BaseEventData eventData)
    {
        if (eventData is AxisEventData { moveVector: { y: >= 0 } })
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

    void SetMixedOption(bool enableMixedOption, DropdownOption option = null)
    {
        MultiSelect = enableMixedOption;

        if (MultiSelect)
            MixedOption = option ?? MixedOption;
    }

    void SetNoneOption(bool enableNoneOption, DropdownOption option = null)
    {
        EnableNoneItem = enableNoneOption;

        if (EnableNoneItem)
            AddNoneItem();
        else
            RemoveNoneItem();
        return;

        void AddNoneItem()
        {
            _noneItem = Instantiate(ItemTemplate, ItemParent);
            _noneItem.gameObject.name = "None";
            _noneItem.Init(this, option ?? NothingOption);
            _noneItem.Toggle.SetIsOnWithoutNotify(true);
            _noneItem.Toggle.onValueChanged.AddListener(OnValueChange_handler);

            var toggleNavigation = _noneItem.Toggle.navigation;
            toggleNavigation.mode = Navigation.Mode.Explicit;
            toggleNavigation.selectOnUp = _selectable;
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

        void RemoveNoneItem()
        {
            if (!_noneItem) return;

            Panel.RemoveSelectable(_noneItem.Toggle);
            Destroy(_noneItem);
        }
    }

    void SetEverythingOption(bool enableEverythingOption, DropdownOption option = null)
    {
        EnableEverythingItem = enableEverythingOption;

        if (EnableEverythingItem)
            AddEverythingItem();
        else
            RemoveEverythingItem();
        return;

        void AddEverythingItem()
        {
            _everythingItem = Instantiate(ItemTemplate, ItemParent);
            _everythingItem.gameObject.name = "Everything";
            _everythingItem.Init(this, option ?? EverythingOption);
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
                    if (EnableNoneItem)
                        _noneItem.Toggle.isOn = true;
                    else if (_items.Count > 0)
                        ToggleItem(0, true);
                }
            }
        }

        void RemoveEverythingItem()
        {
            if (!_everythingItem) return;

            Panel.RemoveSelectable(_everythingItem.Toggle);
            Destroy(_everythingItem);
        }
    }

    /// <summary>
    /// Toggles the selection state of a dropdown item at the specified index.
    /// </summary>
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
                item.Toggle.SetIsOnWithoutNotify(value);
                OnMultipleSelectItem?.Invoke(idx, value);
            }
        }
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

    void OnChangeAnimationType() => _useAnimation = AnimationType is EDropdownAnimation.Fade or EDropdownAnimation.Resize;

    void AddEventsListeners()
    {
        OnSelectItem += OnSelectedItem_handler;
        OnMultipleSelectItem += OnMultipleSelectItem_handler;
        LocalizationSettings.SelectedLocaleChanged += OnChangeLanguage_handler;
    }

    void RemoveEventsListeners()
    {
        OnSelectItem -= OnSelectedItem_handler;
        OnMultipleSelectItem -= OnMultipleSelectItem_handler;
        LocalizationSettings.SelectedLocaleChanged -= OnChangeLanguage_handler;
    }

    void CreateItem(DropdownOption optionData)
    {
        var item = Instantiate(ItemTemplate, ItemParent);

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

        Panel.RemoveSelectable(itemToDestroy.Toggle);
        _items.Remove(itemToDestroy);

        Destroy(itemToDestroy);
    }

    void ShowHidePanel(BaseEventData eventData)
    {
        //Check if was clicked TargetGraphic, to prevent action when click on child Selectable
        if (eventData.selectedObject != _selectable.gameObject) return;

        if (_isDropdownShown)
            Hide();
        else
            Show();
    }

    void RefreshCaption()
    {
        var data = new DropdownOption();

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
            {
                var selectedItem = _items.FirstOrDefault(item => item.Toggle.isOn);

                // If None item selected and don't exist "NoneItem", select first item as default
                if (!selectedItem)
                {
                    ToggleItem(0, true);
                    return;
                }

                data = ObservableOptions.First(option => option.GUID.Equals(_selectedItem.GUID));
            }
        }

        Caption.UpdateContent(data);
        SetNoneAndEverythingItems();
    }

    void SetNoneAndEverythingItems()
    {
        var selectedItemsCount = _items.Count(item => item.Toggle.isOn);

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
        if (!EnableEverythingItem) return;
        _everythingItem.Toggle.SetIsOnWithoutNotify(value);
    }

    void SetItemsOrderAndNavigation()
    {
        SetOrderForExtraItems();
        SetItemsNavigation();
        return;

        void SetOrderForExtraItems()
        {
            var lastChildIdx = ItemParent.childCount - 1;

            if (_noneItem)
                _noneItem.transform.SetSiblingIndex(0);

            if (_everythingItem)
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
                toggleNavigation.selectOnUp = _items.Count > 0 ? _items[^1].Toggle : _noneItem ? _noneItem.Toggle : _selectable;
                _everythingItem.Toggle.navigation = toggleNavigation;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var toggleNavigation = item.Toggle.navigation;
                toggleNavigation.selectOnUp = (i - 1) >= 0 ? _items[i - 1].Toggle : _noneItem ? _noneItem.Toggle : _selectable;
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

    void OnToggleItem_handler(DropdownInfoItem selectedItem, bool isOn)
    {
        if (MultiSelect)
        {
            var idx = _items.IndexOf(selectedItem);
            OnMultipleSelectItem?.Invoke(idx, isOn);
        }
        else
        {
            var sameItem = _selectedItem && _selectedItem.GUID.Equals(selectedItem.GUID);

            if (isOn)
            {
                if (_selectedItem && !sameItem)
                    _selectedItem.Toggle.SetIsOnWithoutNotify(false);

                _selectedItem = selectedItem;
                OnSelectItem?.Invoke(_items.IndexOf(selectedItem));
            }
            else if (sameItem)
                selectedItem.Toggle.SetIsOnWithoutNotify(true);

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

        SetItemsOrderAndNavigation();
        RefreshCaption();
    }

#endregion
}
}