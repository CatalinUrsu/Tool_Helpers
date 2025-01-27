using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Helpers.UI
{
public class DropdownItem : DropdownElementBase, ICancelHandler
{
#region Fields

    [field: SerializeField] public Toggle Toggle { get; set; }

    public Guid GUID { get; private set; }
    
    ICancelHandler  _dropdownCancelHandler;

#endregion
    
    public void OnCancel(BaseEventData eventData) => _dropdownCancelHandler.OnCancel(eventData);

    public void Init(ICancelHandler parentCancelHandler,DropdownOption data)
    {
        _dropdownCancelHandler = parentCancelHandler;
        
        GUID = data.GUID;
        gameObject.SetActive(true);

        UpdateContent(data);
    }
}
}