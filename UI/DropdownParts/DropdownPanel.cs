using System;
using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;
#endif

namespace Helpers.UI
{
[RequireComponent(typeof(RectTransform), typeof(ScrollRect), typeof(NavigationScroll))]
public class DropdownPanel : MonoBehaviour
{
#region Fields

    [SerializeField] NavigationScroll _navigationScroll;
    
    CanvasGroup _canvasGroup;
    CanvasGroup _cg;
    RectTransform _rt;
    Vector2 _sizeDelta;

#if DOTWEEN
    Tween _tween;
#endif

#endregion

#region Monobeh

    void OnValidate()
    {
        _navigationScroll ??= GetComponent<NavigationScroll>();
    }

    [ExecuteAlways]
    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
        _sizeDelta = _rt.sizeDelta;
#if DOTWEEN
        _tween.SetUpdate(true);
#endif
    }

#if DOTWEEN
    void OnDestroy() => _tween.CheckAndEnd();
#endif

#endregion

#region Public methods

    public void Init()
    {
        _rt = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void AddSelectable(params Selectable[] selectables) => _navigationScroll.AddSelectable(selectables);
    
    public void RemoveSelectable(params Selectable[] selectables) => _navigationScroll.RemoveSelectables(selectables);

    public void Show(EDropdownAnimation anim, float duration)
    {
#if DOTWEEN
        _tween.CheckAndEnd(false);
        
        switch (anim)
        {
            case EDropdownAnimation.None:
                gameObject.SetActive(true);
                break;
            case EDropdownAnimation.Fade:
                _tween = _canvasGroup.DOFade(1, duration)
                                     .OnStart(() => gameObject.SetActive(true));
                break;
            case EDropdownAnimation.Resize:
                _tween = _rt.DOSizeDelta(_sizeDelta, duration)
                            .OnStart(() => gameObject.SetActive(true));
                break;
        }
#else
            gameObject.SetActive(true);
#endif
    }

    public void Hide(EDropdownAnimation anim, float duration)
    {
#if DOTWEEN
        _tween.CheckAndEnd(false);

        switch (anim)
        {
            case EDropdownAnimation.None:
                gameObject.SetActive(false);
                break;
            case EDropdownAnimation.Fade:
                _tween = _canvasGroup.DOFade(0, duration)
                                     .OnComplete(OnFinishHide);
                break;
            case EDropdownAnimation.Resize:
                _tween = _rt.DOSizeDelta(Vector2.zero, duration)
                            .OnComplete(OnFinishHide);
                break;
        }
#else
            gameObject.SetActive(false);
#endif
    }

#endregion

    void OnFinishHide()
    {
        gameObject.SetActive(false);
        _navigationScroll.ResetContentPos();
    }
}
}