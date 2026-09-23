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
using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		private void Editor_KeyDown(object sender, KeyEventArgs e)
		{
			if (HandleCompletionKeyDown(e))
				return;
			if (HandleNavigationKeys(e))
				return;
			if (HandleFindPanelEscape(e))
				return;
			if (HandleCtrlCombinations(e))
				return;
			if (HandleAltCombinations(e))
				return;
			if (HandleEditingKeys(e))
				return;
		}

		/// <summary>Возвращает true, если клавиша обработана панелью автодополнения.</summary>
		private bool HandleCompletionKeyDown(KeyEventArgs e)
		{
			if (!_completion.Visible)
				return false;

			switch (e.KeyCode)
			{
				case Keys.Down:
				_completion.MoveSelection(+1);
				break;
				case Keys.Up:
				_completion.MoveSelection(-1);
				break;
				case Keys.Enter:
				case Keys.Tab:
				_completion.Accept();
				break;
				case Keys.Escape:
				_completion.ClosePanel();
				_completionPrefixLength = 0;
				break;
				case Keys.Back:
				if (!_completion.BackspaceChar())
				{
					_completion.ClosePanel();
					_completionPrefixLength = 0;
				}
				return false;   // пусть RichTextBox удалит символ
				case Keys.Left:
				case Keys.Right:
				case Keys.Home:
				case Keys.End:
				_completion.ClosePanel();
				_completionPrefixLength = 0;
				return false;   // не блокируем обычное поведение стрелок
				default:
				return false;
			}

			e.Handled = true;
			e.SuppressKeyPress = true;
			return true;
		}

		private bool HandleNavigationKeys(KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right
			 && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down
			 && e.KeyCode != Keys.Home && e.KeyCode != Keys.End
			 && e.KeyCode != Keys.PageUp && e.KeyCode != Keys.PageDown)
				return false;

			_undo.BreakTypingGroup();
			BeginInvoke(new Action(() => _undo.SyncCaret()));
			return false;   // не блокируем стандартное поведение
		}

		private bool HandleFindPanelEscape(KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Escape || !_findPanel.Visible)
				return false;

			_findPanel.HidePanel();
			e.Handled = true;
			e.SuppressKeyPress = true;
			return true;
		}

		private bool HandleCtrlCombinations(KeyEventArgs e)
		{
			if (!e.Control || e.Alt)
				return false;

			if (!e.Shift && e.KeyCode == Keys.F)
			{ ShowFindReplace(false); }
			else if (!e.Shift && e.KeyCode == Keys.H)
			{ ShowFindReplace(true); }
			else if (!e.Shift && e.KeyCode == Keys.G)
			{ ShowGoToLine(); }
			else if (!e.Shift && e.KeyCode == Keys.Z)
			{ _undo.Undo(); }
			else if (e.KeyCode == Keys.Y || (e.Shift && e.KeyCode == Keys.Z))
			{ _undo.Redo(); }
			else if (e.Shift && e.KeyCode == Keys.K)
			{ _fastHighlightNext = true; DeleteSelectedLines(); }
			else if (!e.Shift && e.KeyCode == Keys.D)
			{ _fastHighlightNext = true; DuplicateSelectedLines(); }
			else if (e.KeyCode == Keys.OemQuestion)
			{
				if (e.Shift)
					UncommentSelectedLines();
				else
					ToggleCommentSelectedLines();
			}
			else
				return false;

			e.Handled = true;
			e.SuppressKeyPress = true;
			return true;
		}

		private bool HandleAltCombinations(KeyEventArgs e)
		{
			if (!e.Alt)
				return false;

			if (e.KeyCode == Keys.Up)
			{
				_fastHighlightNext = true;
				MoveSelectedLines(-1);
				e.Handled = true;
				e.SuppressKeyPress = true;
				return true;
			}
			if (e.KeyCode == Keys.Down)
			{
				_fastHighlightNext = true;
				MoveSelectedLines(+1);
				e.Handled = true;
				e.SuppressKeyPress = true;
				return true;
			}
			return false;
		}

		private bool HandleEditingKeys(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Back:
				_fastHighlightNext = true;
				if (HandleBackspaceKey())
				{
					e.Handled = true;
					e.SuppressKeyPress = true;
					return true;
				}
				return false;

				case Keys.Delete:
				_fastHighlightNext = true;
				return false;

				case Keys.Tab:
				_fastHighlightNext = true;
				HandleTabKey(e.Shift);
				e.Handled = true;
				e.SuppressKeyPress = true;
				return true;

				case Keys.Enter:
				_fastHighlightNext = true;
				InsertNewLineWithIndent();
				e.Handled = true;
				e.SuppressKeyPress = true;
				return true;

				default:
				return false;
			}
		}

		private void Editor_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar < 32)
				return;
			if ((Control.ModifierKeys & Keys.Control) != 0)
				return;
			if ((Control.ModifierKeys & Keys.Alt) != 0)
				return;

			char c = e.KeyChar;

			if (HandleCompletionKeyPress(c))
			{ /* продолжаем — RichTextBox вставит символ */ }
			else if (c == '(' || c == '[' || c == '{' || c == '"' || c == '\'')
				AutoCloseOpening(e, c);
			else if (c == ')' || c == ']' || c == '}' || c == '"' || c == '\'')
				AutoCloseClosing(e, c);
		}

		private void AutoCloseOpening(KeyPressEventArgs e, char open)
		{
			char close;
			switch (open)
			{
				case '(':
				close = ')';
				break;
				case '[':
				close = ']';
				break;
				case '{':
				close = '}';
				break;
				case '"':
				close = '"';
				break;
				case '\'':
				close = '\'';
				break;
				default:
				return;
			}
			HandleOpening(e, open, close);
		}

		private void AutoCloseClosing(KeyPressEventArgs e, char close)
		{
			HandleClosing(e, close);
		}

		/// <summary>
		/// Обработка нажатия открывающего символа ( [ { " '.
		/// Либо оборачиваем выделение, либо вставляем пустую пару с курсором внутри.
		/// </summary>
		private void HandleOpening(KeyPressEventArgs e, char open, char close)
		{
			// Случай 1: есть выделение — оборачиваем его.
			if (_editor.SelectionLength > 0)
			{
				string selected = _editor.SelectedText;

				// Для кавычек: если внутри строки уже есть такая же кавычка,
				// просто обернём и закроем — норм.
				string wrapped = open + selected + close;

				_undo.BeginChange();
				_fastHighlightNext = true;

				_editor.SelectedText = wrapped;
				// Курсор после закрывающего символа.
				_editor.SelectionStart -= close.ToString().Length;
				_editor.SelectionLength = 0;

				e.Handled = true;
				return;
			}

			int caret = _editor.SelectionStart;
			string text = _editor.Text;

			// Случай 2: обычная вставка пары.
			// Overtype для открывающих символов НЕ применяется — пользователь
			// набирает именно открывающую, даже если справа уже стоит закрывающая.
			// Отдельная логика для кавычек: не вставлять вторую кавычку,
			// если мы уже внутри строкового литерала.
			if ((open == '"' || open == '\'') && IsInsideStringLiteral(open))
			{
				// Просто вставляем одиночную кавычку — закрытие пользователь сделает сам.
				return;
			}

			_undo.BeginChange();
			_fastHighlightNext = true;

			string pair = open.ToString() + close.ToString();
			_editor.SelectedText = pair;
			// Курсор между кавычками.
			_editor.SelectionStart = _editor.SelectionStart - 1;
			_editor.SelectionLength = 0;

			e.Handled = true;
		}

		/// <summary>
		/// Грубая проверка: находимся ли мы уже внутри строкового литерала,
		/// ограниченного символом quote (' или ").
		/// Считает количество непарных quote в текущей строке до каретки.
		/// Игнорирует экранированные \" и \'.
		/// </summary>
		private bool IsInsideStringLiteral(char quote)
		{
			int caret = _editor.SelectionStart;
			int line = _editor.GetLineFromCharIndex(caret);
			int lineStart = _editor.GetFirstCharIndexFromLine(line);
			if (lineStart < 0)
				return false;

			int col = caret - lineStart;
			string[] lines = _editor.Lines;
			if (line >= lines.Length)
				return false;
			string t = lines[line];
			if (col > t.Length)
				col = t.Length;

			int count = 0;
			for (int i = 0; i < col; i++)
			{
				if (t[i] == '\\')
				{
					i++;   // пропускаем экранированный символ
					continue;
				}
				if (t[i] == quote)
					count++;
			}

			return (count % 2) == 1;
		}

		/// <summary>
		/// Обработка нажатия закрывающего символа. Если курсор стоит сразу
		/// перед таким же символом — не вставляем второй, просто двигаем курсор.
		/// </summary>
		private void HandleClosing(KeyPressEventArgs e, char close)
		{
			int caret = _editor.SelectionStart;

			// Overtype работает только при пустом выделении.
			if (_editor.SelectionLength == 0 && caret < _editor.TextLength)
			{
				if (_editor.Text[caret] == close)
				{
					_editor.SelectionStart = caret + 1;
					_editor.SelectionLength = 0;
					e.Handled = true;
				}
			}
		}
	}
}
