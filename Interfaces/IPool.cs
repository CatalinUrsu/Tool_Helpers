namespace Helpers.PoolSystem
{
public interface IPool<T>
{
    T Get();
    void Release(T item);
    void ReleaseAll();
    void Clear();
}
}