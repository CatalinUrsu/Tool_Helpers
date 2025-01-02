using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IStateEnterPayload<in TPayload> : IState
{
    UniTaskVoid Enter(TPayload payload);
}
}