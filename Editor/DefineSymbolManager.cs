using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

namespace Helpers.Editor
{
public static class DefineSymbolManager
{
    public static void AddDefineSymbol(string defineSymbol)
    {
        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
        var currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

        if (!currentSymbols.Contains(defineSymbol))
        {
            var newSymbols = currentSymbols + ";" + defineSymbol;
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newSymbols);
            Debug.Log($"Added define: {defineSymbol}");
        }
    }    
}
}