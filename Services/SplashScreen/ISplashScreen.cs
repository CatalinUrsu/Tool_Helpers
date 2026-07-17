using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISplashScreen
{
    public UniTask Show(bool skipAnimation = false);
    public UniTask Hide();
}
}
