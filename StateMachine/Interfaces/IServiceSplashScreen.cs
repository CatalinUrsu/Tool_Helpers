using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IServiceSplashScreen
{
    void RegisterSplashScreen<T>(ISplashScreen<T> panel) where T : ISplashScreenInfo;
    public UniTask ShowPage<T>(T config) where T : ISplashScreenInfo;
    public UniTask HidePage();
}
}