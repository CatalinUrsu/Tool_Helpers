using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IStateEnterPayload<in TPayload> : IState
{
    UniTask Enter(TPayload payload);
}
}