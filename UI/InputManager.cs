using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Helpers
{
public class InputManager : Singleton<InputManager>
{
#region Fields

    [SerializeField] Canvas _inputBlockCanvas;

    int _lockCount;
    static readonly List<RaycastResult> _results = new(4);
    static readonly PointerEventData _pointerEventData = new(EventSystem.current);

    bool InputEnable
    {
        set
        {
            _lockCount = !value ? _lockCount + 1 : Mathf.Max(0, _lockCount - 1);
            bool inputIsLocked = _lockCount != 0;

            _inputBlockCanvas.gameObject.SetActive(inputIsLocked);

            if (inputIsLocked)
                OnToggleInputLock?.Invoke(true);
            else
                OnToggleInputLock?.Invoke(false);

            Debug.Log($"[InputManager] --- <b>{(!value ? "Lock" : "Unlock")}</b> request | system is <b>{!inputIsLocked}</b>");
        }
    }

    public event Action<bool> OnToggleInputLock;

#endregion

#region Public methods

    protected override void Awake()
    {
        base.Awake();
        _inputBlockCanvas.sortingOrder = 50;
    }

    public IDisposable LockInputSystem() => new ReleaseWrapper(this);

    public bool IsPointerOverUI(Vector2 touchPos, GameObject targetGO)
    {
        _results.Clear();
        _pointerEventData.position = touchPos;
        EventSystem.current.RaycastAll(_pointerEventData, _results);

        if (_results.Count == 0)
            return false;

        return (_results[0].gameObject != targetGO);
    }


#endregion

#region Data

    class ReleaseWrapper : IDisposable
    {
        InputManager _inputManager;

        public ReleaseWrapper(InputManager inputManager)
        {
            _inputManager = inputManager;

            _inputManager.InputEnable = false;
        }

        public void Dispose() => _inputManager.InputEnable = true;
    }

#endregion
}
}