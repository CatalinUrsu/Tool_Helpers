using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Helpers
{
public class ButtonHelper : Button
{
    public event Action OnTouchDown;
    public event Action OnTouchUp;
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnTouchDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnTouchUp?.Invoke();
    }
}
}