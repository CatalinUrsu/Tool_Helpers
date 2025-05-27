using System;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.AddressableAssets;

namespace Helpers.Editor
{
public static class SpriteAtlasUtils
{
    public static void SetAllIncludeInBuild(bool enable)
    {
        var spriteAtlases = LoadSpriteAtlases();

        foreach (SpriteAtlas atlas in spriteAtlases) 
            SetIncludeInBuild(atlas, enable);
    }

    static void SetIncludeInBuild(SpriteAtlas spriteAtlas, bool enable)
    {
        if (!IsAddressable(spriteAtlas))
            return;
        
        var so = new SerializedObject(spriteAtlas);
        var atlasEditorData = so.FindProperty("m_EditorData");
        var includeInBuild = atlasEditorData.FindPropertyRelative("bindAsDefault");
        
        includeInBuild.boolValue = enable;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spriteAtlas);
        AssetDatabase.Refresh();
    }

    static SpriteAtlas[] LoadSpriteAtlases()
    {
        var findAssets = AssetDatabase.FindAssets($"t: {nameof(SpriteAtlas)}");

        return findAssets.Length == 0
            ? Array.Empty<SpriteAtlas>()
            : findAssets
              .Select(AssetDatabase.GUIDToAssetPath)
              .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
              .ToArray();
    }

    static bool IsAddressable(SpriteAtlas spriteAtlas)
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        if (settings == null)
            return false;

        var assetPath = AssetDatabase.GetAssetPath(spriteAtlas);
        var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
        
        return entry != null;
    }
}
}