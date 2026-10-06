using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    /// <summary>
    /// Picks a <see cref="DiscreteValueSelectionMode"/>, previewing which end a value travelling between two discrete
    /// values resolves to under the hovered mode.
    /// </summary>
    internal sealed class DiscreteValueSelectionModePickerPopup : PickerPopup<DiscreteValueSelectionMode>
    {
        private static readonly DiscreteValueSelectionMode[] s_modes =
        {
            DiscreteValueSelectionMode.Nearest,
            DiscreteValueSelectionMode.LeftSide,
            DiscreteValueSelectionMode.RightSide,
        };

        public DiscreteValueSelectionModePickerPopup(DiscreteValueSelectionMode current, Action<DiscreteValueSelectionMode> onSelected)
            : base(current, onSelected)
        {
        }

        protected override float ButtonsWidth => 90f;

        protected override int Rows => s_modes.Length;

        protected override void DrawButtons(Rect panel) =>
            ButtonColumn(panel, s_modes, mode => ObjectNames.NicifyVariableName(mode.ToString()));

        protected override void DrawPreview(Rect rect)
        {
            const float pipSize = 13f;
            const float padX = 14f;
            const float inactiveSize = 11f;
            const float activeSize = 18f;
            float lineY = rect.y + rect.height * 0.62f;
            float shapeY = rect.y + rect.height * 0.34f;
            float leftX = rect.x + padX;
            float rightX = rect.xMax - padX;

            CoreEditorGUI.DrawHorizontalAxis(leftX, rightX, lineY, CoreEditorGUI.PreviewAxis);
            DrawEndCap(leftX, lineY, +1f);
            DrawEndCap(rightX, lineY, -1f);

            CoreEditorGUI.EvaluatePipAnimation(EaseType.InOutCubic, out float t, out float fade);
            bool right = ResolvesToRightSide(Hovered, t);
            var pipColor = Color.Lerp(CoreEditorGUI.AccentBlue, CoreEditorGUI.AccentGreen, t) * new Color(0.95f, 0.95f, 0.95f, fade);
            CoreEditorGUI.DrawPip(new Vector2(Mathf.Lerp(leftX, rightX, t), lineY - 4f), pipColor, CoreEditorGUI.PipDirection.Up, pipSize);

            var ghost = new Color(1f, 1f, 1f, 0.28f);
            DrawEndMarker(new Vector2(leftX, shapeY), right ? inactiveSize : activeSize, isSquare: false,
                CoreEditorGUI.AccentBlue * (right ? ghost : Color.white));
            DrawEndMarker(new Vector2(rightX, shapeY), right ? activeSize : inactiveSize, isSquare: true,
                CoreEditorGUI.AccentGreen * (right ? Color.white : ghost));
        }

        private static bool ResolvesToRightSide(DiscreteValueSelectionMode mode, float t) => mode switch
        {
            DiscreteValueSelectionMode.Nearest => t >= 0.5f,
            DiscreteValueSelectionMode.RightSide => true,
            _ => false,
        };
    }
}
