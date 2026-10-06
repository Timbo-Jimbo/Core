using System;
using TimboJimbo.Core;
using TimboJimbo.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    /// <summary>Picks a <see cref="ColorInterpolationMode"/>, previewing an orange to blue blend through the hovered mode.</summary>
    internal sealed class ColorInterpolationModePickerPopup : PickerPopup<ColorInterpolationMode>
    {
        private static readonly ColorInterpolationMode[] s_modes =
        {
            ColorInterpolationMode.RGB,
            ColorInterpolationMode.HSV,
            ColorInterpolationMode.OkLab,
            ColorInterpolationMode.OkLCh,
        };

        private static readonly Color s_from = new(1.00f, 0.38f, 0.12f, 1f);
        private static readonly Color s_to = new(0.18f, 0.52f, 1.00f, 1f);

        public ColorInterpolationModePickerPopup(ColorInterpolationMode current, Action<ColorInterpolationMode> onSelected)
            : base(current, onSelected)
        {
        }

        protected override float ButtonsWidth => 90f;

        protected override int Rows => s_modes.Length;

        protected override void DrawButtons(Rect panel) => ButtonColumn(panel, s_modes, mode => mode.ToString());

        protected override void DrawPreview(Rect rect)
        {
            const float pipSize = 13f;
            const float padX = 14f;
            float lineY = rect.y + rect.height * 0.60f;
            float pipY = rect.y + rect.height * 0.33f;
            float leftX = rect.x + padX;
            float rightX = rect.xMax - padX;
            var mode = Hovered;

            DrawSampledLine(leftX, rightX, lineY, mode);
            DrawEndCap(leftX, lineY, +1f);
            DrawEndCap(rightX, lineY, -1f);

            DrawEndMarker(new Vector2(leftX, pipY), 11f, isSquare: false, Sample(mode, 0f));
            DrawEndMarker(new Vector2(rightX, pipY), 11f, isSquare: true, Sample(mode, 1f));

            CoreEditorGUI.EvaluatePipAnimation(EaseType.InOutCubic, out float t, out float fade);
            var pipColor = Sample(mode, t) * new Color(0.95f, 0.95f, 0.95f, fade);
            CoreEditorGUI.DrawPip(new Vector2(Mathf.Lerp(leftX, rightX, t), lineY - 1f), pipColor, CoreEditorGUI.PipDirection.Up, pipSize);
        }

        private static void DrawSampledLine(float x0, float x1, float y, ColorInterpolationMode mode)
        {
            const int segments = 96;
            var points = new Vector3[segments + 1];
            var colors = new Color[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points[i] = new Vector3(Mathf.Lerp(x0, x1, t), Mathf.Round(y));
                colors[i] = Sample(mode, t);
            }
            Handles.DrawAAPolyLine(4f, colors, points);
        }

        private static Color Sample(ColorInterpolationMode mode, float t) => ColorExtra.LerpUnclamped(mode, s_from, s_to, t);
    }
}
