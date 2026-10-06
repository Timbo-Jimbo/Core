using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    [CustomPropertyDrawer(typeof(ColorInterpolationMode))]
    internal sealed class ColorInterpolationModeDrawer : PickerDrawer<ColorInterpolationMode>
    {
        protected override void Popup(Rect rect, ColorInterpolationMode current, Action<ColorInterpolationMode> onPicked) =>
            CoreEditorGUI.ColorInterpolationModePopup(rect, current, onPicked);
    }
}
