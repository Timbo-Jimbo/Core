using System.Collections.Generic;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    [CustomPropertyDrawer(typeof(EaseType))]
    internal sealed class EaseTypeDrawer : PropertyDrawer
    {
        private static Dictionary<(Object target, string propertyPath), EaseType> _pendingChanges = new();
        
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var buttonRect = EditorGUI.PrefixLabel(rect, label);
            var currentValue = (EaseType)property.enumValueIndex;
            var targetKey = (property.serializedObject.targetObject, property.propertyPath);
            CoreEditorGUI.EaseTypePopup(buttonRect, currentValue, newValue =>
            {
                _pendingChanges[targetKey] = newValue;
            });

            if (_pendingChanges.TryGetValue(targetKey, out var pendingValue))
            {
                property.enumValueIndex = (int)pendingValue;
                _pendingChanges.Remove(targetKey);
            }

            EditorGUI.EndProperty();
        }
    }
}