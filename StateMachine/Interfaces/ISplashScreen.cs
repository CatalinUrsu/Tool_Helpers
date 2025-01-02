using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface ISplashScreen<T> where T : ISplashScreenInfo
{
    public UniTask ShowPanel(T config);
    public UniTask HidePanel();
}
}