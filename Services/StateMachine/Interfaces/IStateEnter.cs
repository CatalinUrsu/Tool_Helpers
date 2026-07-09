using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IStateEnter : IState
{
    UniTask Enter();
}
}