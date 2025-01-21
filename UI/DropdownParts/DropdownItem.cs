using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Helpers.UI
{
public class DropdownItem : DropdownElementBase, IPointerEnterHandler
{
    [field: SerializeField] public Toggle Toggle { get; set; }

    public Guid GUID { get; private set; }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!gameObject.activeInHierarchy || Application.isPlaying)
            return;

        if (Toggle) return;
        Toggle = GetComponent<Toggle>();
    }
#endif

    public void Init(DropdownOption data)
    {
        GUID = data.GUID;
        gameObject.SetActive(true);
        gameObject.name = $"Item_{transform.GetSiblingIndex()}";

        UpdateContent(data);
    }

    public virtual void OnPointerEnter(PointerEventData eventData) => EventSystem.current.SetSelectedGameObject(gameObject);
}
}