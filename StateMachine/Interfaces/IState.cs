using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IState
{
    StatesMachine StatesMachine { get; set; }
    UniTask Exit();
}
}