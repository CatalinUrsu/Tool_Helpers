using R3;
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
//TODO: (cat) Custom Editor
public class DropdownHelper : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
#region Fields

    //TODO: (cat) Attributes: Require, onlyChildren,
    [SerializeField] DropdownCaption _caption;

    [Header("Elements")]
    //TODO: (cat) Attributes: Require, onlyChildren,
    [SerializeField] DropdownPanel _panel;
    //TODO: (cat) Attributes: Require, onlyChildren,
    [SerializeField] Transform _itemParent;
    //TODO: (cat) Attributes: Require !     Check if is prefab and remove
    [SerializeField] DropdownItem _itemTemplate;

    [Header("Properties")] 
    [SerializeField] SerializableReactiveProperty<int> _selectedItemIdx = new SerializableReactiveProperty<int>();
    [SerializeField] bool _multiSelect;

    [Header("Animation")] 
    [SerializeField] float AlphaFadeSpeed = 0.15f;
    [SerializeField] EDropdownAnimation _animation;

    [Header("Options")]
    //TODO: (cat) Attributes: Require !
    [SerializeField] bool _addDeselectAllOption;
    [SerializeField] DropdownOption _nothingOption;
    [SerializeField] DropdownOption _mixedOption;
    [SerializeField] DropdownOption _everythingOption;
    [SerializeField] List<DropdownOption> _options = new List<DropdownOption>();

    public event Action<DropdownOption> OnValueForCaptionChanged;
    public event Action<int> OnToggleItem2;

    public DropdownCaption Caption { get { return _caption; } set { _caption = value; } }
    public DropdownPanel Panel { get { return _panel; } set { _panel = value; } }
    public Transform ItemParent { get { return _itemParent; } set { _itemParent = value; } }
    public DropdownItem ItemTemplate { get { return _itemTemplate; } set { _itemTemplate = value; } }
    public int SelectedItemIdx { get { return _selectedItemIdx.Value; } set { _selectedItemIdx.Value = value; } }
    public bool MultiSelect { get { return _multiSelect; } set { _multiSelect = value; } }
    public EDropdownAnimation Animation { get { return _animation; } set { _animation = value; } }
    public ObservableList<DropdownOption> ObservableOptions = new ObservableList<DropdownOption>();

    bool _isDropdownShown;
    DropdownItem _deselectAllItem;
    List<DropdownItem> _items = new List<DropdownItem>();
    IDisposable _disposable = Disposable.Empty;

#endregion

#region Monobeh

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        Asserts();
        CheckAndAddDeselectAll();
        SetReactiveProperties();
        AddOptions(_options);

        _itemTemplate.gameObject.SetActive(false);
        _panel.gameObject.SetActive(false);
        LocalizationSettings.SelectedLocaleChanged += OnChangeLanguage_handler;

        void Asserts()
        {
            Assert.IsNotNull(_caption, "Caption is not set");
            Assert.IsNotNull(_panel, "OptionsPanel is not set");
            Assert.IsNotNull(_itemParent, "ItemParent is not set");
            Assert.IsNotNull(_itemTemplate, "DropdownItem is not set");
        }

        void SetReactiveProperties()
        {
            var db = Disposable.CreateBuilder();
            var optionsView = ObservableOptions.CreateView(_ => _).AddTo(ref db);
            _selectedItemIdx.Subscribe(OnSelectedItemidxChanged_handler).AddTo(ref db);
            _disposable = db.Build();

            optionsView.ViewChanged += OnOptionsCollectionChanged_handler;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Hide();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _disposable.Dispose();
        LocalizationSettings.SelectedLocaleChanged -= OnChangeLanguage_handler;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!IsActive() || Application.isPlaying)
            return;

        var selectedIdx = Mathf.Clamp(_selectedItemIdx.Value, -1, _options.Count - 1);
        var selectedOption = selectedIdx == -1 ? _nothingOption : _options[selectedIdx];

        if (_caption)
            _caption.UpdateContent(selectedOption);
    }
#endif

#endregion

