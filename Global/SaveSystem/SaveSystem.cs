using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace Helpers
{
/// <summary>
/// Provides reliable save/load operations using JSON serialization.
///
/// Features:
/// • Atomic file replacement.
/// • Automatic backup creation.
/// • Save integrity verification (SHA256 of the JSON written to disk, so new fields in the type do not break old saves).
/// • Save version + migrate callback for format changes.
/// • UTF-8 encoding.
/// • System.Text.Json serialization.
/// </summary>
public static class SaveSystem
{
#region Fields

    static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    const string EXTENSION_TEMP = ".tmp";
    const string EXTENSION_BACKUP = ".bak";

#endregion

#region Public methods

    public static void Save<T>(string filePath, T data, int version = 0)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        EnsureSaveDirectoryExists(filePath);

        var dataNode = JsonSerializer.SerializeToNode(data, JsonOptions)!;
        var container = new JsonObject
        {
            ["Version"] = version,
            ["Hash"] = ComputeHash(dataNode),
            ["Data"] = dataNode
        };

        var tempPath = filePath + EXTENSION_TEMP;
        File.WriteAllText(tempPath, container.ToJsonString(JsonOptions), Encoding.UTF8);

        if (File.Exists(filePath)) // Atomically replaces the existing save while preserving a backup.
            File.Replace(tempPath, filePath, filePath + EXTENSION_BACKUP);
        else
            File.Move(tempPath, filePath);
    }

    /// <param name="version">Current save format version. Older files are passed to <paramref name="migrate"/> before deserialization.</param>
    /// <param name="migrate">Changes old JSON in place: (data, fileVersion).</param>
    public static T LoadOrCreate<T>(string filePath, int version = 0, Action<JsonObject, int> migrate = null) where T : class, new() =>
        TryLoad(filePath, version, migrate, out T loaded) ? loaded : new T();

    /// <summary>
    /// Attempts to load an object from disk.
    /// Returns false if the file is missing or corrupted.
    /// </summary>
    public static bool TryLoad<T>(string filePath, int version, Action<JsonObject, int> migrate, out T value)
    {
        value = default!;

        if (File.Exists(filePath) && TryLoadInternal(filePath, version, migrate, out value))
            return true;

        var backupPath = filePath + EXTENSION_BACKUP;
        return File.Exists(backupPath) && TryLoadInternal(backupPath, version, migrate, out value);
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

    static JsonSerializerOptions CreateJsonOptions() => new()
    {
        WriteIndented = true,
        IncludeFields = true,
        IgnoreReadOnlyProperties = true,
        Converters = { new ReactivePropertyJsonConverterFactory() }
    };

    static void EnsureSaveDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    static bool TryLoadInternal<T>(string filePath, int version, Action<JsonObject, int> migrate, out T value)
    {
        value = default!;

        try
        {
            var container = JsonNode.Parse(File.ReadAllText(filePath, Encoding.UTF8))!.AsObject();
            var data = container["Data"]!.AsObject();
            var hash = container["Hash"]!.GetValue<string>();
            var fileVersion = container["Version"]?.GetValue<int>() ?? 0;

            // Verify that the save has not been corrupted or modified.
            if (!string.Equals(hash, ComputeHash(data), StringComparison.Ordinal))
                return false;

            if (fileVersion < version)
                migrate?.Invoke(data, fileVersion);

            value = data.Deserialize<T>(JsonOptions)!;
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"SaveSystem: Failed to load data from {filePath}. {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Computes the SHA256 hash of compact JSON of the node: does not depend on C# type or file indentation.
    /// </summary>
    static string ComputeHash(JsonNode node)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(node.ToJsonString()));
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

#endregion
}
}
