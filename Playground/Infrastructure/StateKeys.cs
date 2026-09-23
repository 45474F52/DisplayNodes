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

namespace DisplayNodes.Playground.Infrastructure
{
	internal static class StateKeys
	{
        #region WINDOW
        public const string WINDOW_X = "Window.X";
		public const string WINDOW_Y = "Window.Y";
		public const string WINDOW_WIDTH = "Window.Width";
		public const string WINDOW_HEIGHT = "Window.Height";
		public const string WINDOW_STATE = "Window.State";
        #endregion // WINDOW

        #region SPLITTER
        public const string SPLITTER_MAIN_RATIO = "Splitter.MainRatio";
		public const string SPLITTER_VIEW_LOGS_RATIO = "Splitter.ViewLogsRatio";
        #endregion // SPLITTER

        #region GENERAL
        public const string LAST_FILE = "LastFile";
		public const string AUTO_RUN = "AutoRun";
        #endregion // GENERAL

        #region EDITOR
        public const string EDITOR_PREVIEW_WIDTH = "Editor.PreviewWidth";
        public const string EDITOR_PREVIEW_HEIGHT = "Editor.PreviewHeight";

        public const string EDITOR_UNDO_MAX_STEPS = "Editor.UndoMaxSteps";
        public const string EDITOR_TYPING_GROUP_MS = "Editor.TypingGroupMs";
        public const string EDITOR_AUTO_RUN_DEBOUNCE_MS = "Editor.AutoRunDebounceMs";
        public const string EDITOR_LAYOUT_DEBOUNCE_MS = "Editor.LayoutDebounceMs";
        public const string EDITOR_UPDATE_TIMER_INTERVAL = "Editor.UpdateTimerInterval";
        public const string EDITOR_HIGHLIGHT_SLOW_MS = "Editor.HighlightSlowMs";
        public const string EDITOR_HIGHLIGHT_FAST_MS = "Editor.HighlightFastMs";
        public const string EDITOR_RECENT_FILES_MAX = "Editor.RecentFilesMax";
        public const string EDITOR_WRAPPER_LINE_OFFSET = "Editor.WrapperLineOffset";
        public const string EDITOR_INDENT_STRING = "Editor.IndentString";

        public const string EDITOR_FONT_FAMILY = "Editor.FontFamily";
        public const string EDITOR_FONT_SIZE = "Editor.FontSize";
        public const string EDITOR_INDENT_SIZE = "Editor.IndentSize";
        public const string EDITOR_USE_TABS = "Editor.UseTabs";
        public const string EDITOR_SHOW_LINE_NUMBERS = "Editor.ShowLineNumbers";
        public const string EDITOR_SHOW_INDENT_GUIDES = "Editor.ShowIndentGuides";
        public const string EDITOR_SHOW_ERROR_UNDERLINES = "Editor.ShowErrorUnderlines";
        public const string EDITOR_SHOW_CURRENT_LINE_HIGHLIGHT = "Editor.ShowCurrentLineHighlight";
        #endregion // EDITOR

        #region COLORS
        public const string COLOR_DEFAULT = "Color.Default";
        public const string COLOR_KEYWORD = "Color.Keyword";
        public const string COLOR_CONTROL_FLOW = "Color.ControlFlow";
        public const string COLOR_TYPE = "Color.Type";
        public const string COLOR_INTERFACE = "Color.Interface";
        public const string COLOR_METHOD = "Color.Method";
        public const string COLOR_STRUCT = "Color.Struct";
        public const string COLOR_STRING = "Color.String";
        public const string COLOR_COMMENT = "Color.Comment";
        public const string COLOR_NUMBER = "Color.Number";
        public const string COLOR_PREPROCESSOR = "Color.Preprocessor";
        #endregion // COLORS
    }
}
