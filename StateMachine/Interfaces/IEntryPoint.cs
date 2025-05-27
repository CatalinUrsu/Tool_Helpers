using System;
using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IEntryPoint
{
    UniTask Init(StatesMachine statesMachine, Action<float> onUpdateProgress = null);
    UniTask Enter();
    UniTask Exit();
}
}