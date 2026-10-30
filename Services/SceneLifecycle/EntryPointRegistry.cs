using System;
using System.Collections.Generic;

namespace Helpers.Services
{
/// <summary>
/// Entry points of loaded scenes by type. Scene registers its entry point on load and removes it on unload.
/// </summary>
public class EntryPointRegistry
{
    readonly Dictionary<Type, IEntryPoint> _entryPoints = new();

    public void Register<T>(T entryPoint) where T : class, IEntryPoint
    {
        if (!_entryPoints.TryAdd(typeof(T), entryPoint))
            throw new InvalidOperationException($"{typeof(T).Name} is already registered. Previous scene is not unloaded?");
    }

    public void Unregister<T>(T entryPoint) where T : class, IEntryPoint
    {
        if (_entryPoints.TryGetValue(typeof(T), out var current) && current == entryPoint)
            _entryPoints.Remove(typeof(T));
    }

    public T Get<T>() where T : class, IEntryPoint =>
        _entryPoints.TryGetValue(typeof(T), out var entryPoint)
            ? (T)entryPoint
            : throw new InvalidOperationException($"{typeof(T).Name} is not registered. Scene is not loaded?");
}
}
