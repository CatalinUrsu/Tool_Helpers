using System;
using Constants;
using System.IO;
using MemoryPack;

namespace Helpers
{
public static class SaveSystem
{
    public static void Save<T>(string filePath, T sessionElement)
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH) == false)
            Directory.CreateDirectory(ConstSession.SAVES_FOLDER_PATH);

        using var fs = File.Open(filePath, FileMode.Create);
        var writer = new BinaryWriter(fs);
        var serializedData = MemoryPackSerializer.Serialize(sessionElement);
        writer.Write(Convert.ToBase64String(serializedData));
        writer.Flush();
    }
    
    public static T Load<T>(string filePath) where T: class
    {
        if (!Directory.Exists(ConstSession.SAVES_FOLDER_PATH) || !File.Exists(filePath)) return null;

        using var fs = File.Open(filePath, FileMode.Open);
        var reader = new BinaryReader(fs);
        var encodedBytes = Convert.FromBase64String(reader.ReadString());
        return MemoryPackSerializer.Deserialize<T>(encodedBytes);;
    }

    public static void ResetSaves()
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH))
            Directory.Delete(ConstSession.SAVES_FOLDER_PATH, true);
    }
}
}