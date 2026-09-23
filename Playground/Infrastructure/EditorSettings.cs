///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

using DisplayNodes.Playground.Compilation;
using System.Drawing;

namespace DisplayNodes.Playground.Infrastructure
{
	public sealed class EditorSettings
    {
        #region RUNTIME
        public int UndoMaxSteps { get; set; } = 200;
		public int TypingGroupMs { get; set; } = 500;
		public int AutoRunDebounceMs { get; set; } = 500;
		public int LayoutDebounceMs { get; set; } = 200;
		public int UpdateTimerInterval { get; set; } = 16;
		public int HighlightSlowMs { get; set; } = 300;
		public int HighlightFastMs { get; set; } = 50;
		public int RecentFilesMax { get; set; } = 10;
		public int WrapperLineOffset { get; set; } = ScriptWrapper.HEADER_LINE_COUNT;
        #endregion // RUNTIME

        #region SPACING
        public string IndentString { get; set; } = "    ";
        public bool UseTabs { get; set; } = false;
        public int IndentSize { get; set; } = 4;
        #endregion // SPACING

        #region FONT
        public string FontFamily { get; set; } = "Consolas";
        public float FontSize { get; set; } = 10.8f;
        #endregion // FONT

        #region UI FLAGS
        public bool ShowLineNumbers { get; set; } = true;
        public bool ShowIndentGuides { get; set; } = true;
        public bool ShowErrorUnderlines { get; set; } = true;
        public bool ShowCurrentLineHighlight { get; set; } = true;
        #endregion // UI FLAGS

        #region PREVIEW
        public int PreviewDefaultWidth { get; set; } = 800;
        public int PreviewDefaultHeight { get; set; } = 600;
        #endregion // PREVIEW

        #region COLORS
        public Color ColorDefault { get; set; } = Color.FromArgb(0xDC, 0xDC, 0xDC);
        public Color ColorKeyword { get; set; } = Color.FromArgb(0x56, 0x9C, 0xD6);
        public Color ColorControlFlow { get; set; } = Color.FromArgb(0xD8, 0xA0, 0xDF);
        public Color ColorType { get; set; } = Color.FromArgb(0x4E, 0xC9, 0xB0);
        public Color ColorInterface { get; set; } = Color.FromArgb(0xB8, 0xD7, 0xA3);
        public Color ColorMethod { get; set; } = Color.FromArgb(0xDC, 0xDC, 0xAA);
        public Color ColorStruct { get; set; } = Color.FromArgb(0x86, 0xC6, 0x91);
        public Color ColorString { get; set; } = Color.FromArgb(0xD6, 0x9D, 0x85);
        public Color ColorComment { get; set; } = Color.FromArgb(0x57, 0xA6, 0x4A);
        public Color ColorNumber { get; set; } = Color.FromArgb(0xB5, 0xCE, 0xA8);
        public Color ColorPreprocessor { get; set; } = Color.FromArgb(0x9B, 0x9B, 0x9B);
        #endregion // COLORS

        public Font GetFont()
        {
            try { return new Font(FontFamily, FontSize); }
            catch { return new Font("Consolas", 10.8f); }
        }

        internal void Load(AppSettings s)
        {
            if (s == null) return;

            UndoMaxSteps = s.GetInt(StateKeys.EDITOR_UNDO_MAX_STEPS, UndoMaxSteps);
            TypingGroupMs = s.GetInt(StateKeys.EDITOR_TYPING_GROUP_MS, TypingGroupMs);
            AutoRunDebounceMs = s.GetInt(StateKeys.EDITOR_AUTO_RUN_DEBOUNCE_MS, AutoRunDebounceMs);
            LayoutDebounceMs = s.GetInt(StateKeys.EDITOR_LAYOUT_DEBOUNCE_MS, LayoutDebounceMs);
            UpdateTimerInterval = s.GetInt(StateKeys.EDITOR_UPDATE_TIMER_INTERVAL, UpdateTimerInterval);
            HighlightSlowMs = s.GetInt(StateKeys.EDITOR_HIGHLIGHT_SLOW_MS, HighlightSlowMs);
            HighlightFastMs = s.GetInt(StateKeys.EDITOR_HIGHLIGHT_FAST_MS, HighlightFastMs);
            RecentFilesMax = s.GetInt(StateKeys.EDITOR_RECENT_FILES_MAX, RecentFilesMax);
            WrapperLineOffset = s.GetInt(StateKeys.EDITOR_WRAPPER_LINE_OFFSET, WrapperLineOffset);

            IndentSize = s.GetInt(StateKeys.EDITOR_INDENT_SIZE, IndentSize);
            UseTabs = s.GetBool(StateKeys.EDITOR_USE_TABS, UseTabs);

            FontFamily = s.GetString(StateKeys.EDITOR_FONT_FAMILY, FontFamily) ?? "Consolas";
            FontSize = (float)s.GetDouble(StateKeys.EDITOR_FONT_SIZE, FontSize);

            ShowLineNumbers = s.GetBool(StateKeys.EDITOR_SHOW_LINE_NUMBERS, ShowLineNumbers);
            ShowIndentGuides = s.GetBool(StateKeys.EDITOR_SHOW_INDENT_GUIDES, ShowIndentGuides);
            ShowErrorUnderlines = s.GetBool(StateKeys.EDITOR_SHOW_ERROR_UNDERLINES, ShowErrorUnderlines);
            ShowCurrentLineHighlight = s.GetBool(StateKeys.EDITOR_SHOW_CURRENT_LINE_HIGHLIGHT, ShowCurrentLineHighlight);

            PreviewDefaultWidth = s.GetInt(StateKeys.EDITOR_PREVIEW_WIDTH, PreviewDefaultWidth);
            PreviewDefaultHeight = s.GetInt(StateKeys.EDITOR_PREVIEW_HEIGHT, PreviewDefaultHeight);

            ColorDefault = LoadColor(s, StateKeys.COLOR_DEFAULT, ColorDefault);
            ColorKeyword = LoadColor(s, StateKeys.COLOR_KEYWORD, ColorKeyword);
            ColorControlFlow = LoadColor(s, StateKeys.COLOR_CONTROL_FLOW, ColorControlFlow);
            ColorType = LoadColor(s, StateKeys.COLOR_TYPE, ColorType);
            ColorInterface = LoadColor(s, StateKeys.COLOR_INTERFACE, ColorInterface);
            ColorMethod = LoadColor(s, StateKeys.COLOR_METHOD, ColorMethod);
            ColorStruct = LoadColor(s, StateKeys.COLOR_STRUCT, ColorStruct);
            ColorString = LoadColor(s, StateKeys.COLOR_STRING, ColorString);
            ColorComment = LoadColor(s, StateKeys.COLOR_COMMENT, ColorComment);
            ColorNumber = LoadColor(s, StateKeys.COLOR_NUMBER, ColorNumber);
            ColorPreprocessor = LoadColor(s, StateKeys.COLOR_PREPROCESSOR, ColorPreprocessor);
        }

