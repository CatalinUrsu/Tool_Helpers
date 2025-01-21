using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI
{
public class DropdownCaption : DropdownElementBase
{
    [field: SerializeField] public Image ImgArrow;
    
    public void SetArrow(bool show)
    {
        //TODO: (cat) add animation
        if (ImgArrow)
            ImgArrow.enabled = show;
    }
}
}