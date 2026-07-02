using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI
{
public class DropdownInfoBase : MonoBehaviour
{
    [SerializeField] public TMP_Text Text;
    [SerializeField] protected Image _img;

    public void UpdateContent(DropdownOption data)
    {
        var sprite = data.Sprite;

        if (_img != null && sprite != null)
        {
            _img.sprite = sprite;
            _img.enabled = sprite != null;
        }

        if (Text != null)
            Text.SetText(data.Txt);
    }
}
}