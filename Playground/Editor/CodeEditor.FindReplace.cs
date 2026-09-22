using System.Drawing;
using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		public void ShowFindReplace(bool replaceMode)
		{
			_findPanel.ShowPanel(_editor, replaceMode,
				_findPanel.FindText,
				_findPanel.ReplaceText);
			RepositionFindPanel();
			_findPanel.BringToFront();
		}

		/// <summary>
		/// Открыть диалог «перейти к строке». Номер 1-based.
		/// </summary>
		public void ShowGoToLine()
		{
			int totalLines = _editor.GetLineFromCharIndex(_editor.TextLength) + 1;
			int currentLine = _editor.GetLineFromCharIndex(_editor.SelectionStart) + 1;

			using (var dialog = new GoToLineDialog(totalLines, currentLine))
			{
				// Parent — владелец формы, чтобы диалог модальный относительно приложения.
				Form owner = FindForm();
				DialogResult result = (owner != null)
					? dialog.ShowDialog(owner)
					: dialog.ShowDialog();

				if (result != DialogResult.OK)
					return;

				int target = dialog.Result;
				// На случай, если пользователь ввёл число больше, чем строк в файле.
				if (target < 1)
					target = 1;
				if (target > totalLines)
					target = totalLines;

				GoToLine(target - 1);   // 0-based
			}
		}

		/// <summary>
		/// Поставить каретку на указанную (0-based) строку, выделить её целиком
		/// и проскроллить к ней.
		/// </summary>
		private void GoToLine(int line)
		{
			if (line < 0)
				line = 0;
			int totalLines = _editor.GetLineFromCharIndex(_editor.TextLength) + 1;
			if (line >= totalLines)
				line = totalLines - 1;

			int start = _editor.GetFirstCharIndexFromLine(line);
			if (start < 0)
				return;

			int nextStart = _editor.GetFirstCharIndexFromLine(line + 1);
			int end = (nextStart >= 0) ? nextStart : _editor.TextLength;

			_editor.SelectionStart = start;
			_editor.SelectionLength = end - start;
			_editor.ScrollToCaret();
			_editor.Focus();

			_undo.SyncCaret();
			_undo.BreakTypingGroup();
		}

		private void RepositionFindPanel()
		{
			if (_findPanel == null || _editor == null)
				return;
			int x = _editor.Right - _findPanel.Width - 20;
			if (x < _gutter.Right + 4)
				x = _gutter.Right + 4;
			_findPanel.Location = new Point(x, _editor.Top + 6);
		}
	}
}
