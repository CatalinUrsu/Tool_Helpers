using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI
{
public class DropdownElementBase : MonoBehaviour
{
    [SerializeField] public TMP_Text Text;
    [SerializeField] protected Image _img;

    public void UpdateContent(DropdownOption data)
    {
        var sprite = data.Sprite;
        
        if (sprite != null)
        {
            _img.sprite = sprite;
            _img.enabled = sprite != null;
        }
        else
            Text.SetText(data.Txt);
    }
}
}