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

    static JObject _manifest;
    static JObject _dependencies;
    static readonly string _manifestPath = "Packages/manifest.json";
    static readonly PackageInfo[] _packages = {
        new() { Name = "com.innogames.asset-relations-viewer", Url = "https://github.com/innogames/asset-relations-viewer.git" },
        new() { Name = "com.github-glitchenzo.nugetforunity", Url = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity" },
        new() { Name = "com.svermeulen.extenject", Url = "https://github.com/Mathijs-Bakker/Extenject.git?path=UnityProject/Assets/Plugins/Zenject/Source" },
        new() { Name = "com.cysharp.unitask", Url = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
        new() { Name = "com.cysharp.r3", Url = "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity" },
    };

#endregion

#region Public methods

    static PackagesResolver()
    {
        AddExternalPackages();
        AddNugetPackags();
    }

    [MenuItem("Tools/Helpers/Resolver_Packages",false,1)]
    public static void AddExternalPackages()
    {
        if (!GetManifest() || !GetDependencies()) return;

        foreach (var package in _packages)
        {
            if (!_dependencies.ContainsKey(package.Name))
            {
                _dependencies[package.Name] = package.Url;
                Debug.LogWarning($"[GitPackagesResolver]: Installed Package: {package.Name}");
            }
        }

        File.WriteAllText(_manifestPath, _manifest.ToString());
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Helpers/Resolver_Nuget",false,1)]
    public static void AddNugetPackags()
    {
        DefineSymbolManager.AddDefineSymbol("NUGET_INSTALLED");
        AssetDatabase.Refresh();
        
#if NUGET_INSTALLED
        NugetForUnity.NugetPackageInstaller.InstallIdentifier(new NugetForUnity.Models.NugetPackageIdentifier("R3", null));
        NugetForUnity.NugetPackageInstaller.InstallIdentifier(new NugetForUnity.Models.NugetPackageIdentifier("ObservableCollections", null));

        AssetDatabase.Refresh();
#endif
    }

#endregion

#region Private methods

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

#endregion
}
}