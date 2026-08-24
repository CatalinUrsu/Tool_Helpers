using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISceneContext
{
    UniTask DeInit();
    UniTask Init(IProgressTrackingService progressTrackingService, SceneLoadProgress sceneLoadProgress);
}
}