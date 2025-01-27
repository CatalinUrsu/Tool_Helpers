using UnityEngine;

namespace Helpers.UI
{
public class DropdownCaption : DropdownElementBase
{
    [SerializeField] RectTransform _arrowRT;

    Vector3 _showScale = new Vector3(1, -1, 1);

    public void SetArrow(bool show)
    {
        if (_arrowRT)
            _arrowRT.localScale = show ? _showScale : Vector3.one;
    }
}
}