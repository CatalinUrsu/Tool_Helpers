using R3;
using System;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI
{
[AddComponentMenu("UI (Canvas)/Helpers/Button Helper")]
[RequireComponent(typeof(Button))]
public class ButtonHelper : MonoBehaviour
{
    public Button Btn { get; private set; }
    public RectTransform RT { get; private set; }

    public event Action OnPointerDown;
    public event Action OnPointerUp;

    protected void OnValidate()
    {
        Btn ??= GetComponent<Button>();
        RT ??= GetComponent<RectTransform>();
    }

    [ContextMenu("Init")]
    public virtual void Init()
    {
        Btn.OnPointerDownAsObservable()
           .Subscribe(_ => OnPointerDown_raise())
           .AddTo(this);

        Btn.OnPointerUpAsObservable()
           .Subscribe(_ => OnPointerUp_raise())
           .AddTo(this);
    }

    void OnPointerDown_raise()
    {
        if(!IsInteractable()) return;
        
        OnPointerDown?.Invoke();
    }

    void OnPointerUp_raise()
    {
        if(!IsInteractable()) return;
        
        OnPointerUp?.Invoke();
    }
    
    bool IsInteractable() => Btn.interactable;
}
}