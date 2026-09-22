using System;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		private void HandleTabKey(bool shift)
		{
			_undo.BeginChange();

			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			// ---- нет выделения: простая вставка/удаление в каретке ----
			if (selLength == 0)
			{
				if (!shift)
					_editor.ReplaceSelectionWithUndo(_indent);
				else
					UnindentCurrentLine();
				return;
			}

			// ---- выделение из нескольких строк ----
			int firstLine = _editor.GetLineFromCharIndex(selStart);
			int lastLine = _editor.GetLineFromCharIndex(selStart + selLength);

			// Если выделение заканчивается ровно на начале последней строки,
			// не трогаем её (стандартное поведение редакторов).
			if (lastLine > firstLine
				&& _editor.GetFirstCharIndexFromLine(lastLine) == selStart + selLength)
			{
				lastLine--;
			}

			_editor.SuspendLayout();
			try
			{
				if (!shift)
				{
					// Bottom-up, чтобы индексы строк выше не сдвигались.
					for (int i = lastLine; i >= firstLine; i--)
					{
						int lineStart = _editor.GetFirstCharIndexFromLine(i);
						if (lineStart < 0)
							continue;
						_editor.Select(lineStart, 0);
						_editor.ReplaceSelectionWithUndo(_indent);
					}
				}
				else
				{
					// lines[] кэшируем ДО модификаций: строки ниже firstLine
					// мы не трогаем, а строки выше firstLine не портятся,
					// потому что удаляем мы по одной сверху-вниз? Нет — снизу-вверх,
					// так что кэш валиден для ещё не обработанных строк.
					string[] lines = _editor.Lines;
					for (int i = lastLine; i >= firstLine; i--)
					{
						if (i < 0 || i >= lines.Length)
							continue;
						int lineStart = _editor.GetFirstCharIndexFromLine(i);
						if (lineStart < 0)
							continue;

						int remove = CountLeadingIndent(lines[i], _indent);
						if (remove > 0)
						{
							_editor.Select(lineStart, remove);
							_editor.ReplaceSelectionWithUndo(string.Empty);
						}
					}
				}

				// Перевыделяем те же строки заново.
				int newFirstStart = _editor.GetFirstCharIndexFromLine(firstLine);
				int newLastStart = _editor.GetFirstCharIndexFromLine(lastLine);
				if (newFirstStart >= 0 && newLastStart >= 0)
				{
					string[] lines2 = _editor.Lines;
					int lastLen = (lastLine < lines2.Length) ? lines2[lastLine].Length : 0;
					int end = newLastStart + lastLen;
					_editor.SelectionStart = newFirstStart;
					_editor.SelectionLength = end - newFirstStart;
				}
			}
			finally
			{
				_editor.ResumeLayout();
			}
		}

		private void UnindentCurrentLine()
		{
			_undo.BeginChange();

			int caret = _editor.SelectionStart;
			int line = _editor.GetLineFromCharIndex(caret);
			int lineStart = _editor.GetFirstCharIndexFromLine(line);
			if (lineStart < 0)
				return;

			string[] lines = _editor.Lines;
			if (line >= lines.Length)
				return;

			int remove = CountLeadingIndent(lines[line], _indent);
			if (remove == 0)
				return;

			_editor.Select(lineStart, remove);
			_editor.ReplaceSelectionWithUndo(string.Empty);

			int newCaret = Math.Max(lineStart, caret - remove);
			_editor.Select(newCaret, 0);
		}

		private void InsertNewLineWithIndent()
		{
			_undo.BeginChange();

			int caret = _editor.SelectionStart;
			int line = _editor.GetLineFromCharIndex(caret);
			int lineStart = _editor.GetFirstCharIndexFromLine(line);
			if (lineStart < 0)
			{
				_editor.ReplaceSelectionWithUndo("\r\n");
				return;
			}

			string[] lines = _editor.Lines;
			string lineText = (line < lines.Length) ? lines[line] : "";

			int indentLen = 0;
			while (indentLen < lineText.Length
				&& (lineText[indentLen] == ' ' || lineText[indentLen] == '\t'))
			{
				indentLen++;
			}

			string indent = lineText.Substring(0, indentLen);

			// Опционально: +1 уровень, если строка заканчивается на "{".
			// Для fluent-цепочек не сработает (строка заканчивается на ")"),
			// но для обычного C# с блоками — полезно.
			bool braceOpen = lineText.TrimEnd().EndsWith("{");

			_editor.ReplaceSelectionWithUndo("\r\n" + indent + (braceOpen ? _indent : ""));
		}

		/// <summary>
		/// Удаляет все строки, попадающие в текущее выделение (или строку
		/// с кареткой, если выделения нет). Удаление включает переводы строк,
		/// чтобы не оставалось пустых линий. Каретка после операции ставится
		/// в начало той строки, что оказалась на месте первой удалённой.
		/// </summary>
		private void DeleteSelectedLines()
		{
			_undo.BeginChange();

			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			int firstLine = _editor.GetLineFromCharIndex(selStart);
			int lastLine = _editor.GetLineFromCharIndex(selStart + selLength);

			// Если выделение заканчивается ровно на начале последней строки,
			// не трогаем её — стандартное поведение редакторов.
			if (lastLine > firstLine
				&& _editor.GetFirstCharIndexFromLine(lastLine) == selStart + selLength)
			{
				lastLine--;
			}

			int totalLines = _editor.Lines.Length;

			int deleteStart;
			int deleteEnd;

			if (lastLine < totalLines - 1)
			{
				// Обычный случай: удаляем строки [firstLine..lastLine]
				// вместе с переводом строки ПОСЛЕ них.
				deleteStart = _editor.GetFirstCharIndexFromLine(firstLine);
				deleteEnd = _editor.GetFirstCharIndexFromLine(lastLine + 1);
			}
			else
			{
				// Удаляем до последней строки включительно.
				// Перевод строки есть только ПЕРЕД первой удаляемой — его и захватим,
				// иначе останется "хвост" из лишнего перевода.
				if (firstLine > 0)
				{
					int lineStart = _editor.GetFirstCharIndexFromLine(firstLine);
					int pos = lineStart;
					// Откатываемся назад, захватывая перевод строки перед блоком.
					if (pos >= 2 && _editor.Text[pos - 2] == '\r' && _editor.Text[pos - 1] == '\n')
						pos -= 2;
					else if (pos >= 1 && _editor.Text[pos - 1] == '\n')
						pos -= 1;
					deleteStart = pos;
					deleteEnd = _editor.TextLength;
				}
				else
				{
					// Единственная строка в документе — просто очищаем всё.
					deleteStart = 0;
					deleteEnd = _editor.TextLength;
				}
			}

			if (deleteEnd <= deleteStart)
				return;

			_editor.Select(deleteStart, deleteEnd - deleteStart);
			_editor.ReplaceSelectionWithUndo(string.Empty);

			// Каретка — в начало области, где был удалённый блок.
			_editor.Select(deleteStart, 0);
			_editor.ScrollToCaret();
		}

		/// <summary>
		/// Дублирует строки, попадающие в текущее выделение (или строку
		/// с кареткой, если выделения нет), вставляя копию сразу под
		/// оригиналом. Каретка после операции — на той же визуальной позиции,
		/// но уже в скопированной строке (как в VS Code).
		/// </summary>
		private void DuplicateSelectedLines()
		{
			_undo.BeginChange();

			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			int firstLine = _editor.GetLineFromCharIndex(selStart);
			int lastLine = _editor.GetLineFromCharIndex(selStart + selLength);

			// Если выделение заканчивается ровно на начале последней строки,
			// не дублируем её.
			if (lastLine > firstLine
				&& _editor.GetFirstCharIndexFromLine(lastLine) == selStart + selLength)
			{
				lastLine--;
			}

			int blockStart = _editor.GetFirstCharIndexFromLine(firstLine);
			int blockEnd;

			int totalLines = _editor.GetLineFromCharIndex(_editor.TextLength) + 1;
			if (lastLine < totalLines - 1)
			{
				// Есть строка после блока — граница по её началу.
				blockEnd = _editor.GetFirstCharIndexFromLine(lastLine + 1);
			}
			else
			{
				// Блок — последние строки документа, до конца текста.
				blockEnd = _editor.TextLength;
			}

			if (blockEnd < blockStart)
				return;

			// Сколько символов от начала строки firstLine до каретки —
			// чтобы потом восстановить каретку на ту же визуальную позицию.
			int caretOffsetInBlock = selStart - blockStart;
			if (caretOffsetInBlock < 0)
				caretOffsetInBlock = 0;

			string block = _editor.Text.Substring(blockStart, blockEnd - blockStart);

			bool needsLeadingNewline = blockEnd == _editor.TextLength
				&& !block.EndsWith("\n") && !block.EndsWith("\r");
			string newline = DetectNewline(block);
			string toInsert = (needsLeadingNewline ? newline : "") + block;

			// Вставляем сразу после блока.
			_editor.Select(blockEnd, 0);
			_editor.ReplaceSelectionWithUndo(toInsert);

			// Новая позиция каретки — в копии, на той же визуальной позиции.
			int copyStart = blockEnd + (needsLeadingNewline ? newline.Length : 0);
			int newCaret = copyStart + caretOffsetInBlock;
			int newLength = selLength; // выделение сохраняем как было

			if (newCaret > _editor.TextLength)
				newCaret = _editor.TextLength;

			_editor.Select(newCaret, newLength);
			_editor.ScrollToCaret();
		}

		private void MoveSelectedLines(int direction)
		{
			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			int firstLine = _editor.GetLineFromCharIndex(selStart);
			int lastLine = _editor.GetLineFromCharIndex(selStart + selLength);

			// Если выделение заканчивается ровно на начале последней строки,
			// не двигаем эту строку.
			if (lastLine > firstLine
				&& _editor.GetFirstCharIndexFromLine(lastLine) == selStart + selLength)
			{
				lastLine--;
			}

			int totalLines = _editor.GetLineFromCharIndex(_editor.TextLength) + 1;

			if (direction < 0 && firstLine == 0)
				return;
			if (direction > 0 && lastLine == totalLines - 1)
				return;

			string text = _editor.Text;
			int textLen = text.Length;

			int rangeStart, rangeEnd;
			string replacement;
			int caretDelta;

			if (direction < 0)
			{
				// ---------- движение вверх ----------
				int prevLine = firstLine - 1;

				int prevStart = _editor.GetFirstCharIndexFromLine(prevLine);
				int blockStart = _editor.GetFirstCharIndexFromLine(firstLine);
				int blockEnd = (lastLine + 1 < totalLines)
					? _editor.GetFirstCharIndexFromLine(lastLine + 1)
					: textLen;

				string prevText = text.Substring(prevStart, blockStart - prevStart);
				string blockText = text.Substring(blockStart, blockEnd - blockStart);

				// Критерий: есть ли в исходном тексте что-то ПОСЛЕ блока.
				// Если да — blockText заканчивается \r\n (начало следующей строки уже
				// включено в диапазон). Если нет — blockText это последняя строка
				// без \r\n, и нам нужно его самим поставить при обмене.
				bool blockHasTrailingNewline = blockEnd < textLen;

				if (blockHasTrailingNewline)
				{
					// Оба куска заканчиваются \r\n — просто меняем местами.
					replacement = blockText + prevText;
				}
				else
				{
					string newline = DetectNewline(prevText + blockText);
					string prevNoNewline = prevText.EndsWith("\r\n")
						? prevText.Substring(0, prevText.Length - 2)
						: prevText.EndsWith("\n")
							? prevText.Substring(0, prevText.Length - 1)
							: prevText;
					replacement = blockText + newline + prevNoNewline;
				}

				rangeStart = prevStart;
				rangeEnd = blockEnd;

				// Каретка была в блоке; блок теперь начинается на prevStart.
				caretDelta = -(blockStart - prevStart);
			}
			else
			{
				// ---------- движение вниз ----------
				int nextLine = lastLine + 1;

				int blockStart = _editor.GetFirstCharIndexFromLine(firstLine);
				int blockEnd = _editor.GetFirstCharIndexFromLine(nextLine);
				int nextEnd = (nextLine + 1 < totalLines)
					? _editor.GetFirstCharIndexFromLine(nextLine + 1)
					: textLen;

				string blockText = text.Substring(blockStart, blockEnd - blockStart);
				string nextText = text.Substring(blockEnd, nextEnd - blockEnd);

				// Аналогичный критерий: есть ли что-то после nextLine.
				bool nextHasTrailingNewline = nextEnd < textLen;

				if (nextHasTrailingNewline)
				{
					// Оба куска заканчиваются \r\n — просто меняем местами.
					replacement = nextText + blockText;
				}
				else
				{
					string newline = DetectNewline(blockText + nextText);
					string blockNoNewline = blockText.EndsWith("\r\n")
						? blockText.Substring(0, blockText.Length - 2)
						: blockText.EndsWith("\n")
							? blockText.Substring(0, blockText.Length - 1)
							: blockText;
					replacement = nextText + newline + blockNoNewline;
				}

				rangeStart = blockStart;
				rangeEnd = nextEnd;

				// Каретка была в блоке; блок теперь начинается на blockEnd.
				caretDelta = nextText.Length;
			}

			_undo.BeginChange();

			_editor.Select(rangeStart, rangeEnd - rangeStart);
			_editor.ReplaceSelectionWithUndo(replacement);

			int newSelStart = selStart + caretDelta;
			if (newSelStart < 0)
				newSelStart = 0;
			if (newSelStart > _editor.TextLength)
				newSelStart = _editor.TextLength;

			int newSelLength = selLength;
			if (newSelStart + newSelLength > _editor.TextLength)
				newSelLength = _editor.TextLength - newSelStart;

			_editor.SelectionStart = newSelStart;
			_editor.SelectionLength = newSelLength;
			_editor.ScrollToCaret();
		}

		/// <summary>
		/// Определяет стиль перевода строки в тексте.
		/// Возвращает "\r\n" для Windows, "\n" для Unix, "\r" для старого Mac.
		/// </summary>
		private static string DetectNewline(string text)
		{
			if (text.Contains("\r\n"))
				return "\r\n";
			if (text.Contains("\n"))
				return "\n";
			if (text.Contains("\r"))
				return "\r";
			return Environment.NewLine;
		}

		/// <summary>
		/// Умный Backspace. Если каретка в ведущих пробелах строки — стирает
		/// до ближайшей 4-пробельной границы, а не один символ.
		/// Возвращает true, если мы обработали Backspace сами.
		/// </summary>
		private bool HandleBackspaceKey()
		{
			// С непустым выделением пусть работает стандартный Backspace.
			if (_editor.SelectionLength > 0)
				return false;

			int caret = _editor.SelectionStart;
			if (caret == 0)
				return false;

			// --- Удаление пустой пары ---
			if (caret < _editor.TextLength)
			{
				char left = _editor.Text[caret - 1];
				char right = _editor.Text[caret];

				bool isEmptyPair =
					(left == '(' && right == ')') ||
					(left == '[' && right == ']') ||
					(left == '{' && right == '}') ||
					(left == '"' && right == '"') ||
					(left == '\'' && right == '\'');

				if (isEmptyPair)
				{
					_undo.BeginChange();
					_fastHighlightNext = true;

					_editor.Select(caret - 1, 2);
					_editor.ReplaceSelectionWithUndo("");
					return true;
				}
			}

			int line = _editor.GetLineFromCharIndex(caret);
			int lineStart = _editor.GetFirstCharIndexFromLine(line);
			if (lineStart < 0)
				return false;

			// Сколько символов от начала строки до каретки.
			int col = caret - lineStart;
			if (col <= 0)
				return false;

			// Проверяем, что весь промежуток [lineStart, caret) — пробелы или табы.
			string[] lines = _editor.Lines;
			if (line >= lines.Length)
				return false;

			string lineText = lines[line];
			if (col > lineText.Length)
				return false;

			for (int i = 0; i < col; i++)
			{
				char ch = lineText[i];
				if (ch != ' ' && ch != '\t')
					return false;   // не весь префикс — пробелы, значит обычный Backspace
			}

			// До этого места всё — leading whitespace. Считаем, сколько удалить.
			int remove;
			int mod = col % _indent.Length;
			if (mod == 0)
				remove = _indent.Length;
			else
				remove = mod;

			if (remove > col)
				remove = col;

			if (remove <= 0)
				return false;

			_undo.BeginChange();
			_editor.Select(caret - remove, remove);
			_editor.ReplaceSelectionWithUndo(string.Empty);
			return true;
		}

		private static int CountLeadingIndent(string line, string indent)
		{
			int count = 0;
			while (count < indent.Length && count < line.Length)
			{
				if (line[count] == ' ')
					count++;
				else if (line[count] == '\t')
				{ count++; break; }
				else
					break;
			}
			return count;
		}
	}
}
