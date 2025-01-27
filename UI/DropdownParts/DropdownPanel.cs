using Helpers.UI;
using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;
#endif

namespace Helpers
{
[RequireComponent(typeof(RectTransform), typeof(ScrollRect), typeof(NavigationScroll))]
public class DropdownPanel : MonoBehaviour
{
#region Fields

    NavigationScroll _navigationScroll;
    CanvasGroup _canvasGroup;
    CanvasGroup _cg;
    RectTransform _rt;
    Vector2 _sizeDelta;

#if DOTWEEN
    Tween _tween;
#endif

#endregion

#region Monobeh

    [ExecuteAlways]
    void Awake()
    {
        _navigationScroll = GetComponent<NavigationScroll>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
        _sizeDelta = _rt.sizeDelta;
        _tween.SetUpdate(true);
    }

    void OnDestroy() => _tween.CheckAndEnd();

#endregion

#region Public methods

    public void Init()
    {
        _rt = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void AddSelectable(params Selectable[] selectables) => _navigationScroll.AddSelectable(selectables);

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