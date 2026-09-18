using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TimboJimboEditor.Core
{
	/// <summary>
	/// Shared foldout header used across Timbo Jimbo packages so inspectors look like one family.
	///
	/// The header is a full-bleed band with a divider above it and the foldout arrow on the left.
	/// What goes <em>in</em> the band is up to the caller (<c>drawContent</c>); <see cref="Title"/>
	/// draws the standard primary/secondary label pair for callers that want the common look.
	/// Clicking anywhere on the band toggles; alt-click reports a "group" toggle so lists of
	/// foldouts can expand or collapse together.
	///
	/// <see cref="BeginContent"/>/<see cref="EndContent"/> optionally indent the body beneath a
	/// header, and <see cref="State"/> / <see cref="SessionBool"/> provide expanded-state storage.
	/// </summary>
	public static class FoldoutGUI
	{
		// ── Header ───────────────────────────────────────────────────────────────

		/// <summary>Draws a foldout header and toggles <paramref name="expanded"/> in place when clicked.</summary>
		public static void Draw(ref bool expanded, Action drawContent, Action<bool> onToggle = null, Action<bool> onGroupToggle = null)
		{
			expanded = Draw(expanded, drawContent, onToggle, onGroupToggle);
		}

		/// <summary>
		/// Draws a foldout header. Returns the new expanded state; <paramref name="onToggle"/> (or
		/// <paramref name="onGroupToggle"/> for an alt-click) is invoked when it changes, so callers
		/// that persist the state elsewhere can ignore the return value.
		/// </summary>
		public static bool Draw(bool expanded, Action drawContent, Action<bool> onToggle = null, Action<bool> onGroupToggle = null)
		{
			onGroupToggle ??= onToggle;

			// BeginHorizontal with a styled background paints the band behind the child controls.
			// The style's left padding reserves room for the arrow, so content starts to its right.
			using (var scope = new EditorGUILayout.HorizontalScope(Styles.RowStyle, GUILayout.ExpandWidth(true)))
			{
				Event evt = Event.current;

				// A fixed oversized bleed reliably covers the full inspector width regardless of
				// sidebars, scroll bars, or currentViewWidth quirks.
				const float BleedAmount = 4000f;
				var rowRect = scope.rect;
				var bleedRect = new Rect(rowRect.x - BleedAmount, rowRect.y, rowRect.width + BleedAmount * 2f, rowRect.height);

				if (evt.type == EventType.Repaint)
				{
					EditorGUI.DrawRect(bleedRect, Styles.BackgroundColor);
					EditorGUI.DrawRect(new Rect(bleedRect.x, rowRect.y, bleedRect.width, Styles.TopBorderThickness), Styles.BorderColor);

					float arrowHeight = EditorGUIUtility.singleLineHeight;
					var arrowRect = new Rect(rowRect.x, rowRect.y + (rowRect.height - arrowHeight) * 0.5f, 13f, arrowHeight);
					EditorStyles.foldout.Draw(arrowRect, GUIContent.none, false, false, expanded, false);
				}

				using (new GUILayout.HorizontalScope(GUILayout.MinHeight(EditorGUIUtility.singleLineHeight + 2f)))
				{
					drawContent?.Invoke();
				}

				// Inner controls (kebab buttons etc.) get the event first; if they consumed it,
				// evt.type is Used by the time we get here.
				if (evt.type == EventType.MouseDown && evt.button == 0 && bleedRect.Contains(evt.mousePosition))
				{
					bool group = evt.alt;
					GUI.changed = true;
					evt.Use();

					expanded = !expanded;
					if (group)
						onGroupToggle?.Invoke(expanded);
					else
						onToggle?.Invoke(expanded);
				}
			}

			return expanded;
		}

		// ── Header labels ────────────────────────────────────────────────────────

		/// <summary>
		/// The standard header label pair: a bold primary title and an optional subtle secondary
		/// label beside it (e.g. "French" and "fr • français"). Call from within <c>drawContent</c>.
		/// Does not add a FlexibleSpace; callers place trailing controls themselves.
		/// </summary>
		public static void Title(string title, string subtitle = null)
		{
			Title(new GUIContent(title), string.IsNullOrEmpty(subtitle) ? null : new GUIContent(subtitle));
		}

		public static void Title(GUIContent title, GUIContent subtitle = null)
		{
			GUILayout.Label(title, Styles.TitleStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

			if (subtitle != null && (!string.IsNullOrEmpty(subtitle.text) || subtitle.image != null))
			{
				GUILayout.Space(Styles.TitleSubtitleGap);
				GUILayout.Label(subtitle, Styles.SubtitleStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
			}
		}

		public static GUIStyle TitleStyle => Styles.TitleStyle;
		public static GUIStyle SubtitleStyle => Styles.SubtitleStyle;

		// ── Content ──────────────────────────────────────────────────────────────

		private static readonly Stack<float> s_labelWidthStack = new();

		/// <summary>
		/// Begins the body beneath a header: indented one level with consistent top/bottom padding.
		/// Label width shrinks by the indent so field columns stay aligned with rows outside the
		/// section, exactly like <c>EditorGUI.indentLevel</c>. Pair with <see cref="EndContent"/>.
		/// </summary>
		public static void BeginContent()
		{
			EditorGUILayout.BeginVertical(Styles.ContentStyle);
			s_labelWidthStack.Push(EditorGUIUtility.labelWidth);
			EditorGUIUtility.labelWidth = Mathf.Max(0f, EditorGUIUtility.labelWidth - Styles.ContentIndent);
		}

		public static void EndContent()
		{
			if (s_labelWidthStack.Count > 0)
				EditorGUIUtility.labelWidth = s_labelWidthStack.Pop();
			EditorGUILayout.EndVertical();
		}

		// ── Header furniture ─────────────────────────────────────────────────────

		public static bool KebabMenuButton(string label = null, string tooltip = null) => GhostButton("_Menu", label, tooltip);
		public static bool RemoveButton(string label = null, string tooltip = null) => GhostButton("Toolbar Minus", label, tooltip);
		public static bool AddButton(string label = null, string tooltip = null) => GhostButton("Toolbar Plus", label, tooltip);

		/// <summary>
		/// A small icon button with a hover highlight, sized to its icon (and optional label).
		/// The icon is looked up via <see cref="EditorGUIUtility.IconContent(string)"/>.
		/// </summary>
		public static bool GhostButton(string iconName, string label = null, string tooltip = null)
		{
			GUIContent icon = EditorGUIUtility.IconContent(iconName);
			var content = new GUIContent(label ?? icon.text, icon.image, tooltip ?? icon.tooltip);
			return GUILayout.Button(content, Styles.GhostIconStyle, GUILayout.ExpandWidth(false));
		}

		// ── State ────────────────────────────────────────────────────────────────

		/// <summary>
		/// Session-scoped expanded state keyed by a target object plus a caller-chosen key.
		/// Assets key by GUID (stable across reloads); scene objects key by entity id.
		/// </summary>
		public static class State
		{
			public static bool Get(Object target, string key, bool defaultValue) => SessionState.GetBool(KeyFor(target, key), defaultValue);
			public static void Set(Object target, string key, bool value) => SessionState.SetBool(KeyFor(target, key), value);

			public static string KeyFor(Object target, string key)
			{
				string id = "none";
				if (target != null)
				{
					string assetPath = AssetDatabase.GetAssetPath(target);
					id = string.IsNullOrEmpty(assetPath) ? target.GetEntityId().ToString() : AssetDatabase.AssetPathToGUID(assetPath);
				}
				return $"TimboJimbo.Foldout.{id}.{key}";
			}
		}

		/// <summary>
		/// Per-object expanded flag that also remembers the user's last choice as the default for
		/// objects it has not seen yet (via EditorPrefs), so a section you keep collapsing stays
		/// collapsed on the next component you inspect.
		/// </summary>
		public sealed class SessionBool
		{
			private readonly string _key;
			private readonly bool _defaultExpanded;
			private readonly HashSet<int> _initialised = new();

			public SessionBool(string key, bool defaultExpanded = true)
			{
				_key = key;
				_defaultExpanded = defaultExpanded;
			}

			public bool Get(SerializedObject target) => Get(target.targetObject);
			public bool Set(SerializedObject target, bool value) => Set(target.targetObject, value);

			public bool Get(Object target)
			{
				EnsureInit(target);
				bool defaultExpanded = EditorPrefs.GetBool($"TimboJimbo.Foldout.Default.{_key}", _defaultExpanded);
				return SessionState.GetBool(InstanceKey(target), defaultExpanded);
			}

			public bool Set(Object target, bool value)
			{
				SessionState.SetBool(InstanceKey(target), value);
				EditorPrefs.SetBool($"TimboJimbo.Foldout.Default.{_key}", value);
				return value;
			}

			private string InstanceKey(Object target) => $"TimboJimbo.Foldout.{(target != null ? target.GetEntityId().ToString() : "none")}.{_key}";

			private void EnsureInit(Object target)
			{
				int id = target != null ? target.GetEntityId().GetHashCode() : 0;
				if (_initialised.Add(id))
					Set(target, Get(target));
			}
		}

		// ── Styles ───────────────────────────────────────────────────────────────

		public static class Styles
		{
			private static readonly Color BackgroundColorDarkSkin = new(0.19f, 0.19f, 0.19f, 1f);
			private static readonly Color BackgroundColorLightSkin = new(0.74f, 0.74f, 0.74f, 1f);
			private static readonly Color BorderColorDarkSkin = new(0f, 0f, 0f, 0.38f);
			private static readonly Color BorderColorLightSkin = new(0f, 0f, 0f, 0.18f);
			private static readonly Color SubtleTextColorDarkSkin = new(0.70f, 0.70f, 0.70f, 1f);
			private static readonly Color SubtleTextColorLightSkin = new(0.35f, 0.35f, 0.35f, 1f);

			public const float TopBorderThickness = 1f;
			public const float RowLeftPadding = 22f;   // room for the arrow
			public const float RowRightPadding = 8f;
			public const float RowVerticalPadding = 5f;
			public const float TitleSubtitleGap = 6f;

			public const float ContentIndent = 15f;    // matches EditorGUI.indentLevel
			public const float ContentTopPadding = 6f;
			public const float ContentBottomPadding = 8f;

			private static GUIStyle _rowStyle;
			private static GUIStyle _contentStyle;
			private static GUIStyle _titleStyle;
			private static GUIStyle _subtitleStyle;
			private static GUIStyle _ghostIconStyle;
			private static Texture2D _backgroundTexture;
			private static bool _builtForProSkin;

			public static Color BackgroundColor => ForSkin(BackgroundColorDarkSkin, BackgroundColorLightSkin);
			public static Color BorderColor => ForSkin(BorderColorDarkSkin, BorderColorLightSkin);
			public static Color SubtleTextColor => ForSkin(SubtleTextColorDarkSkin, SubtleTextColorLightSkin);

			public static GUIStyle RowStyle { get { Ensure(); return _rowStyle; } }
			public static GUIStyle ContentStyle { get { Ensure(); return _contentStyle; } }
			public static GUIStyle TitleStyle { get { Ensure(); return _titleStyle; } }
			public static GUIStyle SubtitleStyle { get { Ensure(); return _subtitleStyle; } }
			public static GUIStyle GhostIconStyle { get { Ensure(); return _ghostIconStyle; } }

			private static Color ForSkin(Color dark, Color light) => EditorGUIUtility.isProSkin ? dark : light;

			private static void Ensure()
			{
				bool pro = EditorGUIUtility.isProSkin;
				if (_rowStyle != null && _builtForProSkin == pro)
					return;

				_builtForProSkin = pro;

				if (_backgroundTexture != null)
					Object.DestroyImmediate(_backgroundTexture);

				_backgroundTexture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
				_backgroundTexture.SetPixel(0, 0, BackgroundColor);
				_backgroundTexture.Apply();

				_rowStyle = new GUIStyle
				{
					normal = { background = _backgroundTexture },
					padding = new RectOffset((int)RowLeftPadding, (int)RowRightPadding, (int)(TopBorderThickness + RowVerticalPadding), (int)RowVerticalPadding),
					margin = new RectOffset(0, 0, 0, 0),
					stretchWidth = true,
				};

				_contentStyle = new GUIStyle
				{
					padding = new RectOffset((int)ContentIndent, 0, (int)ContentTopPadding, (int)ContentBottomPadding),
					margin = new RectOffset(0, 0, 0, 0),
					stretchWidth = true,
				};

				_titleStyle = new GUIStyle(EditorStyles.label)
				{
					fontSize = 12,
					fontStyle = FontStyle.Bold,
					clipping = TextClipping.Clip,
					alignment = TextAnchor.MiddleLeft,
				};

				_subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
				{
					normal = { textColor = SubtleTextColor },
					clipping = TextClipping.Clip,
					alignment = TextAnchor.MiddleLeft,
				};

				_ghostIconStyle = new GUIStyle(EditorStyles.iconButton)
				{
					alignment = TextAnchor.MiddleCenter,
					imagePosition = ImagePosition.ImageLeft,
					fontSize = EditorStyles.label.fontSize,
					fixedWidth = 0f,
					fixedHeight = EditorGUIUtility.singleLineHeight,
				};
			}
		}
	}
}
