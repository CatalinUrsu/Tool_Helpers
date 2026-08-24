using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if UNITY_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Helpers.Editor
{
public class ScenesViewer : EditorWindow
{
#region Fieldss

    Vector2 _scrollPos; // Tracks scroll position.

    readonly List<EntityId> _recents = new();
    readonly List<EntityId> _pins = new();
    readonly List<EntityId> _removed = new();
    readonly List<string> _scenes = new();
    readonly List<Object> _scenesObj = new();
    
    EditorBuildSettingsScene[] _scenesBuild => EditorBuildSettings.scenes;
    
    double _ts;
    bool _showRecent;
    bool _showInProject;

    const double REFRESH_PERIOD = 5.0;
    const int PIN_BUTTON_WIDTH = 20;

    GUIStyle _buttonStyle => _buttonStyle_f ??= new GUIStyle(GUI.skin.GetStyle("Button")) {alignment = TextAnchor.MiddleLeft};
    GUIStyle _buttonStyle_f;

#endregion

#region Implementation

    [MenuItem("Tools/Helpers/ScenesViewer _F3", false, 1000)]
    static void Init() => GetWindow(typeof(ScenesViewer), false, "ScenesViewer");

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);

        PrintScenesInBuild();
#if UNITY_ADDRESSABLES
        PrintAddressableScenes();
#endif
        PrintScenesInProject();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        _showRecent = EditorGUILayout.Foldout(_showRecent, "Recent");
        if (_showRecent)
        {
            ListRecent();
            ListPins();
        }
    }

    void PrintScenesInBuild()
    {
        var active = SceneManager.GetActiveScene();
        int activeSceneIndex = active.buildIndex;
        int sceneCount = _scenesBuild.Length;

        GUILayout.Label(" In Build", EditorStyles.boldLabel);


        // Next and previous buttons
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = activeSceneIndex != -1;
            if (GUILayout.Button("<="))
                SwitchScene((activeSceneIndex - 1 + sceneCount) % sceneCount);
            if (GUILayout.Button("=>"))
                SwitchScene((activeSceneIndex + 1) % sceneCount);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        var old = GUI.color;

        for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];

            if (scene.enabled)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);

                // Highlight current
                GUI.color = i == active.buildIndex ? Color.red : Color.white;

                GUILayout.BeginHorizontal();
                var pressed = GUILayout.Button($"{i}: {sceneName}", _buttonStyle);
                var add = CreateSmallButton("+");
                var sub = CreateSmallButton("-");
                var show = CreateSmallButton("*");
                GUILayout.EndHorizontal();

                if (pressed)
                    SwitchScene(i);

                if (add)
                    EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
                if (sub)
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(scene.path), true);

                if (show)
                {
                    var evt = Event.current;
                    if (evt.alt)
                        Selection.activeObject = GetObjFromAssetsPath(scene.path);
                    else
                        EditorGUIUtility.PingObject(GetObjFromAssetsPath(scene.path));
                }
            }
        }

        GUI.color = old;
    }

    #if UNITY_ADDRESSABLES
    void PrintAddressableScenes()
    {
        var activeScenePath = SceneManager.GetActiveScene().path;
        var addressableScenes = GetAddressableScenePaths();
        if (addressableScenes.Count == 0)
            return;

        GUILayout.Space(10);
        GUILayout.Label("Addressable Scenes", EditorStyles.boldLabel);

        var old = GUI.color;
        for (var i = 0; i < addressableScenes.Count; i++)
        {
            var scenePath = addressableScenes[i];
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            var isActive = EqualPaths(activeScenePath, scenePath);

            GUI.color = isActive ? Color.red : Color.white;

            GUILayout.BeginHorizontal();
            var pressed = GUILayout.Button($"A{i}: {sceneName}", _buttonStyle);
            var add = CreateSmallButton("+");
            var sub = CreateSmallButton("-");
            var show = CreateSmallButton("*");
            GUILayout.EndHorizontal();

            if (pressed)
                SwitchScene(scenePath);

            if (add)
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            if (sub)
                EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(scenePath), true);

            if (show)
            {
                var evt = Event.current;
                var obj = GetObjFromAssetsPath(scenePath);
                if (evt.alt)
                    Selection.activeObject = obj;
                else
                    EditorGUIUtility.PingObject(obj);
            }
        }

        GUI.color = old;
    }

    List<string> GetAddressableScenePaths()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        if (settings == null)
            return new List<string>();

        var result = new List<string>();
        var uniquePaths = new HashSet<string>();

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;

            foreach (AddressableAssetEntry entry in group.entries)
            {
                var assetPath = entry?.AssetPath;
                if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".unity"))
                    continue;

                if (uniquePaths.Add(assetPath))
                    result.Add(assetPath);
            }
        }

        return result;
    }
    #endif
    
    void PrintScenesInProject()
    {
        GUILayout.Space(20);
        _showInProject = EditorGUILayout.Foldout(_showInProject, "In Project (slower)");
        if (!_showInProject) return;
        
        var path = Application.dataPath;
        path = path.Substring(0, path.Length - 6); // without Assets
        var currSceneName = SceneManager.GetActiveScene().path;
        
        GUILayout.Label("Scenes In Project", EditorStyles.boldLabel);

        //Next and previous buttons
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<="))
        {
            var index = _scenes.IndexOf(path + currSceneName);
            var len = _scenes.Count;
            SwitchScene(_scenes[(index + len - 1) % len]);
        }

        if (GUILayout.Button("=>"))
        {
            int index = _scenes.LastIndexOf(path + currSceneName);
            SwitchScene(_scenes[(index + 1) % _scenes.Count]);
        }

        GUILayout.EndHorizontal();

        // Caching, rebuild only triggers if > RefreshPeriod
        var t = EditorApplication.timeSinceStartup;
        if (t - _ts > REFRESH_PERIOD)
        {
            CreateSceneList();
            _ts = t;
        }

        var old = GUI.color;

        for (var i = 0; i < _scenes.Count; i++)
        {
            var f = _scenes[i];

            bool eq = EqualPaths(currSceneName, GetPathFromAssets(f));
            GUI.color = eq ? Color.red : Color.white;

            GUILayout.BeginHorizontal();
            bool pressed = GUILayout.Button($"{i}: {_scenesObj[i].name}", _buttonStyle);
            bool add = CreateSmallButton("&");
            bool show = CreateSmallButton("*");
            GUILayout.EndHorizontal();

            if (add)
                AppendScene(f);

            if (pressed)
                SwitchScene(f);

            if (show)
            {
                var evt = Event.current;
                if (evt.alt)
                    Selection.activeObject = _scenesObj[i];
                else
                    EditorGUIUtility.PingObject(_scenesObj[i]);
            }
        }

        GUI.color = old;
    }

    void AppendScene(string fullpath)
    {
        var p = GetPathFromAssets(fullpath);
        if (string.IsNullOrEmpty(p))
            return;

        EditorSceneManager.OpenScene(p, OpenSceneMode.Additive);
    }

    void SwitchScene(string fullpath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            var p = GetPathFromAssets(fullpath);
            if (string.IsNullOrEmpty(p))
                return;

            EditorSceneManager.OpenScene(p);
        }
    }

    void SwitchScene(int index)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(_scenesBuild[index].path);
    }

    bool EqualPaths(string path1, string path2)
    {
        return path1.Replace("/", "\\") == path2.Replace("/", "\\");
    }

    bool CreateObjectButton(Object obj)
    {
        return GUILayout.Button(
                                new GUIContent($"{obj.GetEntityId()}: {obj.name}"),
                                new GUIStyle(GUI.skin.GetStyle("Button")) {alignment = TextAnchor.MiddleLeft}
                               );
    }

    bool CreateSmallButton(string str)
    {
        // var old = GUI.skin.button.overflow.alignment;
        // GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        var ret = GUILayout.Button(str, GUILayout.MaxWidth(PIN_BUTTON_WIDTH));
        // GUI.skin.label.alignment = old;
        return ret;
    }

    string GetPathFromAssets(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;

        var normalized = BackToForwardSlash(fullPath);
        if (normalized.StartsWith("Assets/"))
            return normalized;

        var dataPath = BackToForwardSlash(Application.dataPath);
        if (normalized.StartsWith(dataPath + "/"))
            return $"Assets{normalized.Substring(dataPath.Length)}";

        // Keep unknown format as-is so callers can still attempt to open valid project paths.
        return normalized;
    }

    Object GetObjFromAssetsPath(string fullPath)
    {
        return AssetDatabase.LoadMainAssetAtPath(fullPath);
    }

    string BackToForwardSlash(string path)
    {
        return path.Replace('\\', '/');
    }

    void CreateSceneList()
    {
        _scenes.Clear();
        _scenesObj.Clear();

        var files = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        foreach (var t in files)
        {
            string f = BackToForwardSlash(t);
            _scenes.Add(f);
            _scenesObj.Add(GetObjFromAssetsPath(GetPathFromAssets(f)));
        }
    }

    void ListRecent()
    {
        GUILayout.Label("Recent objects", EditorStyles.boldLabel);
        _recents.RemoveAll(x => EditorUtility.EntityIdToObject(x) == null);

        var old = GUI.color;

        foreach (var recent in _recents)
        {
            var n = EditorUtility.EntityIdToObject(recent);
            var s = Selection.activeEntityId;

            GUI.color = s == recent ? Color.red : Color.white;

            GUILayout.BeginHorizontal();

            bool pressed = CreateObjectButton(n);
            var plus = CreateSmallButton("+");
            GUILayout.EndHorizontal();

            if (pressed)
                Selection.entityIds = new[] {recent};

            if (plus && !_pins.Contains(recent))
                _pins.Add(recent);

        }

        GUI.color = old;
    }

    void ListPins()
    {
        GUILayout.Label("Pinned objects", EditorStyles.boldLabel);
        _pins.RemoveAll(x => EditorUtility.EntityIdToObject(x) == null);
        _removed.Clear();

        var old = GUI.color;

        foreach (var recent in _pins)
        {
            var n = EditorUtility.EntityIdToObject(recent);

            var s = Selection.activeEntityId;
            GUI.color = s == recent ? Color.red : Color.white;

            GUILayout.BeginHorizontal();
            bool pressed = CreateObjectButton(n);
            bool minus = CreateSmallButton("-");
            GUILayout.EndHorizontal();


            if (pressed)
                Selection.entityIds = new [] {recent};

            if (minus)
                _removed.Add(recent);

        }

        GUI.color = old;

        _pins.RemoveAll(x => _removed.Contains(x));
    }

    void OnSelectionChange()
    {
        UpdateList();
        Repaint();
    }

    void UpdateList()
    {
        var s = Selection.activeEntityId;

        if (s == 0 || _recents.Contains(s))
            return;

        _recents.Add(s);

        if (_recents.Count > 20)
            _recents.RemoveAt(0);
    }


#endregion
}
}