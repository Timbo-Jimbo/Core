using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    [CustomPropertyDrawer(typeof(DiscreteValueSelectionMode))]
    internal sealed class DiscreteValueSelectionModeDrawer : PickerDrawer<DiscreteValueSelectionMode>
    {
        protected override void Popup(Rect rect, DiscreteValueSelectionMode current, Action<DiscreteValueSelectionMode> onPicked) =>
            CoreEditorGUI.DiscreteValueSelectionModePopup(rect, current, onPicked);
    }
}
