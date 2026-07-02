using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Helpers.UI
{
[RequireComponent(typeof(ScrollRect))]
public class NavigationScroll : MonoBehaviour
{
#region Fields

    [SerializeField] List<Selectable> _selectables;
    
    float _scrollTop;
    float _scrollBottom;

    ScrollRect _scrollRect => scrollRect ??= GetComponent<ScrollRect>();
    ScrollRect scrollRect;
    readonly CompositeDisposable _disposable = new();

#endregion

#region Monobeh

    void Awake()
    {
        foreach (var selectable in _selectables)
            AddSelectListener(selectable);
    }

    void OnEnable()
    {
        var scrollRT = _scrollRect.GetComponent<RectTransform>();
        _scrollTop = scrollRT.position.y;
        _scrollBottom = _scrollTop - scrollRT.rect.height;
    }

    void OnDestroy() => _disposable.Dispose();

#endregion

#region Public methods

    public void AddSelectable(params Selectable[] selectables)
    {
        foreach (var selectable in selectables)
        {
            if (_selectables.Contains(selectable)) continue;
            
            _selectables.Add(selectable);
            AddSelectListener(selectable);
        }
    }

    public void RemoveSelectables(Selectable[] selectables)
    {
        foreach (var selectable in selectables)
            if (_selectables.Contains(selectable))
                _selectables.Remove(selectable);
    }

    public void ResetContentPos() => _scrollRect.content.anchoredPosition = Vector2.zero;

#endregion

#region Private methods

    void AddSelectListener(Selectable selectable)
    {
        selectable.OnSelectAsObservable()
                  .Subscribe(OnSelect)
                  .AddTo(_disposable);
    }
    
    /// <summary>
    /// This handles keyboard/gamepad navigation inside a Unity UI
    /// </summary>
    void OnSelect(BaseEventData eventData)
    {
        if(eventData is AxisEventData axisEventData)
        {
            var selectableRT = eventData.selectedObject.GetComponent<RectTransform>();
            var posTop = selectableRT.position.y + selectableRT.rect.height / 2;
            var posBottom = posTop - selectableRT.rect.height;
            var differenceTop = _scrollTop - posTop;
            var differenceBottom = posBottom - _scrollBottom;
            
            if (axisEventData.moveVector.y > 0 && differenceTop < 0)
            {
                Debug.Log($"++++++");
                _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, _scrollRect.content.anchoredPosition.y + differenceTop);
            }
            else if (axisEventData.moveVector.y < 0 && differenceBottom < 0)
            {
                Debug.Log($"------");
                _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, _scrollRect.content.anchoredPosition.y - differenceBottom);
            }
        }
    }

#endregion
    
}
}