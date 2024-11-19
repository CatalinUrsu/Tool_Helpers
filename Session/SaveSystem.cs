using System;
using Constants;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Source.Session
{
public class SaveSystem
{
    public void Save<T>(string filePath, T sessionElement)
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH) == false)
            Directory.CreateDirectory(ConstSession.SAVES_FOLDER_PATH);

        using (FileStream fs = File.Open(filePath, FileMode.Create))
        {
            BinaryWriter writer = new BinaryWriter(fs);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sessionElement));
            writer.Write(Convert.ToBase64String(plainTextBytes));
            writer.Flush();
        }
    }
    
    public T Load<T>(string filePath) where T: class
    {
        if (!Directory.Exists(ConstSession.SAVES_FOLDER_PATH) || !File.Exists(filePath)) return null;

        using (FileStream fs = File.Open(filePath, FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fs);
            byte[] encodedBytes = Convert.FromBase64String(reader.ReadString());
            var value = Encoding.UTF8.GetString(encodedBytes);
            return JsonConvert.DeserializeObject<T>(value);;
        }
    }

    public static void ResetSaves()
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH))
            Directory.Delete(ConstSession.SAVES_FOLDER_PATH, true);
    }
}
}