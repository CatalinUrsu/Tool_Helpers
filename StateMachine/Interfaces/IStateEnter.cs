using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IStateEnter : IState
{
    UniTask Enter();
}
}