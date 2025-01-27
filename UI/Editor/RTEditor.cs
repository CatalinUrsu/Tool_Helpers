using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Helpers.UI.Editor
{
[CustomEditor(typeof(RectTransform), true)]
public class RTEditor : UnityEditor.Editor
{
#region Fields

    UnityEditor.Editor _editorInstance;
    bool _showTools;
    float _toolFieldWidth = 300;
    float _columnWidth = 150;
    float _buttonsWidth = 50;

    [NonSerialized] GUIStyle _labelStyle;
    [NonSerialized] GUIStyle _columnStyle;

    GUIStyle ColumnStyle
    {
        get
        {
            if (_columnStyle == null)
            {
                _columnStyle = new GUIStyle(GUI.skin.window);
                _columnStyle.padding = new RectOffset(2, 2, 5, 5);
                _columnStyle.alignment = TextAnchor.UpperCenter;
            }

            return _columnStyle;
        }
    }

    GUIStyle LabelStyle
    {
        get
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle();
                _labelStyle.alignment = TextAnchor.MiddleCenter;
                _labelStyle.fontStyle = FontStyle.Bold;
                _labelStyle.normal.textColor = Color.white;
            }

            return _labelStyle;
        }
    }


#endregion

#region Editor

    private void OnEnable()
    {
        _showTools = EditorPrefs.GetBool("ToolsVisible");
        Type rtEditor = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.RectTransformEditor");
        _editorInstance = CreateEditor(target, rtEditor);
    }

    void OnDisable()
    {
        if (_editorInstance)
            DestroyImmediate(_editorInstance);
    }

    void OnDestroy()
    {
        if (_editorInstance)
            Destroy(_editorInstance);
    }

    /// <summary>
    /// Tools for faster work with RectTransforms. Functionalities:
    ///  - Move anchors to Corners
    ///  - Instantly mirror RectTransforms around anchors and parent.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (!_editorInstance)
            return;

        _editorInstance.OnInspectorGUI();

        _showTools = EditorGUILayout.Foldout(_showTools, "Tools");
        EditorPrefs.SetBool("ToolsVisible", _showTools);

        if (_showTools)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinWidth(_toolFieldWidth), GUILayout.MaxWidth(_toolFieldWidth));

            if (GUILayout.Button("Anchor To Corners"))
                AnchorsToCorners();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(_toolFieldWidth));
            ShowMirrorAroundAnchor();
            ShowMirrorAroundParent();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    void OnSceneGUI()
    {
        if (!_editorInstance)
            return;

        MethodInfo onSceneGUI_Method = _editorInstance.GetType().GetMethod("OnSceneGUI", BindingFlags.NonPublic | BindingFlags.Instance);
        onSceneGUI_Method.Invoke(_editorInstance, null);
    }


#endregion

#region Private methods

    static void AnchorsToCorners()
    {
        foreach (Transform transform in Selection.transforms)
        {
            var rectTransform = transform as RectTransform;
            var rtParent = Selection.activeTransform.parent as RectTransform;

            if (!rectTransform || !rtParent)
                return;

            var parentRect = rtParent.rect;
            var newAnchorsMin = new Vector2(rectTransform.anchorMin.x + rectTransform.offsetMin.x / parentRect.width, rectTransform.anchorMin.y + rectTransform.offsetMin.y / parentRect.height);
            var newAnchorsMax = new Vector2(rectTransform.anchorMax.x + rectTransform.offsetMax.x / parentRect.width, rectTransform.anchorMax.y + rectTransform.offsetMax.y / parentRect.height);

            rectTransform.anchorMin = newAnchorsMin.RoundDigits(3);
            rectTransform.anchorMax = newAnchorsMax.RoundDigits(3);
            rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
        }
    }

    void ShowMirrorAroundAnchor()
    {
        EditorGUILayout.BeginVertical(ColumnStyle);

        EditorGUILayout.LabelField("Mirror Around Anchors", LabelStyle, GUILayout.Width(_columnWidth));

        EditorGUILayout.BeginHorizontal(GUILayout.Width(_columnWidth));
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("H", GUILayout.Width(_buttonsWidth)))
            Mirror(true, false);
        if (GUILayout.Button("V", GUILayout.Width(_buttonsWidth)))
            Mirror(false, false);

        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void ShowMirrorAroundParent()
    {
        EditorGUILayout.BeginVertical(ColumnStyle);

        EditorGUILayout.LabelField("Mirror Around Parent", LabelStyle, GUILayout.Width(_columnWidth));

        EditorGUILayout.BeginHorizontal(GUILayout.Width(_columnWidth));
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("H", GUILayout.Width(_buttonsWidth)))
            Mirror(true, true);
        if (GUILayout.Button("V", GUILayout.Width(_buttonsWidth)))
            Mirror(false, true);

        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    static void Mirror(bool horizontally, bool aroundParent)
    {
        foreach (var transform in Selection.transforms)
        {
            var rectTransform = transform as RectTransform;
            var rtParent = Selection.activeTransform.parent as RectTransform;

            if (!rectTransform || !rtParent)
                return;

            if (aroundParent)
            {
                var oldAnchorMin = rectTransform.anchorMin;
                var oldAnchorMax = rectTransform.anchorMax;
                rectTransform.anchorMin = horizontally ? new Vector2(1 - oldAnchorMax.x, oldAnchorMin.y) : new Vector2(oldAnchorMin.x, 1 - oldAnchorMax.y);
                rectTransform.anchorMax = horizontally ? new Vector2(1 - oldAnchorMin.x, oldAnchorMax.y) : new Vector2(oldAnchorMax.x, 1 - oldAnchorMin.y);
            }

            var oldOffsetMin = rectTransform.offsetMin;
            var oldOffsetMax = rectTransform.offsetMax;
            rectTransform.offsetMin = horizontally ? new Vector2(-oldOffsetMax.x, oldOffsetMin.y) : new Vector2(oldOffsetMin.x, -oldOffsetMax.y);
            rectTransform.offsetMax = horizontally ? new Vector2(-oldOffsetMin.x, oldOffsetMax.y) : new Vector2(oldOffsetMax.x, -oldOffsetMin.y);

            var oldLocalScale = rectTransform.localScale;
            rectTransform.localScale = horizontally ? new Vector3(-oldLocalScale.x, oldLocalScale.y, oldLocalScale.z) : new Vector3(oldLocalScale.x, -oldLocalScale.y, oldLocalScale.z);
        }
    }

#endregion
}
}