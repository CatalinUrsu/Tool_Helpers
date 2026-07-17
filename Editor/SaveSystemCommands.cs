using UnityEditor;

namespace Helpers.Editor
{
public class SaveSystemCommands
{
    [MenuItem("Tools/Helpers/ResetSaves", false, 2)]
    public static void ResetSaves() => SaveSystem.DeleteDirectory();
}
}