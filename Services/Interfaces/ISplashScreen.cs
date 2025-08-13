using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISplashScreen
{
    public UniTask ShowPanel(bool skipAnimation);
    public UniTask HidePanel();
}
}
