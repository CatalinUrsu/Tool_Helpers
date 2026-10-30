using System;

namespace Helpers
{
public interface IInputLock
{
    event Action<bool> OnToggleInputLock;

    /// <summary>
    /// Locks input until returned handle is disposed. Locks are counted, input unlocks after the last one.
    /// </summary>
    IDisposable LockInputSystem();
}
}
