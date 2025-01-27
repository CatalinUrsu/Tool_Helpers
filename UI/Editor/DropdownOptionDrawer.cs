using UnityEditor;
using UnityEngine;

namespace UI.Editor
{
[CustomPropertyDrawer(typeof(DropdownOption))]
public class DropdownOptionDrawer : PropertyDrawer
{
#region Monobeh

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * 5;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);
        position.height = EditorGUIUtility.singleLineHeight;
        position.width += EditorGUIUtility.labelWidth;
        position.x -= EditorGUIUtility.labelWidth;
        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.indentLevel++;

        var useLocalizedTextProp = DrawLocalizedBoolCheck(ref position, property);
        DrawTextField(ref position, property, useLocalizedTextProp);
        DrawSpriteField(ref position, property);

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

#endregion

#region Private Methods

    static SerializedProperty DrawLocalizedBoolCheck(ref Rect position, SerializedProperty property)
    {
        SerializedProperty useLocalizedTextProp = property.FindPropertyRelative("_useLocalizedText");
        EditorGUI.PropertyField(position, useLocalizedTextProp, new GUIContent("Localized Text"));
        position.y += EditorGUIUtility.singleLineHeight;
        return useLocalizedTextProp;
    }

    static void DrawTextField(ref Rect position, SerializedProperty property, SerializedProperty useLocalizedTextProp)
    {
        if (useLocalizedTextProp.boolValue)
        {
            SerializedProperty localizedTxtProp = property.FindPropertyRelative("_localizedTxt");
            EditorGUI.PropertyField(position, localizedTxtProp, new GUIContent("Text"));
        }
        else
        {
            SerializedProperty usualTxtProp = property.FindPropertyRelative("_usualTxt");
            EditorGUI.PropertyField(position, usualTxtProp, new GUIContent("Text"));
        }

        position.y += EditorGUIUtility.singleLineHeight;
    }

    static void DrawSpriteField(ref Rect position, SerializedProperty property)
    {
        SerializedProperty spriteProp = property.FindPropertyRelative("_sprite");
        EditorGUI.PropertyField(position, spriteProp, new GUIContent("Image"));
    }

#endregion
}
}