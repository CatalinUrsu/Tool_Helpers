using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

namespace Helpers.Editor.Bootstrap
{
[InitializeOnLoad]
    public static class PackagesResolver
    {
#region Fields

        const string MANIFEST_PATH = "Packages/manifest.json";
        const string DEPENDENCIES_KEYWORD = "\"dependencies\": {";
        
        static readonly (string Name, string Url)[] _packages = {
            ("com.innogames.asset-relations-viewer", "https://github.com/innogames/asset-relations-viewer.git"),
            ("com.github-glitchenzo.nugetforunity", "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity"),
            ("com.svermeulen.extenject", "https://github.com/Mathijs-Bakker/Extenject.git?path=UnityProject/Assets/Plugins/Zenject/Source"),
            ("com.cysharp.unitask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"),
            ("com.cysharp.r3", "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity")
        };

#endregion

#region Public methods

        // Delay execution until the editor is fully initialized
        static PackagesResolver() => EditorApplication.delayCall += ResolveAll;

        [MenuItem("Tools/Helpers/Wizard/Resolve All Packages", false, 1)]
        public static void ResolveAll()
        {
            var upmChanged = AddExternalPackages();
            
            // Install NuGet packages if no other packages added to manifest
            if (upmChanged) return;
            NugetPackagesResolver.AddNugetPackages();
        }

        public static bool AddExternalPackages()
        {
            if (!File.Exists(MANIFEST_PATH))
            {
                Debug.LogError($"[GitPackagesResolver]: Error — file not found: {MANIFEST_PATH}");
                return false;
            }

            var manifest = File.ReadAllText(MANIFEST_PATH);
            var isModified = false;
            var insertIndex = manifest.IndexOf(DEPENDENCIES_KEYWORD);
            var insertions = "\n";

            // Find the start of the dependencies block
            if (insertIndex == -1) 
            {
                Debug.LogError("[GitPackagesResolver]: Invalid manifest.json format");
                return false;
            }
            
            insertIndex += DEPENDENCIES_KEYWORD.Length;

            foreach (var pkg in _packages)
            {
                if (manifest.Contains($"\"{pkg.Name}\"")) continue;
                
                insertions += $"    \"{pkg.Name}\": \"{pkg.Url}\",\n";
                isModified = true;
                Debug.Log($"[GitPackagesResolver]: Added Git package: {pkg.Name}");
            }

            if (isModified)
            {
                manifest = manifest.Insert(insertIndex, insertions);
                File.WriteAllText(MANIFEST_PATH, manifest);
                Client.Resolve(); 
                return true;
            }

            return false;
        }
        
#endregion
    }
}