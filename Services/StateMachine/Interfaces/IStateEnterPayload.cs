using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IStateEnterPayload<in TPayload> : IState
{
    UniTask Enter(TPayload payload);
}
}