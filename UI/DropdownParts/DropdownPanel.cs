using Helpers.UI;
using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;
#endif

namespace Helpers
{
[RequireComponent(typeof(RectTransform), typeof(ScrollRect))]
public class DropdownPanel : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup _layoutGroup;

    RectTransform _rt;

#if DOTWEEN
    Tween _tween;
#endif

    public void Init()
    {
        _rt = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void Show(EDropdownAnimation anim)
    {
        if (anim == EDropdownAnimation.None)
            gameObject.SetActive(true);
        //TODO: (cat) add animation
    }

    public void Hide(EDropdownAnimation anim)
    {
        if (anim == EDropdownAnimation.None)
            gameObject.SetActive(true);
        //TODO: (cat) add animation
    }
}
}