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
using System.Runtime.InteropServices;
using System.Windows.Forms;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Editor
{
	public class ScrollRichTextBox : RichTextBox
	{
		public event EventHandler Scrolled;

		public bool SuppressTextChanged { get; set; }

		private const int WM_VSCROLL = 0x0115;
		private const int WM_HSCROLL = 0x0114;
		private const int WM_MOUSEWHEEL = 0x020A;
		private const int WM_PAINT = 0x000F;
		private const int WM_USER = 0x0400;

		// RichEdit language options
		private const int EM_GETLANGOPTIONS = 0x0400 + 121; // WM_USER + 121
		private const int EM_SETLANGOPTIONS = 0x0400 + 120; // WM_USER + 120
		private const int EM_SETUNDOLIMIT = 0x0400 + 82;
		private const int IMF_SMOOTHSCROLL = 0x0100;
		private const int EM_REPLACESEL = 0x00C2;
		private const int EM_STOPGROUPTYPING = 0x0438;
		private const int EM_GETRECT = 0x00B2;
		private const int EM_GETSCROLLPOS = WM_USER + 221;

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT { public int Left, Top, Right, Bottom; }

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private readonly List<int> _errorLines = new List<int>();
		
		private string _indent = "    ";

		private bool _drawIndentGuides = true;
		private bool _drawErrorUnderlines = true;
		private bool _drawCurrentLineHighlight = true;
        private bool _showLineNumbers = true;

        private Color _currentLineHighlightColor = Color.FromArgb(0x18, 0x36, 0x3C, 0x4A);

		private int _cachedCharWidth = -1;

		public bool DrawIndentGuides
		{
			get { return _drawIndentGuides; }
			set { _drawIndentGuides = value; Invalidate(); }
		}

		public string IndentString
		{
			get { return _indent; }
			set { _indent = value ?? "    "; Invalidate(); }
		}

		public bool DrawErrorUnderlines
		{
			get { return _drawErrorUnderlines; }
			set { _drawErrorUnderlines = value; Invalidate(); }
		}

		public bool DrawCurrentLineHighlight
		{
			get { return _drawCurrentLineHighlight; }
			set { _drawCurrentLineHighlight = value; Invalidate(); }
		}

        public bool ShowLineNumbers
        {
            get { return _showLineNumbers; }
            set { _showLineNumbers = value; Invalidate(); }
        }

        public Color CurrentLineHighlightColor
		{
			get { return _currentLineHighlightColor; }
			set { _currentLineHighlightColor = value; Invalidate(); }
		}

		/// <summary>
		/// Установить список ошибочных строк (1-based номера строк).
		/// Передайте null или пустой список, чтобы очистить подсветку.
		/// </summary>
		public void SetErrorLines(IEnumerable<int> lines)
		{
			_errorLines.Clear();
			if (lines != null)
			{
				foreach (int line in lines)
					_errorLines.Add(line);
			}
			Invalidate();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			try
			{
				IntPtr opts = SendMessage(Handle, EM_GETLANGOPTIONS, IntPtr.Zero, IntPtr.Zero);
				long newOpts = opts.ToInt64() & ~(long)IMF_SMOOTHSCROLL;
				SendMessage(Handle, EM_SETLANGOPTIONS, IntPtr.Zero, (IntPtr)newOpts);
				SendMessage(Handle, EM_SETUNDOLIMIT, IntPtr.Zero, (IntPtr)0);
			}
			catch
			{
			}
		}

		protected override void WndProc(ref Message m)
		{
			bool notify = m.Msg == WM_VSCROLL
					   || m.Msg == WM_HSCROLL
					   || m.Msg == WM_MOUSEWHEEL;

			if (m.Msg == WM_PAINT)
			{
				base.WndProc(ref m);

				if (_drawCurrentLineHighlight)
					DrawCurrentLineHighlightOverlay();

				if (_drawIndentGuides)
					DrawIndentGuidesOverlay();

				if (_drawErrorUnderlines && _errorLines.Count > 0)
					DrawErrorUnderlinesOverlay();

				return;
			}

			base.WndProc(ref m);

			if (notify)
			{
				Scrolled?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			base.OnSelectionChanged(e);

			if (_drawCurrentLineHighlight)
				Invalidate();
		}
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			_cachedCharWidth = -1;
		}

		/// <summary>
		/// Рисует полупрозрачную полосу под строкой, где стоит каретка.
		/// Полоса идёт поверх текста — с альфой, поэтому текст остаётся читаемым.
		/// </summary>
		private void DrawCurrentLineHighlightOverlay()
		{
			int lineIdx = GetLineFromCharIndex(SelectionStart);
			if (lineIdx < 0)
				return;

			int firstChar = GetFirstCharIndexFromLine(lineIdx);
			if (firstChar < 0)
				return;

			Point origin = GetPositionFromCharIndex(firstChar);
			int top = origin.Y;
			int lineHeight = Font.Height;

			if (top > ClientSize.Height)
				return;
			if (top + lineHeight < 0)
				return;

			using (var g = Graphics.FromHwnd(Handle))
			using (var brush = new SolidBrush(_currentLineHighlightColor))
			{
				g.FillRectangle(brush, 0, top, ClientSize.Width, lineHeight);
			}
		}

		/// <summary>
		/// Рисует красную волнистую линию под строками из _errorLines.
		/// Работает поверх уже нарисованного текста, поэтому не портит
		/// ни содержимое, ни подсветку синтаксиса, ни undo.
		/// </summary>
		private void DrawErrorUnderlinesOverlay()
		{
			if (_errorLines.Count == 0)
				return;

			int clientWidth = ClientSize.Width;
			int clientHeight = ClientSize.Height;
			if (clientWidth <= 0 || clientHeight <= 0)
				return;

			int lineHeight = Font.Height;

			// Параметры зигзага. STEP — расстояние между пиками,
			// AMPLITUDE — высота волны.
			const int STEP = 4;
			const int AMPLITUDE = 2;

			// Y-координата линии относительно низа строки — чуть выше нижней границы.
			const int BOTTOM_PAD = 0;

			Color errorColor = Color.FromArgb(240, 100, 100);

			int totalLines = GetLineFromCharIndex(TextLength) + 1;
			string[] lines = Lines;

			using (var g = Graphics.FromHwnd(Handle))
			using (var pen = new Pen(errorColor, 1f))
			{
				// Кэшируем ширину символа один раз на весь метод.
				string measureStr = "MMMMMMMMMM"; // 10 символов
				int measureWidth = TextRenderer.MeasureText(
					measureStr, Font,
					new Size(int.MaxValue, int.MaxValue),
					TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
				int charWidth = measureWidth / measureStr.Length;

				for (int k = 0; k < _errorLines.Count; k++)
				{
					int lineIdx = _errorLines[k] - 1;   // 1-based → 0-based
					if (lineIdx < 0 || lineIdx >= totalLines)
						continue;
					if (lineIdx >= lines.Length)
						continue;

					string lineText = lines[lineIdx];

					// Пустые строки не подчёркиваем — нечего подчёркивать.
					string trimmed = lineText.TrimEnd();
					if (trimmed.Length == 0)
						continue;

					int firstChar = GetFirstCharIndexFromLine(lineIdx);
					if (firstChar < 0)
						continue;

					Point origin = GetPositionFromCharIndex(firstChar);
					int top = origin.Y;

					// Y нижней границы волны.
					int baseY = top + lineHeight - BOTTOM_PAD;

					// Фильтр по клиентской области.
					if (baseY + AMPLITUDE < 0)
						continue;
					if (top > clientHeight)
						continue;

					int startX = origin.X;

					int endX = startX + trimmed.Length * charWidth;

					if (endX <= 0)
						continue;
					if (startX >= clientWidth)
						continue;
					if (startX < 0)
						startX = 0;
					if (endX > clientWidth)
						endX = clientWidth;

					int width = endX - startX;
					if (width < 2)
						continue;

					// Рисуем зигзаг посегментно через DrawLine.
					// Это полностью исключает аллокацию new Point[] на каждом кадре,
					// снижая нагрузку на GC до нуля при отрисовке подчёркиваний.
					for (int p = 0; p < width / STEP + 1; p++)
					{
						int x1 = startX + p * STEP;
						int x2 = startX + (p + 1) * STEP;

						if (x2 > endX)
							x2 = endX;
						if (x1 >= endX)
							break;

						int y1 = baseY - ((p % 2 == 0) ? 0 : AMPLITUDE);
						int y2 = baseY - (((p + 1) % 2 == 0) ? 0 : AMPLITUDE);

						g.DrawLine(pen, x1, y1, x2, y2);
					}
				}
			}
		}

		private void DrawIndentGuidesOverlay()
		{
			if (Lines.Length == 0)
				return;

			// Точная ширина моноширинного символа через GDI (TextRenderer).
			// RichTextBox рендерит текст через GDI, поэтому и мерить надо GDI,
			// а не GDI+ (MeasureString даёт padding и врёт на 1–2 px).
			if (_cachedCharWidth <= 0)
			{
				_cachedCharWidth = TextRenderer.MeasureText(
					"M", Font,
					new Size(int.MaxValue, int.MaxValue),
					TextFormatFlags.NoPadding).Width;
			}
			int charWidth = _cachedCharWidth;

			if (charWidth <= 0)
				return;

			int indentWidth = _indent.Length * charWidth;
			int lineHeight = Font.Height;

			using (var g = Graphics.FromHwnd(Handle))
			using (var pen = new Pen(Color.FromArgb(60, 60, 60), 1))
			{
				int firstLine = GetLineFromCharIndex(GetCharIndexFromPosition(new Point(0, 0)));
				int maxLine = Lines.Length;

				for (int i = firstLine; i < maxLine; i++)
				{
					int lineStart = GetFirstCharIndexFromLine(i);
					if (lineStart < 0)
						continue;

					Point linePos = GetPositionFromCharIndex(lineStart);
					int y = linePos.Y;

					if (y > ClientSize.Height)
						break;

					if (y + lineHeight <= 0)
						continue;

					string lineText = Lines[i];

					// Пустые строки — гайды не рисуем.
					if (lineText.IsNullOrWhiteSpace())
						continue;

					// Считаем ведущие пробелы.
					int indent = 0;
					while (indent < lineText.Length && lineText[indent] == ' ')
						indent++;

					int level = indent / _indent.Length;
					if (level == 0)
						continue;

					// linePos.X — X первого символа строки, то есть колонка 0
					// в клиентских координатах. Уже включает горизонтальный скролл.
					int baseX = linePos.X;

					// Гайды на границах уровней: 0, 4, 8, ..., (level-1)*4.
					for (int l = 0; l < level; l++)
					{
						int x = baseX + l * indentWidth;

						if (x < 0 || x >= ClientSize.Width)
							continue;

						g.DrawLine(pen, x, y, x, y + lineHeight - 1);
					}
				}
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (SuppressTextChanged)
				return;
			base.OnTextChanged(e);
		}

		/// <summary>Принудительно уведомить подписчиков об изменении текста.</summary>
		public void RaiseTextChanged()
		{
			OnTextChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Программная замена текущего выделения строкой text с гарантированной
		/// записью в undo-стек RichTextBox. Работает надёжнее, чем SelectedText.
		/// </summary>
		public void ReplaceSelectionWithUndo(string text)
		{
			if (text == null)
				text = "";

			// Останавливаем текущую группу ввода: всё, что было набрано,
			// фиксируется как отдельный undo-шаг, а не сливается с нашей вставкой.
			SendMessage(Handle, EM_STOPGROUPTYPING, IntPtr.Zero, IntPtr.Zero);

			IntPtr ptr = Marshal.StringToHGlobalUni(text);
			try
			{
				// wParam = 0 → записать в undo-стек.
				SendMessage(Handle, EM_REPLACESEL, IntPtr.Zero, ptr);
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}

			// Закрываем нашу операцию — следующий ввод пойдёт отдельной группой.
			SendMessage(Handle, EM_STOPGROUPTYPING, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
