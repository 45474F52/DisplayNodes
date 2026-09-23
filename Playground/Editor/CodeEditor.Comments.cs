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

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		/// <summary>
		/// Ctrl+/ — если хотя бы одна строка выделения не закомментирована,
		/// закомментировать все; иначе раскомментировать все.
		/// </summary>
		private void ToggleCommentSelectedLines()
		{
			int firstLine, lastLine;
			GetSelectedLineRange(out firstLine, out lastLine);
			if (firstLine < 0)
				return;

			// Проверяем: все ли строки уже закомментированы?
			bool allCommented = true;
			for (int i = firstLine; i <= lastLine; i++)
			{
				if (!IsLineCommented(i))
				{
					allCommented = false;
					break;
				}
			}

			if (allCommented)
				DoUncommentLines(firstLine, lastLine);
			else
				DoCommentLines(firstLine, lastLine);
		}

		/// <summary>
		/// Ctrl+Shift+/ — принудительно снять комментарий со всех строк выделения.
		/// </summary>
		private void UncommentSelectedLines()
		{
			int firstLine, lastLine;
			GetSelectedLineRange(out firstLine, out lastLine);
			if (firstLine < 0)
				return;

			DoUncommentLines(firstLine, lastLine);
		}

		/// <summary>
		/// Возвращает диапазон строк, попадающих в выделение.
		/// Если выделение оканчивается ровно на начале строки, эта строка не включается.
		/// </summary>
		private void GetSelectedLineRange(out int firstLine, out int lastLine)
		{
			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			firstLine = _editor.GetLineFromCharIndex(selStart);
			lastLine = _editor.GetLineFromCharIndex(selStart + selLength);

			if (lastLine > firstLine
				&& _editor.GetFirstCharIndexFromLine(lastLine) == selStart + selLength)
			{
				lastLine--;
			}
		}

		/// <summary>
		/// Строка считается закомментированной, если после ведущих пробелов
		/// идёт "//". Пустые строки считаются "закомментированными" — они не мешают
		/// групповому раскомментированию.
		/// </summary>
		private bool IsLineCommented(int line)
		{
			string[] lines = _editor.Lines;
			if (line < 0 || line >= lines.Length)
				return false;

			string t = lines[line];
			int i = 0;
			while (i < t.Length && (t[i] == ' ' || t[i] == '\t'))
				i++;

			if (i >= t.Length)
				return true;   // пустая строка
			return i + 1 < t.Length && t[i] == '/' && t[i + 1] == '/';
		}

		private void DoCommentLines(int firstLine, int lastLine)
		{
			_undo.BeginChange();

			_editor.SuspendLayout();
			try
			{
				// Bottom-up, чтобы индексы строк выше не сдвигались.
				for (int i = lastLine; i >= firstLine; i--)
				{
					int lineStart = _editor.GetFirstCharIndexFromLine(i);
					if (lineStart < 0)
						continue;

					// Пропускаем пустые строки — не хотим комментировать пробелы.
					string[] lines = _editor.Lines;
					if (i < lines.Length && lines[i].Trim().Length == 0)
						continue;

					// Находим длину ведущих пробелов.
					string t = lines[i];
					int indentLen = 0;
					while (indentLen < t.Length && (t[indentLen] == ' ' || t[indentLen] == '\t'))
						indentLen++;

					// Вставляем "//" после отступа.
					_editor.Select(lineStart + indentLen, 0);
					_editor.ReplaceSelectionWithUndo("// ");
				}

				// Восстанавливаем выделение на те же строки (с учётом сдвигов).
				ReselectLines(firstLine, lastLine);
			}
			finally
			{
				_editor.ResumeLayout();
			}
		}

		private void DoUncommentLines(int firstLine, int lastLine)
		{
			_undo.BeginChange();

			_editor.SuspendLayout();
			try
			{
				for (int i = lastLine; i >= firstLine; i--)
				{
					int lineStart = _editor.GetFirstCharIndexFromLine(i);
					if (lineStart < 0)
						continue;

					string[] lines = _editor.Lines;
					if (i >= lines.Length)
						continue;

					string t = lines[i];
					int indentLen = 0;
					while (indentLen < t.Length && (t[indentLen] == ' ' || t[indentLen] == '\t'))
						indentLen++;

					// Пустая строка — пропускаем.
					if (indentLen >= t.Length)
						continue;

					// Должно быть "//".
					if (indentLen + 1 >= t.Length)
						continue;
					if (t[indentLen] != '/' || t[indentLen + 1] != '/')
						continue;

					// Определяем длину того, что срезать: "// " (3 символа) или "//" (2).
					int remove = 2;
					if (indentLen + 2 < t.Length && t[indentLen + 2] == ' ')
						remove = 3;

					_editor.Select(lineStart + indentLen, remove);
					_editor.ReplaceSelectionWithUndo(string.Empty);
				}

				ReselectLines(firstLine, lastLine);
			}
			finally
			{
				_editor.ResumeLayout();
			}
		}

		/// <summary>
		/// Перевыделяет строки [firstLine..lastLine] после правок.
		/// Нужно, потому что вставка/удаление "// " сместило индексы.
		/// </summary>
		private void ReselectLines(int firstLine, int lastLine)
		{
			int newFirstStart = _editor.GetFirstCharIndexFromLine(firstLine);
			int newLastStart = _editor.GetFirstCharIndexFromLine(lastLine);

			if (newFirstStart < 0)
				return;

			int end;
			if (lastLine + 1 <= _editor.GetLineFromCharIndex(_editor.TextLength))
			{
				int nextStart = _editor.GetFirstCharIndexFromLine(lastLine + 1);
				end = (nextStart >= 0) ? nextStart : _editor.TextLength;
			}
			else
			{
				end = _editor.TextLength;
			}

			if (end < newFirstStart)
				end = newFirstStart;

			_editor.SelectionStart = newFirstStart;
			_editor.SelectionLength = end - newFirstStart;
		}
	}
}
