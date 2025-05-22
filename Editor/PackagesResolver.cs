using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Helpers.Editor
{
[InitializeOnLoad]
public static class PackagesResolver
{
#region Fields

    struct PackageInfo
    {
        public string Name;
        public string Url;
    }

    static readonly string _manifestPath = "Packages/manifest.json";
    static JObject _manifest;
    static JObject _dependencies;

    static PackageInfo[] _packages = new[]
    {
        new PackageInfo { Name = "com.innogames.asset-relations-viewer", Url = "https://github.com/innogames/asset-relations-viewer.git" },
        new PackageInfo { Name = "com.dbrizov.naughtyattributes", Url = "https://github.com/dbrizov/NaughtyAttributes.git#upm" },
        new PackageInfo { Name = "com.github-glitchenzo.nugetforunity", Url = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity" },
        new PackageInfo { Name = "com.cysharp.unitask", Url = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
        new PackageInfo { Name = "com.cysharp.memorypack", Url = "https://github.com/Cysharp/MemoryPack.git?path=src/MemoryPack.Unity/Assets/MemoryPack.Unity" },
        new PackageInfo { Name = "com.cysharp.r3", Url = "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity" },
    };

#endregion

    static PackagesResolver()
    {
        AddExternalPackages();
        AddNugetPackags();
    }

    [MenuItem("Tools/Helpers/Resolver_Packages")]
    public static void AddExternalPackages()
    {
        if (!GetManifest() || !GetDependencies()) return;

        foreach (var package in _packages)
            AddPackage(package);
        AssetDatabase.Refresh();

        DefineSymbolManager.AddDefineSymbol("NUGET_INSTALLED");
        AssetDatabase.Refresh();

        void AddPackage(PackageInfo packageInfo)
        {
            if (_dependencies.ContainsKey(packageInfo.Name)) return;

            _dependencies[packageInfo.Name] = packageInfo.Url;
            File.WriteAllText(_manifestPath, _manifest.ToString());
            Debug.LogWarning($"[GitPackagesResolver]: Installed Package: {packageInfo.Name}");
        }
    }

    [MenuItem("Tools/Helpers/Resolver_Nuget")]
    public static void AddNugetPackags()
    {
#if NUGET_INSTALLED
        NugetForUnity.NugetPackageInstaller.InstallIdentifier(new NugetForUnity.Models.NugetPackageIdentifier("MemoryPack", null));
        NugetForUnity.NugetPackageInstaller.InstallIdentifier(new NugetForUnity.Models.NugetPackageIdentifier("R3", null));
        NugetForUnity.NugetPackageInstaller.InstallIdentifier(new NugetForUnity.Models.NugetPackageIdentifier("ObservableCollections", null));

        AssetDatabase.Refresh();
#endif
    }

    static bool GetManifest()
    {
        if (_manifest != null) return true;
            
        if (!File.Exists(_manifestPath))
        {
            Debug.LogError($"[GitPackagesResolver]: No manifest at: + {_manifestPath}");
            return false;
        }

        _manifest = JObject.Parse(File.ReadAllText(_manifestPath));
        return true;
    }

    static bool GetDependencies()
    {
        if (_dependencies != null) return true;
        
        _dependencies = _manifest["dependencies"] as JObject;
        if (_dependencies == null)
        {
            Debug.LogError($"[GitPackagesResolver]: No Dependencies  in manifest.json");
            return false;
        }

        return true;
    }

}
}