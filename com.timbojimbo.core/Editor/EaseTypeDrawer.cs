using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    [CustomPropertyDrawer(typeof(EaseType))]
    internal sealed class EaseTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var buttonRect = EditorGUI.PrefixLabel(rect, label);
            var currentValue = (EaseType)property.enumValueIndex;
            CoreEditorGUI.EaseTypePopup(buttonRect, currentValue, newValue =>
            {
                property.enumValueIndex = (int)newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            EditorGUI.EndProperty();
        }
    }
}