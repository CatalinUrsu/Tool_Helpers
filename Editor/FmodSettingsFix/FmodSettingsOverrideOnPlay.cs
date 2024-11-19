#if UNITY_EDITOR
using FMODUnity;
using UnityEditor.AddressableAssets;

namespace Helpers
{
public class FmodSettingsOverrideOnPlay : Singleton<FmodSettingsOverrideOnPlay>
{
    protected override void Awake()
    {
        base.Awake();
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            Settings.Instance.ImportType = ImportType.StreamingAssets;
    }

    void OnDestroy() => Settings.Instance.ImportType = ImportType.AssetBundle;
}
}
#endif
