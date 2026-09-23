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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor : UserControl
	{
		private readonly ScrollRichTextBox _editor;
		private readonly BufferedPanel _gutter;
		private readonly SyntaxHighlighter _highlighter;
		private readonly UndoManager _undo;
		private readonly FindReplacePanel _findPanel;
		private readonly CompletionPanel _completion;
		private readonly Timer _scrollSyncTimer;
		private readonly string _indent;

		private int _lineHeight;
		private bool _fastHighlightNext;
		private int _completionPrefixLength;

        static CodeEditor()
        {
            try
            {
				Assembly[] assemblies = new Assembly[]
                {
                    typeof(DisplayNodes.Core.LayoutNode).Assembly,
                    typeof(LibDisplayDrawing.IComponent).Assembly,
                    typeof(System.Drawing.Color).Assembly,
                    typeof(DisplayNodes.Gdi.GdiFont).Assembly
                };

                var docProvider = new XmlDocProvider();
                docProvider.LoadFromAssemblies(assemblies);

                ApiIndex.Initialize(docProvider, assemblies);
            }
            catch (Exception ex) { AppLog.Error("CodeEditor.ApiIndexInit", ex); }
        }

        public CodeEditor(EditorSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			_indent = settings.IndentString;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			BackColor = Color.FromArgb(30, 30, 30);

			_gutter = new BufferedPanel { Dock = DockStyle.Left, Width = 46, BackColor = Color.FromArgb(30, 30, 30) };
			_gutter.Paint += Gutter_Paint;
			_gutter.MouseDown += Gutter_MouseDown;

			_scrollSyncTimer = new Timer { Interval = settings.UpdateTimerInterval };
			_scrollSyncTimer.Tick += (s, e) => _gutter.Invalidate();

			_editor = new ScrollRichTextBox
			{
				Dock = DockStyle.Fill,
				Font = settings.GetFont(),
				BackColor = Color.FromArgb(30, 30, 30),
				ForeColor = Color.Gainsboro,
				BorderStyle = BorderStyle.None,
				WordWrap = false,
				ScrollBars = RichTextBoxScrollBars.Both,
				AcceptsTab = true,
				HideSelection = false,
				DetectUrls = false,
				IndentString = _indent,
                DrawIndentGuides = settings.ShowIndentGuides,
                DrawErrorUnderlines = settings.ShowErrorUnderlines,
                DrawCurrentLineHighlight = settings.ShowCurrentLineHighlight,
				ShowLineNumbers = settings.ShowLineNumbers,
            };

			_highlighter = new SyntaxHighlighter(_editor, settings);
			_undo = new UndoManager(_editor, settings);
			_findPanel = new FindReplacePanel { Visible = false };
			_completion = new CompletionPanel { Visible = false };
			_completion.ItemAccepted += Completion_ItemAccepted;

			_lineHeight = _editor.Font.Height;

			HookEditorEvents();
			HookMouseEvents();
			HookResizeEvents();

			Controls.Add(_editor);
			Controls.Add(_gutter);
			Controls.Add(_completion);
			Controls.Add(_findPanel);

			UpdateGutterWidth();
		}

		public RichTextBox Editor => _editor;

		public override string Text
		{
			get => _editor.Text;
			set => _editor.Text = value;
		}

		/// <summary>
		/// Установить список ошибочных строк (1-based номера строк в пользовательском
		/// коде). Передайте null или пустой список, чтобы очистить подсветку.
		/// </summary>
		public void SetErrorLines(IEnumerable<int> lines)
		{
			_editor.SetErrorLines(lines);
		}

		/// <summary>
		/// Полностью стереть историю undo/redo. Вызывать после загрузки файла
		/// из внешнего источника, чтобы Ctrl+Z не откатывал к состоянию до загрузки.
		/// </summary>
		public void ResetUndo()
		{
			_undo.Reset();
		}

		private void HookEditorEvents()
		{
			_editor.TextChanged += Editor_TextChanged;
			_editor.TextChanged += OnEditorTextChangedForHighlighter;
			_editor.KeyDown += Editor_KeyDown;
			_editor.KeyPress += Editor_KeyPress;
			_editor.FontChanged += OnEditorFontChanged;
		}

		private void HookMouseEvents()
		{
			_editor.Scrolled += OnEditorScrolled;
			_editor.SelectionChanged += OnEditorSelectionChanged;
			_editor.MouseUp += OnEditorMouseUp;
			_editor.MouseDown += OnEditorMouseDown;
		}

		private void HookResizeEvents()
		{
			_editor.Resize += OnEditorResize;
		}

		private void OnEditorScrolled(object sender, EventArgs e)
		{
			_gutter.Invalidate();          // Мгновенная пометка на перерисовку
			_scrollSyncTimer.Stop();       // Сброс таймера
			_scrollSyncTimer.Start();      // Перезапуск: жёлоб будет перерисовываться каждые 16 мс, пока идёт скролл
			_editor.Invalidate();
			RepositionFindPanel();
		}

		private void OnEditorSelectionChanged(object sender, EventArgs e)
		{
			if (!_highlighter.IsApplying)
				_gutter.Invalidate();
		}

		private void OnEditorTextChangedForHighlighter(object sender, EventArgs e)
		{
			_highlighter.RequestHighlight(_fastHighlightNext);
			_fastHighlightNext = false;
			_undo.RecordTyping();
			_editor.Invalidate();
		}

		private void OnEditorMouseUp(object sender, MouseEventArgs e)
		{
			_undo.SyncCaret();
			_undo.BreakTypingGroup();
		}

		private void OnEditorMouseDown(object sender, MouseEventArgs e)
		{
			if (_completion.Visible)
			{
				_completion.ClosePanel();
				_completionPrefixLength = 0;
			}
		}

		private void OnEditorFontChanged(object sender, EventArgs e)
		{
			_lineHeight = _editor.Font.Height;
			_gutter.Invalidate();
		}

		private void OnEditorResize(object sender, EventArgs e)
		{
			_gutter.Invalidate();
			RepositionFindPanel();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_highlighter?.Dispose();
				_scrollSyncTimer?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
