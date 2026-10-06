using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TimboJimboEditor.Core
{
    /// <summary>
    /// Draws an enum property as a dropdown button that opens its picker popup. The popup picks outside the GUI loop the
    /// property was drawn in, so the pick is held by target and property path and written on the next draw, inside the
    /// loop, where prefab overrides, undo and change checks see it as any other edit.
    /// </summary>
    internal abstract class PickerDrawer<T> : PropertyDrawer where T : struct, Enum
    {
        private static readonly Dictionary<(Object Target, string Path), T> s_picked = new();

        /// <summary>The dropdown button: opens the picker, which calls <paramref name="onPicked"/> with the value picked.</summary>
        protected abstract void Popup(Rect rect, T current, Action<T> onPicked);

        public sealed override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var buttonRect = EditorGUI.PrefixLabel(rect, label);
            var key = (property.serializedObject.targetObject, property.propertyPath);
            var current = (T)Enum.ToObject(typeof(T), property.enumValueIndex);
            Popup(buttonRect, current, picked => s_picked[key] = picked);

            if (s_picked.TryGetValue(key, out var value))
            {
                property.enumValueIndex = Convert.ToInt32(value);
                s_picked.Remove(key);
            }

            EditorGUI.EndProperty();
        }
    }
}
