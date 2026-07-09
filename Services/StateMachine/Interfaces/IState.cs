using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IState
{
    StatesMachine StatesMachine { get; set; }
    UniTask Exit();
}
}