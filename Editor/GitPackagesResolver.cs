using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NugetForUnity;
using Newtonsoft.Json.Linq;
using NugetForUnity.Models;
using System.Collections.Generic;

namespace Helpers.Editor
{
[InitializeOnLoad]
public static class GitPackagesResolver
{
#region Fields

    enum EPackages
    {
        UniRx,
        AssetRelations,
        Nuget,
        MemoryPack,
        UniTask,
        R3
    }

    struct PackageInfo
    {
        public string Name;
        public string Url;
    }

    static readonly string _manifestPath = "Packages/manifest.json";

    static Dictionary<EPackages, PackageInfo> _packagesInfo = new()
    {
        { EPackages.UniRx, new PackageInfo { Name = "com.neuecc.unirx", Url = "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts" } },
        { EPackages.AssetRelations, new PackageInfo { Name = "com.innogames.asset-relations-viewer", Url = "https://github.com/innogames/asset-relations-viewer.git" } },
        { EPackages.Nuget, new PackageInfo { Name = "com.github-glitchenzo.nugetforunity", Url = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity" } },
        { EPackages.UniTask, new PackageInfo { Name = "com.cysharp.unitask", Url = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" } },
        { EPackages.MemoryPack, new PackageInfo { Name = "com.cysharp.memorypack", Url = "https://github.com/Cysharp/MemoryPack.git?path=src/MemoryPack.Unity/Assets/MemoryPack.Unity" } },
        { EPackages.R3, new PackageInfo { Name = "com.cysharp.r3", Url = "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity" } },
    };

#endregion

    static GitPackagesResolver()
    {
        CheckExternalPackages();
        CheckFmodPackage();
    }

#region ExternalPackages

    [MenuItem("Tools/PackageResolver")]
    static void CheckExternalPackages()
    {
        if (!TryGetManifest(out var manifest)) return;
        if (!TryGetDependencies(manifest, out var dependencies)) return;

        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.UniRx]);
        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.AssetRelations]);
        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.Nuget]);
        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.MemoryPack]);
        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.UniTask]);
        AddPackageIfNotExists(manifest, dependencies, _packagesInfo[EPackages.R3]);

        AssetDatabase.Refresh();

        CheckNugetPackages();

        AssetDatabase.Refresh();
    }
    
    static void AddPackageIfNotExists(JObject manifest, JObject dependencies, PackageInfo packageInfo)
    {
        if (dependencies.ContainsKey(packageInfo.Name)) return;

        dependencies[packageInfo.Name] = packageInfo.Url;
        File.WriteAllText(_manifestPath, manifest.ToString());
        Debug.Log($"Added package {packageInfo.Name} to manifest.json.");
    }

    static bool TryGetManifest(out JObject manifest)
    {
        manifest = null;
        if (!File.Exists(_manifestPath))
        {
            Debug.LogError("manifest.json not found at: " + _manifestPath);
            return false;
        }

        manifest = JObject.Parse(File.ReadAllText(_manifestPath));
        return true;
    }

    static bool TryGetDependencies(JObject manifest, out JObject dependencies)
    {
        dependencies = manifest["dependencies"] as JObject;
        if (dependencies == null)
        {
            Debug.LogError("Dependencies not found in manifest.json");
            return false;
        }

        return true;
    }

    static void CheckNugetPackages()
    {
        NugetPackageInstaller.InstallIdentifier(new NugetPackageIdentifier("MemoryPack", null));
        NugetPackageInstaller.InstallIdentifier(new NugetPackageIdentifier("R3", null));
    }

#endregion

#region FMOD

    static void CheckFmodPackage()
    {
        if (!FileAlreadyExist())
            EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        var packagePath = GetFmodPackagePath();

        if (File.Exists(packagePath))
        {
            Debug.Log($"Importing {packagePath}");
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh();
            EditorApplication.update -= OnEditorUpdate;
        }
        else
        {
            Debug.LogError($"Unity package file not found: {packagePath}");
        }
    }

    static string GetFmodPackagePath()
    {
        var scriptPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        var scriptName = scriptPath.Split('\\').Last();

        return scriptPath.Replace(scriptName, "FMOD for Unity.unitypackage");
    }

    static bool FileAlreadyExist() => Directory.Exists(Path.Combine(Application.dataPath, "Plugins", "FMOD"));

#endregion
}
}