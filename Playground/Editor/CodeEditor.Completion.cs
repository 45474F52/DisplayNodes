using System.Collections.Generic;
using System.Drawing;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		/// <summary>
		/// Обработка ввода при открытой/закрытой панели автодополнения.
		/// Возвращает true, если символ ушёл в фильтр (RichTextBox должен его вставить).
		/// </summary>
		private bool HandleCompletionKeyPress(char c)
		{
			if (c == '.' && !_completion.Visible)
			{
				if (TryOpenCompletion())
				{
					_completionPrefixLength = 0;
					return true;
				}
				return false;
			}

			if (!_completion.Visible)
				return false;

			if (char.IsLetterOrDigit(c) || c == '_')
			{
				if (!_completion.TypeChar(c))
					_completion.ClosePanel();
				else
					_completionPrefixLength++;
				return true;
			}

			_completion.ClosePanel();
			_completionPrefixLength = 0;
			return false;
		}

		private void Completion_ItemAccepted(string name)
		{
			int caret = _editor.SelectionStart;
			int prefixLen = _completionPrefixLength;

			_editor.Select(caret - prefixLen, prefixLen);
			_editor.SelectedText = name;
			_editor.SelectionLength = 0;
			_editor.Focus();

			_undo.SyncCaret();
			_undo.BreakTypingGroup();
			_fastHighlightNext = true;
		}

		private bool TryOpenCompletion()
		{
			int caret = _editor.SelectionStart;
			if (caret == 0)
				return false;

			string text = _editor.Text;
			int end = caret;
			int start = end;
			while (start > 0)
			{
				char c = text[start - 1];
				if (char.IsLetterOrDigit(c) || c == '_')
				{ start--; continue; }
				break;
			}

			if (start == end)
				return false;
			string ident = text.Substring(start, end - start);

			List<string> members = ApiIndex.GetMembers(ident);

			if (members == null || members.Count == 0)
				return false;

			PositionCompletionPanel(caret);
			_completion.Open(members);
			return true;
		}

		private void PositionCompletionPanel(int caret)
		{
			Point caretPos = _editor.GetPositionFromCharIndex(caret);
			int panelX = _editor.Left + caretPos.X;
			int panelY = _editor.Top + caretPos.Y + _editor.Font.Height;

			if (panelY + _completion.Height > _editor.Bottom)
				panelY = _editor.Top + caretPos.Y - _completion.Height;
			if (panelX + _completion.Width > _editor.Right)
				panelX = _editor.Right - _completion.Width;
			if (panelX < _editor.Left)
				panelX = _editor.Left;

			_completion.Location = new Point(panelX, panelY);
		}
	}
}
