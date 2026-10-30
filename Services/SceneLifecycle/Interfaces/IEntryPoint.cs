using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IEntryPoint
{
    UniTask DeInit();
    UniTask Init(IProgressTrackingService progressTrackingService, SceneLoadProgress sceneLoadProgress);
}
}