#region Public methods

    public virtual void OnPointerClick(PointerEventData eventData) => Show();

    public virtual void OnSubmit(BaseEventData eventData) => Show();

    public virtual void OnCancel(BaseEventData eventData) => Hide();

    public void AddOptions(IEnumerable<DropdownOption> options) => ObservableOptions.AddRange(options);

    public void ClearOptions()
    {
        ObservableOptions.Clear();
        _selectedItemIdx.Value = -1;
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= ObservableOptions.Count)
            return;

        if (_items[index].Toggle.isOn) return;

        _items[index].Toggle.isOn = true;
    }

    public void DeselectItem(int index)
    {
        if (index < 0 || index >= ObservableOptions.Count)
            return;

        if (!_items[index].Toggle.isOn) return;

        _items[index].Toggle.isOn = false;
    }

    public void DeselectAllItems()
    {
        if (!MultiSelect)
            return;

        foreach (var item in _items)
        {
            if (item.Toggle.isOn)
                item.Toggle.SetIsOnWithoutNotify(false);
        }

        _selectedItemIdx.Value = -1;
    }

    public void RefreshCaption()
    {
        DropdownOption data = _nothingOption;

        if (ObservableOptions.Count > 0 && _selectedItemIdx.Value >= 0)
        {
            if (MultiSelect)
            {
                var selectedItems = _items.Where(item => item.Toggle.isOn).ToArray();
                int selectedItemsCount = selectedItems.Length;

                data = selectedItemsCount switch
                       {
                           0 => _nothingOption,
                           1 => ObservableOptions[_items.IndexOf(selectedItems[0])],
                           _ when selectedItemsCount == _items.Count => _everythingOption,
                           _ => _mixedOption
                       };
            }
            else
                data = ObservableOptions[Mathf.Clamp(_selectedItemIdx.Value, 0, ObservableOptions.Count - 1)];

        }

        _caption.UpdateContent(data);
    }

    public void Show()
    {
        if (_isDropdownShown)
            return;

        _panel.Show(Animation);
        //TODO: (cat) set animation for Caption
        //TODO: (cat) Add navigation for items (for first item navigation up will be caption and reverse)
    }

    public void Hide()
    {
        if (!_isDropdownShown)
            return;

        //TODO: (cat) set animation for Caption
        _panel.Hide(Animation);

        Select();
    }

#endregion

#region Private methods

    void CreateItem(DropdownOption optionData)
    {
        DropdownItem item = Instantiate(_itemTemplate, _itemParent);

        item.Init(optionData);
        item.Toggle.SetIsOnWithoutNotify(optionData.IsOn);
        item.Toggle.onValueChanged.AddListener(value => OnToggleItem_handler(item, value));

        _items.Add(item);
    }

    void DestroyItem(DropdownOption optionData)
    {
        var itemToDestroy = _items.First(dropdownItem => dropdownItem.GUID.Equals(optionData.GUID));

        if (itemToDestroy == null) return;
        _items.Remove(itemToDestroy);

        Destroy(itemToDestroy);
    }

    void CheckAndAddDeselectAll()
    {
        if (!_addDeselectAllOption) return;

        _deselectAllItem = Instantiate(_itemTemplate, _itemParent);

        _deselectAllItem.Init(_nothingOption);
        _deselectAllItem.Toggle.SetIsOnWithoutNotify(true);
        _deselectAllItem.Toggle.onValueChanged.AddListener(OnDeselectAllToggle_handler);

        void OnDeselectAllToggle_handler(bool value)
        {
            if (value)
                DeselectAllItems();
            else
                ToggleDeselectAllItem(true);
        }
    }

    void ToggleDeselectAllItem(bool value)
    {
        if (!_deselectAllItem) return;
        _deselectAllItem.Toggle.SetIsOnWithoutNotify(value);
    }

    void OnToggleItem_handler(DropdownItem selectedItem, bool isOn)
    {
        if (MultiSelect)
        {
            var selectedItemsCount = _items.Where(item => item.Toggle.isOn).ToArray().Length;
            ToggleDeselectAllItem(selectedItemsCount == 0);
            OnSelectedItemidxChanged_handler(_items.IndexOf(selectedItem));
        }
        else
        {
            var sameAsPrevious = _selectedItemIdx.Value >= 0 && _items[_selectedItemIdx.Value].GUID.Equals(selectedItem.GUID);
            if (isOn)
            {
                _items[_selectedItemIdx.Value].Toggle.SetIsOnWithoutNotify(false);
                _selectedItemIdx.Value = _items.IndexOf(selectedItem);

                ToggleDeselectAllItem(false);
            }
            else if (sameAsPrevious)
            {
                if (_deselectAllItem)
                {
                    ToggleDeselectAllItem(true);
                    _selectedItemIdx.Value = -1;
                }
                else
                {
                    selectedItem.Toggle.SetIsOnWithoutNotify(true);
                }
            }

            Hide();
        }
    }

    void OnChangeLanguage_handler(Locale obj)
    {
        if (_deselectAllItem)
            _deselectAllItem.UpdateContent(_nothingOption);

        for (int i = 0; i < ObservableOptions.Count; i++)
            _items[i].UpdateContent(ObservableOptions[i]);

        RefreshCaption();
    }

    void OnSelectedItemidxChanged_handler(int idx)
    {
        OnToggleItem2?.Invoke(idx);

        RefreshCaption();
    }

    void OnOptionsCollectionChanged_handler(in SynchronizedViewChangedEventArgs<DropdownOption, DropdownOption> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewValues)
                    CreateItem(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldValues)
                    DestroyItem(item);
                break;
        }

        RefreshCaption();
    }

#endregion
}
}