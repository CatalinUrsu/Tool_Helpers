using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IState
{
    StateMachine StateMachine { get; set; }
    UniTask Exit();
}
}