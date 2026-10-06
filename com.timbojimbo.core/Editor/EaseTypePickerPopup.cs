using System;
using TimboJimbo.Core;
using UnityEditor;
using UnityEngine;

namespace TimboJimboEditor.Core
{
    /// <summary>
    /// Picks an <see cref="EaseType"/>: a row per family (In, In Out, Out), previewing the hovered ease's curve with a
    /// pip running along it.
    /// </summary>
    internal sealed class EaseTypePickerPopup : PickerPopup<EaseType>
    {
        private const float FamilyLabelWidth = 50f;
        private const float CurveDisplayMin = -0.2f;
        private const float CurveDisplayMax = 1.2f;

        private static readonly (string Family, EaseType[] Types)[] s_families =
        {
            ("Linear",  new[] { EaseType.Linear }),
            ("Sine",    new[] { EaseType.InSine,    EaseType.InOutSine,    EaseType.OutSine    }),
            ("Quad",    new[] { EaseType.InQuad,    EaseType.InOutQuad,    EaseType.OutQuad    }),
            ("Cubic",   new[] { EaseType.InCubic,   EaseType.InOutCubic,   EaseType.OutCubic   }),
            ("Quart",   new[] { EaseType.InQuart,   EaseType.InOutQuart,   EaseType.OutQuart   }),
            ("Quint",   new[] { EaseType.InQuint,   EaseType.InOutQuint,   EaseType.OutQuint   }),
            ("Expo",    new[] { EaseType.InExpo,    EaseType.InOutExpo,    EaseType.OutExpo    }),
            ("Circ",    new[] { EaseType.InCirc,    EaseType.InOutCirc,    EaseType.OutCirc    }),
            ("Back",    new[] { EaseType.InBack,    EaseType.InOutBack,    EaseType.OutBack    }),
            ("Elastic", new[] { EaseType.InElastic, EaseType.InOutElastic, EaseType.OutElastic }),
            ("Bounce",  new[] { EaseType.InBounce,  EaseType.InOutBounce,  EaseType.OutBounce  }),
        };

        private static readonly string[] s_variants = { "In", "In Out", "Out" };

        public EaseTypePickerPopup(EaseType current, Action<EaseType> onSelected) : base(current, onSelected)
        {
        }

        protected override float ButtonsWidth => 272f;

        protected override int Rows => s_families.Length;

        protected override void DrawButtons(Rect panel)
        {
            float buttonsX = panel.x + FamilyLabelWidth;
            float buttonsWidth = panel.width - FamilyLabelWidth;
            for (int f = 0; f < s_families.Length; f++)
            {
                var (family, types) = s_families[f];
                float y = panel.y + ButtonHeight * f;
                var labelRect = new Rect(panel.x, y, FamilyLabelWidth, ButtonHeight);
                if (new Rect(panel.x, y, panel.width, ButtonHeight).Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(labelRect, new Color(1f, 1f, 1f, 0.1f));
                EditorGUI.LabelField(labelRect, family, Styles.RowLabel);

                if (types.Length == 1)
                {
                    Button(new Rect(buttonsX, y, buttonsWidth, ButtonHeight), types[0], family, Styles.Solo);
                    continue;
                }
                float width = buttonsWidth / 3f;
                Button(new Rect(buttonsX, y, width, ButtonHeight), types[0], s_variants[0], Styles.Left);
                Button(new Rect(buttonsX + width, y, width, ButtonHeight), types[1], s_variants[1], Styles.Mid);
                Button(new Rect(buttonsX + width * 2f, y, width, ButtonHeight), types[2], s_variants[2], Styles.Right);
            }
        }

        protected override void DrawPreview(Rect rect)
        {
            const float pipSize = 15f;
            var ease = Hovered;
            float padX = 10f + pipSize;
            float padY = OvershootPadding(ease);
            float innerW = rect.width - padX * 2f;
            float innerH = rect.height - padY * 2f;
            float X(float t) => rect.x + padX + t * innerW;
            float Y(float v) => rect.y + padY + (1f - (v - CurveDisplayMin) / (CurveDisplayMax - CurveDisplayMin)) * innerH;

            CoreEditorGUI.DrawHorizontalAxis(X(0f), X(1f), Y(0f), CoreEditorGUI.PreviewAxis);
            CoreEditorGUI.DrawHorizontalAxis(X(0f), X(1f), Y(1f), CoreEditorGUI.PreviewAxis);

            var (start, end) = CurveColors(ease);
            const int segments = 80;
            var points = new Vector3[segments + 1];
            var colors = new Color[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float v = EaseUtility.Evaluate(t, ease);
                points[i] = new Vector3(X(t), Y(v));
                colors[i] = Color.Lerp(start, end, v);
            }
            Handles.DrawAAPolyLine(3f, colors, points);

            CoreEditorGUI.EvaluatePipAnimation(ease, out float value, out float fade);
            var pipColor = Color.Lerp(start, end, value) * new Color(0.9f, 0.9f, 0.9f, fade);
            CoreEditorGUI.DrawPip(new Vector2(X(1f), Y(value)), pipColor, CoreEditorGUI.PipDirection.Left, pipSize);
        }

        // Room above and below the curve for the eases that overshoot.
        private static float OvershootPadding(EaseType type) => type switch
        {
            EaseType.InElastic or EaseType.OutElastic => 30f,
            EaseType.InBack or EaseType.OutBack or EaseType.InOutBack or EaseType.InOutElastic => 10f,
            _ => 0f,
        };

        private static (Color start, Color end) CurveColors(EaseType type)
        {
            if (type == EaseType.Linear) return (CoreEditorGUI.AccentBlue, CoreEditorGUI.AccentGreen);
            string name = type.ToString();
            if (name.StartsWith("InOut", StringComparison.Ordinal)) return (CoreEditorGUI.AccentBlue, CoreEditorGUI.AccentGreen);
            if (name.StartsWith("In", StringComparison.Ordinal)) return (CoreEditorGUI.AccentBlue, CoreEditorGUI.AccentWhite);
            return (CoreEditorGUI.AccentWhite, CoreEditorGUI.AccentGreen);
        }
    }
}
