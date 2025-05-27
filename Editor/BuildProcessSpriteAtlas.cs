using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Helpers.Editor
{
/// <summary>
/// When loading the sprite atlases via Addressable, we should avoid them getting build into the app.
/// Otherwise, the sprite atlases will get loaded into memory twice.
/// </summary>
public class BuildProcessSpriteAtlas : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get; }

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("[BuildProcessSpriteAtlas]: Set the `IncludeInBuild` flag to `false` for all Addressable SpriteAtlases before start the App build.");
        SpriteAtlasUtils.SetAllIncludeInBuild(false);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("[BuildProcessSpriteAtlas]: Set the `IncludeInBuild` flag to `true` for all Addressable SpriteAtlases after finishing the App build.");
        SpriteAtlasUtils.SetAllIncludeInBuild(true);
    }
}
}