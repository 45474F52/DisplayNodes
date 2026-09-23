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
using System.Drawing;
using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	public partial class CodeEditor
	{
		private int GetLineCount()
		{
			return _editor.GetLineFromCharIndex(_editor.TextLength) + 1;
		}

		private void Editor_TextChanged(object sender, EventArgs e)
		{
			UpdateGutterWidth();
			_gutter.Invalidate();
		}

		private void UpdateGutterWidth()
		{
			int lineCount = GetLineCount();
			int digits = Math.Max(2, lineCount.ToString().Length);
			Size sz = TextRenderer.MeasureText(new string('9', digits), _editor.Font);
			int newWidth = sz.Width + 16;
			if (_gutter.Width != newWidth)
				_gutter.Width = newWidth;
		}

		private void Gutter_Paint(object sender, PaintEventArgs e)
		{
			if (!_editor.ShowLineNumbers)
				return;

			int gutterHeight = _gutter.ClientSize.Height;
			int gutterWidth = _gutter.ClientSize.Width;

			int firstVisibleLine = _editor.GetLineFromCharIndex(
				_editor.GetCharIndexFromPosition(new Point(1, 1)));

			int firstCharIdx = _editor.GetFirstCharIndexFromLine(firstVisibleLine);
			if (firstCharIdx < 0)
				return;

			int y = _editor.GetPositionFromCharIndex(firstCharIdx).Y;
			int maxLine = GetLineCount();
			int currentLine = _editor.GetLineFromCharIndex(_editor.SelectionStart);

			// Измеряем ширину максимального номера один раз (например, "9999").
			// Для моноширинного шрифта ширина числа пропорциональна количеству цифр.
			string maxNum = maxLine.ToString();
			int maxNumWidth = TextRenderer.MeasureText(maxNum, _editor.Font).Width;

			for (int i = firstVisibleLine; i < maxLine; i++)
			{
				if (y > gutterHeight)
					break;
				if (y + _lineHeight > 0)
				{
					string num = (i + 1).ToString();
					// Вычисляем ширину текущего номера пропорционально.
					// Для моноширинного шрифта: ширина = (длина числа / длина макс. числа) * макс. ширина.
					int numWidth = (num.Length * maxNumWidth) / maxNum.Length;
					Color numColor = (i == currentLine) ? Color.Gainsboro : Color.Gray;
					TextRenderer.DrawText(
						e.Graphics, num, _editor.Font,
						new Point(gutterWidth - numWidth - 8, y),
						numColor);
				}
				y += _lineHeight;
			}
		}

		private void Gutter_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			int charIdx = _editor.GetCharIndexFromPosition(new Point(1, e.Y));
			int line = _editor.GetLineFromCharIndex(charIdx);

			int start = _editor.GetFirstCharIndexFromLine(line);
			if (start < 0)
				return;

			int nextStart = _editor.GetFirstCharIndexFromLine(line + 1);
			int end = (nextStart >= 0) ? nextStart : _editor.TextLength;

			_editor.SelectionStart = start;
			_editor.SelectionLength = end - start;
			_editor.Focus();
			_undo.SyncCaret();
		}
	}
}
