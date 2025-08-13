using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
/// <summary>
/// Managing splash screen operations in the application. <br />
/// <b>Usage:</b> Register splash screens with unique keys, show a specific splash screen, and hide the current splash screen.
/// </summary>
public interface IServiceSplashScreen
{
    void RegisterSplashScreen(string key, ISplashScreen splashScreen);
    public UniTask ShowPage(string key, bool skipAnimation);
    public UniTask HidePage();
}
}