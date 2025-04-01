using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Helpers.Editor
{
public class ProjectInitializer : EditorWindow
{
#region Fields

    static readonly string _id = "InitializeProject_";

    static string[] _folders = new string[]
    {
        "Content/Audio", "Content/Fonts",
        "Content/Materials", "Content/Prefabs",
        "Content/Scenes", "Content/Source",
        "Content/Textures", "Content/Textures/Atlases",
        "Content/Textures/UI", "Plugins"
    };
    
    static ProjectInitializer _window;
    static string _assetsFolder;

    static float _contentMaxWidth = 370;
    static float _buttonWidth = 150;
    static Vector2 _windowScrollPos;
    static Vector2 _localAssetsScollPos;
    static Vector2 _windowSize = new Vector2(400, 500);

    static bool _createFolders = true;
    static bool _updatePackages = true;
    static bool _importAssets = true;

    static List<string> _items;
    static List<bool> _checkedStates;


#endregion

    void OnEnable() => LoadPrefsSettings();

    void OnDisable() => SavePrefsSettings();

    void OnGUI()
    {
        _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);
        GUILayout.Label("Project Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        DrawCheckbox("Create Folders", "Common folders: Scripts, Scenes, Textures..", ref _createFolders);
        DrawCheckbox("Resolve dependencies", "Adds  packages to manifest.json", ref _updatePackages);

        GUILayout.Space(20);
        GUILayout.Box("", GUILayout.Width(_contentMaxWidth), GUILayout.Height(2));
        DrawCheckbox("Import Local Assets", "Imports selected assets from the list below", ref _importAssets);
        if (_importAssets)
        {
            DrawAddAssetsButton();
            DrawAssetList();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Setup Project", GUILayout.Height(50)) || (Event.current.keyCode == KeyCode.Return))
            SetupProject();

        if (Event.current.keyCode == KeyCode.Escape)
            _window.Close();
    }

    [MenuItem("Tools/Helpers/Initialize Project _F2", false, 0)]
    public static void Init()
    {
        _window = (ProjectInitializer)EditorWindow.GetWindow(typeof(ProjectInitializer));
        _window.titleContent = new GUIContent("Initialize Project");
        _window.minSize = _window.maxSize = _windowSize;
        LoadPrefsSettings();
        _window.Show();
    }

    static void LoadPrefsSettings()
    {
        _items = new List<string>();
        _checkedStates = new List<bool>();

        _importAssets = EditorPrefs.GetBool(_id + "importAssets", true);
        string[] listOfAssets = EditorPrefs.GetString(_id + "listOfAssets", "").Split('|');
        string[] checkedState = EditorPrefs.GetString(_id + "checkedState", "").Split('|');

        foreach (var asset in listOfAssets)
            if (!string.IsNullOrEmpty(asset))
                _items.Add(asset);

        foreach (var state in checkedState)
            if (!string.IsNullOrEmpty(state))
                _checkedStates.Add(state == "1");
    }

    static void SavePrefsSettings()
    {
        var listOfAssets = string.Join("|", _items);
        var checkedState = string.Join("|", _checkedStates.Select(state => state ? "1" : "0"));

        EditorPrefs.SetString(_id + "listOfAssets", listOfAssets);
        EditorPrefs.SetString(_id + "checkedState", checkedState);
        EditorPrefs.SetBool(_id + "importAssets", _importAssets);
    }

    /// <summary>
    /// Draw button that open file dialog to select assets from local asset store folder and add to <b>_items</b>
    /// </summary>
    static void DrawAddAssetsButton()
    {
        if (!GUILayout.Button("Select local assets...", GUILayout.Width(_buttonWidth))) return;

        var assetsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity\\Asset Store-5.x");
        if (Directory.Exists(assetsFolder))
        {
            string path = EditorUtility.OpenFilePanel("Select Asset to Include", assetsFolder, "unitypackage");
            if (!string.IsNullOrEmpty(path) && !_items.Contains(path))
            {
                _items.Add(path);
                _checkedStates.Add(true);
            }
            else
                Debug.LogWarning($"{Path.GetFileName(path)} is already added.");
        }
        else
            Debug.LogError($"Asset folder not found: {assetsFolder}");
    }

    /// <summary>
    /// Draw selected local assets in list with functionalities (include/exclude, swap up/down, remove)
    /// </summary>
    static void DrawAssetList()
    {
        Color originalColor = GUI.backgroundColor;

        GUILayout.Space(5);
        _localAssetsScollPos = EditorGUILayout.BeginScrollView(_localAssetsScollPos, "box", GUILayout.Width(_contentMaxWidth), GUILayout.Height(150));

        for (int i = 0; i < _items.Count; i++)
            DrawLocalAssets(i);

        GUILayout.EndScrollView();
        GUI.backgroundColor = originalColor;

        void DrawLocalAssets(int idx)
        {
            GUILayout.BeginHorizontal();

            _checkedStates[idx] = EditorGUILayout.Toggle(_checkedStates[idx], GUILayout.Width(20));

            GUILayout.Label(Path.GetFileName(_items[idx]), GUILayout.Width(240));

            EditorGUI.BeginDisabledGroup(idx == 0);
            if (GUILayout.Button("^", GUILayout.Width(20)))
                SwapItems(idx, idx - 1);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(idx == _items.Count - 1);
            if (GUILayout.Button("v", GUILayout.Width(20)))
                SwapItems(idx, idx + 1);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("x", GUILayout.Width(20)))
            {
                _items.RemoveAt(idx);
                _checkedStates.RemoveAt(idx);
            }

            GUILayout.EndHorizontal();
        }

        void SwapItems(int index1, int index2)
        {
            var tempItem = _items[index1];
            var tempState = _checkedStates[index1];
            _items[index1] = _items[index2];
            _checkedStates[index1] = _checkedStates[index2];
            _items[index2] = tempItem;
            _checkedStates[index2] = tempState;
        }
    }

    static void DrawCheckbox(string label, string tooltip, ref bool value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(label, tooltip), GUILayout.Width(200));
        value = EditorGUILayout.Toggle(value);
        EditorGUILayout.EndHorizontal();
    }

    static void SetupProject()
    {
        _assetsFolder = Application.dataPath;

        CreateFolders();
        UpdatePackages();
        ImportLocalAssets();
        SavePrefsSettings();
        _window.Close();

        PlayerSettings.SplashScreen.show = false;
        AssetDatabase.Refresh();

        void CreateFolders()
        {
            if (!_createFolders) return;

            foreach (string folder in _folders)
            {
                string folderPath = Path.Combine(_assetsFolder, folder);
                if (Directory.Exists(folderPath)) continue;
                Directory.CreateDirectory(folderPath);
            }

            AssetDatabase.Refresh();
        }

        void UpdatePackages()
        {
            if (!_updatePackages) return;

            PackagesResolver.AddExternalPackages();
            PackagesResolver.AddNugetPackags();
        }

        void ImportLocalAssets()
        {
            if (!_importAssets) return;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_checkedStates[i] && !File.Exists(_items[i]))
                {
                    Debug.LogError("File not found: " + _items[i]);
                    continue;
                }

                if (_checkedStates[i])
                {
                    Debug.Log("Importing: " + Path.GetFileName(_items[i]));
                    AssetDatabase.ImportPackage(_items[i], false);
                }
            }
        }
    }
}
}