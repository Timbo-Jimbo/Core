using System;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    /// <summary>
    /// The shell the enum pickers share: a panel of toggle buttons on the left, the current value lit, and a live
    /// preview on the right of whichever value the mouse is over. Clicking a button picks it and closes the popup.
    /// Subclasses lay their buttons out (<see cref="DrawButtons"/>, each through <see cref="Button"/>) and draw the
    /// preview (<see cref="DrawPreview"/>, on repaint only).
    /// </summary>
    internal abstract class PickerPopup<T> : PopupWindowContent where T : struct, Enum
    {
        protected const float Padding = 6f;
        protected const float ButtonHeight = 18f;
        protected const float PreviewWidth = 176f;

        private readonly Action<T> _onSelected;
        private T _current;

        protected PickerPopup(T current, Action<T> onSelected)
        {
            _current = current;
            Hovered = current;
            _onSelected = onSelected;
        }

        /// <summary>The value the mouse is over (the current one until it moves), which the preview shows.</summary>
        protected T Hovered { get; private set; }

        /// <summary>How wide the button panel is.</summary>
        protected abstract float ButtonsWidth { get; }

        /// <summary>How many rows of buttons there are.</summary>
        protected abstract int Rows { get; }

        protected abstract void DrawButtons(Rect panel);

        protected abstract void DrawPreview(Rect panel);

        public sealed override Vector2 GetWindowSize() =>
            new(Padding + ButtonsWidth + Padding + PreviewWidth + Padding, Padding + ButtonHeight * Rows + Padding);

        public sealed override void OnOpen()
        {
            editorWindow.wantsMouseMove = true;
            EditorApplication.update += Repaint;
        }

        public sealed override void OnClose() => EditorApplication.update -= Repaint;

        // The preview animates, so it is drawn again every editor update while the popup is open.
        private void Repaint() => editorWindow?.Repaint();

        public sealed override void OnGUI(Rect rect)
        {
            float left = rect.x + Padding;
            float top = rect.y + Padding;
            float height = rect.height - Padding * 2f;

            DrawButtons(new Rect(left, top, ButtonsWidth, height));
            if (Event.current.type == EventType.Repaint)
            {
                var preview = new Rect(left + ButtonsWidth + Padding, top, PreviewWidth, height);
                EditorGUI.DrawRect(preview, CoreEditorGUI.PreviewBackground);
                Handles.BeginGUI();
                DrawPreview(preview);
                Handles.EndGUI();
            }

            if (Event.current.type == EventType.MouseMove)
                Event.current.Use();
        }

        /// <summary>One button: lit while it is the current value, and the hovered value while the mouse is over it.</summary>
        protected void Button(Rect rect, T value, string label, GUIStyle style)
        {
            if (rect.Contains(Event.current.mousePosition) && !Hovered.Equals(value))
            {
                Hovered = value;
                editorWindow?.Repaint();
            }

            bool isCurrent = value.Equals(_current);
            if (GUI.Toggle(rect, isCurrent, label, style) != isCurrent)
            {
                _current = value;
                _onSelected?.Invoke(value);
                editorWindow?.Close();
            }
        }

        /// <summary>A column of buttons, one row each.</summary>
        protected void ButtonColumn(Rect panel, T[] values, Func<T, string> label)
        {
            for (int i = 0; i < values.Length; i++)
                Button(new Rect(panel.x, panel.y + ButtonHeight * i, panel.width, ButtonHeight), values[i], label(values[i]), Styles.Solo);
        }

        // ── Preview drawing ──────────────────────────────────────────────────────

        /// <summary>A chevron closing off one end of the preview's line, pointing in (<paramref name="direction"/> +1 at the left end, -1 at the right).</summary>
        protected static void DrawEndCap(float x, float y, float direction)
        {
            const float w = 4f;
            const float h = 5f;
            var previous = Handles.color;
            Handles.color = CoreEditorGUI.PreviewAxis;
            Handles.DrawAAPolyLine(1.5f,
                new Vector3(x + direction * w, y - h),
                new Vector3(x, y),
                new Vector3(x + direction * w, y + h));
            Handles.color = previous;
        }

        /// <summary>The marker of one end of the preview: a disc for the start, a square for the end.</summary>
        protected static void DrawEndMarker(Vector2 center, float size, bool isSquare, Color color)
        {
            float half = size * 0.5f;
            var previous = Handles.color;
            Handles.color = color;
            if (isSquare)
            {
                Handles.DrawAAConvexPolygon(
                    new Vector3(center.x - half, center.y - half),
                    new Vector3(center.x + half, center.y - half),
                    new Vector3(center.x + half, center.y + half),
                    new Vector3(center.x - half, center.y + half));
            }
            else
            {
                const int segments = 32;
                var points = new Vector3[segments];
                for (int i = 0; i < segments; i++)
                {
                    float a = i / (float)segments * Mathf.PI * 2f;
                    points[i] = new Vector3(center.x + Mathf.Cos(a) * half, center.y + Mathf.Sin(a) * half);
                }
                Handles.DrawAAConvexPolygon(points);
            }
            Handles.color = previous;
        }

        protected static class Styles
        {
            private static GUIStyle s_solo, s_left, s_mid, s_right, s_rowLabel;

            public static GUIStyle Solo => s_solo ??= Button(EditorStyles.miniButton);
            public static GUIStyle Left => s_left ??= Button(EditorStyles.miniButtonLeft);
            public static GUIStyle Mid => s_mid ??= Button(EditorStyles.miniButtonMid);
            public static GUIStyle Right => s_right ??= Button(EditorStyles.miniButtonRight);
            public static GUIStyle RowLabel => s_rowLabel ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 10 };

            private static GUIStyle Button(GUIStyle from) => new(from) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };
        }
    }
}
