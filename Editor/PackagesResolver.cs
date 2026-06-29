using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.PackageManager;

namespace Helpers.Editor
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

        static readonly string[] _packagesNuget = {
            "R3",
            "ObservableCollections"
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
            AddNugetPackages();
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

        [MenuItem("Tools/Helpers/Wizard/Resolve Nuget", false, 1)]
        public static void AddNugetPackages()
        {
            if (TryGetNugetAssemblies(out var installerType, out var identifierType))
            {
                OpenRestartDialogue();
                return;
            }

            var installMethod = installerType.GetMethod("InstallIdentifier", BindingFlags.Static | BindingFlags.Public);
            if (installMethod == null) return;
            
            foreach (var packageName in _packagesNuget) 
                InstallNugetPackage(installMethod, identifierType, packageName);
        }
        
#endregion

#region Private methods

        /// <summary>
        /// Use reflection so the script compiles even if NuGetForUnity is not installed yet
        /// </summary>
        /// <returns></returns>
        static bool TryGetNugetAssemblies(out Type installerType, out Type identifierType)
        {
            installerType = AppDomain.CurrentDomain.GetAssemblies()
                                     .Select(assembly => assembly.GetType("NugetForUnity.NugetPackageInstaller"))
                                     .FirstOrDefault(type => type != null);

            identifierType = AppDomain.CurrentDomain.GetAssemblies()
                                      .Select(assembly => assembly.GetType("NugetForUnity.Models.NugetPackageIdentifier"))
                                      .FirstOrDefault(type => type != null);

            return installerType == null || identifierType == null;
        }

        static void OpenRestartDialogue()
        {
            var shouldRestart = EditorUtility.DisplayDialog("Restart Required", // Window title
                                                            "For Nuget packages install, need to restart Unity", // The message body
                                                            "Restart", // Positive button text
                                                            "Cancel"); // Negative button text
            

            if (shouldRestart)
            {
                Debug.Log("[EditorRestarter]: Restarting Unity...");

                // Force save scenes if needed before restart to prevent data loss
                // EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo(); 

                EditorApplication.OpenProject(Environment.CurrentDirectory);
            }
            else
            {
                Debug.Log("[EditorRestarter]: Restart cancelled by the user.");
            }
        }

        static object CreateNugetPackageIdentifier(Type identifierType, string packageName)
        {
            var stringConstructor = identifierType.GetConstructor(new[] { typeof(string) });
            if (stringConstructor != null)
                return stringConstructor.Invoke(new object[] { packageName });

            var twoStringConstructor = identifierType.GetConstructor(new[] { typeof(string), typeof(string) });
            if (twoStringConstructor != null)
                return twoStringConstructor.Invoke(new object[] { packageName, null });


            var constructors = string.Join("\n", identifierType.GetConstructors()
                                                               .Select(ctor =>
                                                               {
                                                                   var parameters = ctor.GetParameters();
                                                                   return $"{identifierType.Name}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))})";
                                                               }));

            throw new MissingMethodException($"No supported NugetPackageIdentifier constructor found. Available constructors:\n{constructors}");
        }

        static void InstallNugetPackage(MethodInfo installMethod, Type identifierType, string packageName)
        {
            // Equivalent to: new NugetPackageIdentifier(packageName, null)
            var identifier = CreateNugetPackageIdentifier(identifierType, packageName);
            installMethod.Invoke(null, new[]
            {
                identifier,
                true,   // refreshAssets
                false,  // isSlimRestoreInstall
                true    // allowUpdateForExplicitlyInstalled
            });
                    
            Debug.Log($"[NugetPackagesResolver]: NuGet package installation verified: {packageName}");
        }

#endregion
    }
}