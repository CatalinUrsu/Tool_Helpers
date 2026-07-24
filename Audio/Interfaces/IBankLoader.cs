using Cysharp.Threading.Tasks;

namespace Helpers.Audio
{
public interface IBankLoader
{
    UniTask Init();
    void Deinit();
}
}