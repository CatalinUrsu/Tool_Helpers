using UnityEngine;

namespace Helpers.UI
{
public class DropdownInfoCaption : DropdownInfoBase
{
    [Space]
    [SerializeField] RectTransform _arrowRT;

    readonly Vector3 _showScale = new(1, -1, 1);

    public virtual void SetArrow(bool show)
    {
        if (_arrowRT)
            _arrowRT.localScale = show ? _showScale : Vector3.one;
    }
}
}