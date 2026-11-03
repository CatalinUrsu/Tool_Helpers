using System;
using UnityEngine;

namespace Helpers
{
public interface IInputManager
{
    event Action<bool> OnToggleInputLock;

    bool IsLocked { get; }

    /// <summary>
    /// Locks input until returned handle is disposed. Locks are counted, input unlocks after the last one.
    /// </summary>
    IDisposable Lock();

    /// <summary>
    /// Checks if the pointer is over a UI element, excluding the target GameObject. Returns true if the pointer is over a UI element that is not the target GameObject.
    /// </summary>
    bool IsPointerOverUI(Vector2 touchPos, GameObject targetGO);
}
}