        internal void Save(AppSettings s)
        {
            if (s == null) return;

            s.SetInt(StateKeys.EDITOR_UNDO_MAX_STEPS, UndoMaxSteps);
            s.SetInt(StateKeys.EDITOR_TYPING_GROUP_MS, TypingGroupMs);
            s.SetInt(StateKeys.EDITOR_AUTO_RUN_DEBOUNCE_MS, AutoRunDebounceMs);
            s.SetInt(StateKeys.EDITOR_LAYOUT_DEBOUNCE_MS, LayoutDebounceMs);
            s.SetInt(StateKeys.EDITOR_UPDATE_TIMER_INTERVAL, UpdateTimerInterval);
            s.SetInt(StateKeys.EDITOR_HIGHLIGHT_SLOW_MS, HighlightSlowMs);
            s.SetInt(StateKeys.EDITOR_HIGHLIGHT_FAST_MS, HighlightFastMs);
            s.SetInt(StateKeys.EDITOR_RECENT_FILES_MAX, RecentFilesMax);
            s.SetInt(StateKeys.EDITOR_WRAPPER_LINE_OFFSET, WrapperLineOffset);

            s.SetInt(StateKeys.EDITOR_INDENT_SIZE, IndentSize);
            s.SetBool(StateKeys.EDITOR_USE_TABS, UseTabs);

            s.SetString(StateKeys.EDITOR_FONT_FAMILY, FontFamily);
            s.SetDouble(StateKeys.EDITOR_FONT_SIZE, FontSize);

            s.SetBool(StateKeys.EDITOR_SHOW_LINE_NUMBERS, ShowLineNumbers);
            s.SetBool(StateKeys.EDITOR_SHOW_INDENT_GUIDES, ShowIndentGuides);
            s.SetBool(StateKeys.EDITOR_SHOW_ERROR_UNDERLINES, ShowErrorUnderlines);
            s.SetBool(StateKeys.EDITOR_SHOW_CURRENT_LINE_HIGHLIGHT, ShowCurrentLineHighlight);

            s.SetInt(StateKeys.EDITOR_PREVIEW_WIDTH, PreviewDefaultWidth);
            s.SetInt(StateKeys.EDITOR_PREVIEW_HEIGHT, PreviewDefaultHeight);

            SaveColor(s, StateKeys.COLOR_DEFAULT, ColorDefault);
            SaveColor(s, StateKeys.COLOR_KEYWORD, ColorKeyword);
            SaveColor(s, StateKeys.COLOR_CONTROL_FLOW, ColorControlFlow);
            SaveColor(s, StateKeys.COLOR_TYPE, ColorType);
            SaveColor(s, StateKeys.COLOR_INTERFACE, ColorInterface);
            SaveColor(s, StateKeys.COLOR_METHOD, ColorMethod);
            SaveColor(s, StateKeys.COLOR_STRUCT, ColorStruct);
            SaveColor(s, StateKeys.COLOR_STRING, ColorString);
            SaveColor(s, StateKeys.COLOR_COMMENT, ColorComment);
            SaveColor(s, StateKeys.COLOR_NUMBER, ColorNumber);
            SaveColor(s, StateKeys.COLOR_PREPROCESSOR, ColorPreprocessor);
        }

        private static Color LoadColor(AppSettings s, string key, Color fallback)
        {
            string v = s.GetString(key, null);
            if (string.IsNullOrEmpty(v)) return fallback;
            // Формат: "R,G,B,A"
            var parts = v.Split(',');
            if (parts.Length < 3) return fallback;
            try
            {
                int r = int.Parse(parts[0]);
                int g = int.Parse(parts[1]);
                int b = int.Parse(parts[2]);
                int a = parts.Length > 3 ? int.Parse(parts[3]) : 255;
                return Color.FromArgb(a, r, g, b);
            }
            catch { return fallback; }
        }

        private static void SaveColor(AppSettings s, string key, Color c)
        {
            s.SetString(key, string.Format("{0},{1},{2},{3}", c.R, c.G, c.B, c.A));
        }
    }
}
