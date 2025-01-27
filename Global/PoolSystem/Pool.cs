using System;
using System.Collections.Generic;

namespace Helpers.PoolSystem
{
public class Pool<T>
{
    readonly Func<Action<T>, T> _createFunc;
    readonly Action<T> _onGet;
    readonly Action<T> _onRelease;
    readonly Action<T> _onDestroy;
    readonly int _maxSize;
    protected readonly Queue<T> _pool = new();
    protected readonly List<T> _active = new();

    /// <summary>
    /// Assign values to pool data and preload several amount of items if is neded
    /// </summary>
    /// <param name="createFunc">Create item function, <b>Action<T></b> is Release method, that give posibility to pooled object to return itself to pool </param>
    /// <param name="maxSize">If queue reach maxSize, that released item will be destroyed instead of enque</param>
    public Pool(Func<Action<T>, T> createFunc,
                Action<T> onGet = null,
                Action<T> onRelease = null,
                Action<T> onDestroy = null,
                int preloadCount = 0,
                int maxSize = 20)
    {
        if (maxSize <= 0)
            throw new ArgumentException("Max Size must be greater than 0", nameof (maxSize));
        
        _createFunc = createFunc;
        _onGet = onGet;
        _onRelease = onRelease;
        _onDestroy = onDestroy;
        _maxSize = maxSize;

        Preload(preloadCount);
    }

    public virtual T Get()
    {
        var item = _pool.Count > 0 ? _pool.Dequeue() : _createFunc.Invoke(Release);
        _active.Add(item);
        _onGet?.Invoke(item);

        return item;
    }

    public virtual void Release(T item)
    {
        if (_pool.Contains(item)) return;

        if (_pool.Count >= _maxSize)
            _onDestroy?.Invoke(item);
        else
        {
            _onRelease?.Invoke(item);
            _pool.Enqueue(item);
            _active.Remove(item);
        }
    }

    public void ReleaseAll() => _active.ToArray().ForEach(item => Release(item));

    public void Clear()
    {
        ReleaseAll();
        _pool.ForEach(item => _onDestroy(item));

        _active.Clear();
        _pool.Clear();
    }

    void Preload(int preloadCount)
    {
        for (int i = 0; i < preloadCount; i++)
            _pool.Enqueue(_createFunc(Release));
    }
}
}