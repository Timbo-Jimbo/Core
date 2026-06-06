using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    public static class CoreEditorGUI
    {
        public static readonly Color PreviewBackground = new Color(0.13f, 0.13f, 0.13f, 1f);
        public static readonly Color PreviewAxis = new Color(0.227f, 0.227f, 0.227f);

        public static readonly Color AccentBlue = new Color(0.20f, 0.50f, 1.00f, 1f);
        public static readonly Color AccentWhite = new Color(0.90f, 0.90f, 0.90f, 1f);
        public static readonly Color AccentGreen = new Color(0.10f, 0.85f, 0.45f, 1f);

        public static void EaseTypePopup(Rect rect, EaseType current, Action<EaseType> onSelected)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(current.ToString()), FocusType.Passive))
            {
                var popup = new EaseTypePickerPopup(current, onSelected);
                PopupWindow.Show(rect, popup);
            }
        }

        public static void DiscreteValueSelectionModePopup(Rect rect, DiscreteValueSelectionMode current, Action<DiscreteValueSelectionMode> onSelected)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(ObjectNames.NicifyVariableName(current.ToString())), FocusType.Passive))
            {
                var popup = new DiscreteValueSelectionModePickerPopup(current, onSelected);
                PopupWindow.Show(rect, popup);
            }
        }

        public static void ColorInterpolationModePopup(Rect rect, ColorInterpolationMode current, Action<ColorInterpolationMode> onSelected)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(current.ToString()), FocusType.Passive))
            {
                var popup = new ColorInterpolationModePickerPopup(current, onSelected);
                PopupWindow.Show(rect, popup);
            }
        }

        public enum PipDirection { Up, Down, Left, Right }

        public static void EvaluatePipAnimation(EaseType travelEase, out float t, out float fade)
        {
            const double cycle = 1.5;
            const float travelEnd = 0.75f;
            const float fadeOutStart = 0.85f;

            float phase = (float)(EditorApplication.timeSinceStartup % cycle / cycle);
            t = EaseUtility.Evaluate(Mathf.Clamp01(phase / travelEnd), travelEase);
            fade = EaseUtility.Evaluate(
                1f - Mathf.Clamp01((phase - fadeOutStart) / (1f - fadeOutStart)),
                EaseType.InCubic);
        }

        public static void DrawPip(Vector2 tip, Color color, PipDirection direction, float size = 15f)
        {
            float half = size * 0.5f;
            float h = half * 0.6f;

            var p0 = new Vector3(size, -h);
            var p1 = new Vector3(size, h);
            var p2 = new Vector3(half, h);
            var p3 = new Vector3(0f, 0f);
            var p4 = new Vector3(half, -h);

            float rot = direction switch
            {
                PipDirection.Left => 0f,
                PipDirection.Up => 90f,
                PipDirection.Right => 180f,
                PipDirection.Down => 270f,
                _ => 0f,
            };

            var prev = (Handles.color, Handles.matrix);
            Handles.color = color;
            Handles.matrix = Matrix4x4.TRS(new Vector3(tip.x, tip.y), Quaternion.Euler(0f, 0f, rot), Vector3.one);
            Handles.DrawAAConvexPolygon(p0, p1, p2, p3, p4);
            (Handles.color, Handles.matrix) = prev;
        }

        public static void DrawHorizontalAxis(float x0, float x1, float y, Color color)
        {
            var prev = Handles.color;
            Handles.color = color;
            float yr = Mathf.RoundToInt(y);
            Handles.DrawLine(new Vector3(x0, yr), new Vector3(x1, yr), 0.5f);
            Handles.color = prev;
        }
    }

    public static class CoreEditorGUILayout
    {
        public static void EaseTypePopup(EaseType current, Action<EaseType> onSelected, params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(options);
            CoreEditorGUI.EaseTypePopup(rect, current, onSelected);
        }

        public static void DiscreteValueSelectionModePopup(DiscreteValueSelectionMode current, Action<DiscreteValueSelectionMode> onSelected, params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(options);
            CoreEditorGUI.DiscreteValueSelectionModePopup(rect, current, onSelected);
        }

        public static void ColorInterpolationModePopup(ColorInterpolationMode current, Action<ColorInterpolationMode> onSelected, params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(options);
            CoreEditorGUI.ColorInterpolationModePopup(rect, current, onSelected);
        }
    }
}