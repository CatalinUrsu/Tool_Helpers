using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace Helpers.UI.Editor
{
[CustomEditor(typeof(DropdownHelper))]
public class DropdownHelperEditor : SelectableEditor
{
#region Fields

    SerializedProperty _caption;
    SerializedProperty _panel;
    SerializedProperty _itemParent;
    SerializedProperty _itemTemplate;

    SerializedProperty _multiSelect;
    SerializedProperty _addNoneItem;
    SerializedProperty _addEverythingItem;

    SerializedProperty _animation;
    SerializedProperty _panelAnimSpeed;

    SerializedProperty _nothingOption;
    SerializedProperty _mixedOption;
    SerializedProperty _everythingOption;
    SerializedProperty _options;

    [NonSerialized] GUIStyle _boxStyle;

    GUIStyle BoxStyle
    {
        get
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.window);
                _boxStyle.padding = new RectOffset(2, 2, 5, 5);
                _boxStyle.alignment = TextAnchor.UpperCenter;
            }

            return _boxStyle;
        }
    }

#endregion

#region Monobeh

    protected override void OnEnable()
    {
        base.OnEnable();

        _caption = serializedObject.FindProperty("Caption");
        _panel = serializedObject.FindProperty("Panel");
        _itemTemplate = serializedObject.FindProperty("ItemTemplate");
        _itemParent = serializedObject.FindProperty("ItemParent");

        _multiSelect = serializedObject.FindProperty("MultiSelect");
        _addNoneItem = serializedObject.FindProperty("AddNoneItem");
        _addEverythingItem = serializedObject.FindProperty("AddEverythingItem");

        _animation = serializedObject.FindProperty("AnimationType");
        _panelAnimSpeed = serializedObject.FindProperty("PanelAnimSpeed");

        _nothingOption = serializedObject.FindProperty("NothingOption");
        _mixedOption = serializedObject.FindProperty("MixedOption");
        _everythingOption = serializedObject.FindProperty("EverythingOption");
        _options = serializedObject.FindProperty("Options");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawElements();
        DrawAnimation();
        DrawOptions();

        serializedObject.ApplyModifiedProperties();
    }

#endregion

#region Private methods

    void DrawElements()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Elements", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(BoxStyle);

        EditorGUILayout.PropertyField(_caption);
        EditorGUILayout.PropertyField(_panel);
        EditorGUILayout.PropertyField(_itemTemplate);
        EditorGUILayout.PropertyField(_itemParent);

        EditorGUILayout.EndVertical();
    }

    void DrawAnimation()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(BoxStyle);

        EditorGUILayout.PropertyField(_animation);
        if (_animation.enumValueIndex != 0)
            EditorGUILayout.PropertyField(_panelAnimSpeed);

        EditorGUILayout.EndVertical();
    }

    void DrawOptions()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(BoxStyle);

        EditorGUILayout.PropertyField(_multiSelect);
        if (_multiSelect.boolValue)
        {
            // Show additional options (None, Mixed, Everything
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_mixedOption);

            EditorGUILayout.Space();
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
            EditorGUILayout.PropertyField(_addNoneItem);
            if (_addNoneItem.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_nothingOption);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
            EditorGUILayout.PropertyField(_addEverythingItem);
            if (_addEverythingItem.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_everythingOption);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUI.indentLevel++;
            EditorGUI.indentLevel--;
            
            _addNoneItem.boolValue = false;
            _addEverythingItem.boolValue = false;
        }

        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(_options);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

#endregion
}
}