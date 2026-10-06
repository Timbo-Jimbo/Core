using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    [CustomPropertyDrawer(typeof(EaseType))]
    internal sealed class EaseTypeDrawer : PickerDrawer<EaseType>
    {
        protected override void Popup(Rect rect, EaseType current, Action<EaseType> onPicked) =>
            CoreEditorGUI.EaseTypePopup(rect, current, onPicked);
    }
}
