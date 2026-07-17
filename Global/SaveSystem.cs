using System;
using Constants;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Helpers
{
/// <summary>
/// Provides reliable save/load operations using JSON serialization.
///
/// Features:
/// • Atomic file replacement.
/// • Automatic backup creation.
/// • Save integrity verification (SHA256).
/// • UTF-8 encoding.
/// • System.Text.Json serialization.
/// </summary>
public static class SaveSystem
{
#region Fields

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    const string EXTENSION_TEMP = ".tmp";
    const string EXTENSION_BACKUP = ".bak";

#endregion

#region Public methods

    public static void Save<T>(string filePath, T data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        EnsureSaveDirectoryExists(filePath);

        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        var tempPath = filePath + EXTENSION_TEMP;

        SaveContainer container = new()
        {
            Hash = ComputeHash(dataBytes),
            Data = data
        };

        File.WriteAllText(tempPath,
                          JsonSerializer.Serialize(container, JsonOptions),
                          Encoding.UTF8);

        if (File.Exists(filePath)) // Atomically replaces the existing save while preserving a backup.
            File.Replace(tempPath, filePath, filePath + EXTENSION_BACKUP);
        else
            File.Move(tempPath, filePath);
    }

    /// <summary>
    /// Attempts to load an object from disk.
    /// Returns false if the file is missing or corrupted.
    /// </summary>
    public static bool TryLoad<T>(string filePath, out T value)
    {
        value = default!;

        if (File.Exists(filePath) && TryLoadInternal(filePath, out value))
            return true;

        var backupPath = filePath + EXTENSION_BACKUP;
        return File.Exists(backupPath) && TryLoadInternal(backupPath, out value);
    }

    /// <summary>
    /// Deletes a save file and its backup.
    /// </summary>
    public static void Delete(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        var backupPath = filePath + EXTENSION_BACKUP;

        if (File.Exists(backupPath))
            File.Delete(backupPath);
    }

    public static void DeleteDirectory(string directory = "")
    {
        var saveFilePath = !string.IsNullOrEmpty(directory) ? directory : ConstSession.SAVES_FOLDER_PATH;

        if (Directory.Exists(saveFilePath))
            Directory.Delete(saveFilePath, true);
    }

#endregion

#region Private methods

    static void EnsureSaveDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    static bool TryLoadInternal<T>(string filePath, out T value)
    {
        value = default!;

        try
        {
            var container = JsonSerializer.Deserialize<SaveContainer<T>>(File.ReadAllText(filePath, Encoding.UTF8), JsonOptions);

            if (container == null)
                return false;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(container.Data, JsonOptions);

            // Verify that the save has not been corrupted or modified.
            if (!string.Equals(container.Hash, ComputeHash(bytes), StringComparison.Ordinal))
                return false;

            value = container.Data!;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Computes the SHA256 hash of serialized data.
    /// </summary>
    static string ComputeHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

#endregion

#region External Data

    /// <summary>
    /// Represents the on-disk save format.
    /// </summary>
    sealed class SaveContainer<T>
    {
        public string Hash { get; set; }

        public T Data { get; set; }
    }

    // Used only during save to simplify generic type inference.
    sealed class SaveContainer
    {
        public string Hash { get; set; }

        public object Data { get; set; }
    }

#endregion
}
}