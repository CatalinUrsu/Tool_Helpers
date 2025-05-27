using System.IO;
using UnityEditor;
using UnityEngine;

namespace Helpers.Editor
{
public class UISpritePostProcess : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var name = Path.GetFileName(assetPath);

        if (name.ToLower().StartsWith("t_ui_"))
            PreprocessUISprites(name);
    }

    void PreprocessUISprites(string name)
    {
        var importer = (TextureImporter)assetImporter;

        if (importer.textureType == TextureImporterType.Sprite)
            return;

        Debug.Log($"[UISpritePostProcess] Processing UI sprite - {name} ");
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.spriteImportMode = SpriteImportMode.Single;
    }
}
}