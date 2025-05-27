using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface ISplashScreen
{
    public UniTask ShowPanel<T>(T config) where T : ISplashScreenInfo;
    public UniTask HidePanel();
}
}
