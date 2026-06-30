using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Helpers.Editor.Bootstrap
{
public static class NugetPackagesResolver
{
    static readonly string[] _packagesNuget = {
        "R3",
        "ObservableCollections"
    };
    
#region Public methods
    
        [MenuItem("Tools/Helpers/Wizard/Resolve Nuget", false, 1)]
        public static void AddNugetPackages()
        {
            if (TryGetNugetAssemblies(out var installerType, out var identifierType, out var installedPackagesManagerType))
            {
                OpenRestartDialogue();
                return;
            }

            var installMethod = installerType.GetMethod("InstallIdentifier", BindingFlags.Static | BindingFlags.Public);
            var isInstalledMethod = installedPackagesManagerType.GetMethod("IsInstalled",
                                                                           BindingFlags.Static | BindingFlags.NonPublic,
                                                                           null,
                                                                           new[] { typeof(string), typeof(bool) },
                                                                           null);

            if (installMethod == null || isInstalledMethod == null) return;

            foreach (var packageName in _packagesNuget)
            {
                if ((bool)isInstalledMethod.Invoke(null, new object[] { packageName, true }))
                {
                    Debug.Log($"[NugetPackagesResolver]: NuGet package already installed: {packageName}");
                    continue;
                }

                InstallNugetPackage(installMethod, identifierType, packageName);
            }
        }
        
#endregion

#region Private methods

        /// <summary>
        /// Use reflection so the script compiles even if NuGetForUnity is not installed yet
        /// </summary>
        /// <returns></returns>
        static bool TryGetNugetAssemblies(out Type installerType, out Type identifierType, out Type installedPackagesManagerType)
        {
            installerType = AppDomain.CurrentDomain.GetAssemblies()
                                     .Select(assembly => assembly.GetType("NugetForUnity.NugetPackageInstaller"))
                                     .FirstOrDefault(type => type != null);

            identifierType = AppDomain.CurrentDomain.GetAssemblies()
                                      .Select(assembly => assembly.GetType("NugetForUnity.Models.NugetPackageIdentifier"))
                                      .FirstOrDefault(type => type != null);

            installedPackagesManagerType = AppDomain.CurrentDomain.GetAssemblies()
                                                    .Select(assembly => assembly.GetType("NugetForUnity.InstalledPackagesManager"))
                                                    .FirstOrDefault(type => type != null);

            return installerType == null || identifierType == null || installedPackagesManagerType == null;
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