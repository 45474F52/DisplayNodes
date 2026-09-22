using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Editor
{
	internal sealed class SyntaxHighlighter : IDisposable
	{
        private const int WM_USER = 0x0400;
		private const int WM_SETREDRAW = 0x000B;
		private const int EM_GETSCROLLPOS = WM_USER + 221;  // 0x04DD
		private const int EM_SETSCROLLPOS = WM_USER + 222;  // 0x04DE

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT { public int X; public int Y; }

		// в SyntaxHighlighter
		[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
		private struct CHARFORMAT2
		{
			public int cbSize;
			public uint dwMask;
			public uint dwEffects;
			public int yHeight;
			public int yOffset;
			public int crTextColor;
			public byte bCharSet;
			public byte bPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szFaceName;
			public ushort wWeight;
			public short sSpacing;
			public int crBackColor;
			public int lcid;
			public uint dwReserved;
			public short sStyle;
			public ushort wKerning;
			public byte bUnderlineType;
			public byte bAnimation;
			public byte bRevAuthor;
			public byte bReserved1;
		}

		private const int EM_SETCHARFORMAT = 0x0444;
		private const uint CFM_COLOR = 0x40000000;
		private const uint SCF_SELECTION = 0x0001;
		private const uint SCF_DONOTUNDO = 0x2000;

		private void SetSelectionColorNoUndo(Color color)
		{
			CHARFORMAT2 cf = new CHARFORMAT2();
			cf.cbSize = Marshal.SizeOf(typeof(CHARFORMAT2));
			cf.dwMask = CFM_COLOR;
			cf.crTextColor = color.R | (color.G << 8) | (color.B << 16);
			cf.szFaceName = "";

			IntPtr ptr = Marshal.AllocHGlobal(cf.cbSize);
			try
			{
				Marshal.StructureToPtr(cf, ptr, false);
				SendMessage(_editor.Handle, EM_SETCHARFORMAT,
							(IntPtr)(SCF_SELECTION | SCF_DONOTUNDO), ptr);
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private readonly Color _colorDefault;
        private readonly Color _colorKeyword;
        private readonly Color _colorControlFlow;
        private readonly Color _colorType;
        private readonly Color _colorInterface;
        private readonly Color _colorMethod;
        private readonly Color _colorStruct;
        private readonly Color _colorString;
        private readonly Color _colorComment;
        private readonly Color _colorNumber;
        private readonly Color _colorPreprocessor;

        private readonly int _slowInterval;
		private readonly int _fastInterval;
		private readonly RichTextBox _editor;
		private readonly Timer _debounce;

		/// <summary>True, пока идёт Apply(). CodeEditor по этому флагу глушит перерисовку жёлоба.</summary>
		public bool IsApplying { get; private set; }

		public SyntaxHighlighter(RichTextBox editor, EditorSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			_editor = editor ?? throw new ArgumentNullException(nameof(editor));

			_slowInterval = settings.HighlightSlowMs;
			_fastInterval = settings.HighlightFastMs;

			_debounce = new Timer { Interval = _slowInterval };
			_debounce.Tick += (s, e) => { _debounce.Stop(); Apply(); };

            _colorDefault = settings.ColorDefault;
            _colorKeyword = settings.ColorKeyword;
            _colorControlFlow = settings.ColorControlFlow;
            _colorType = settings.ColorType;
            _colorInterface = settings.ColorInterface;
            _colorMethod = settings.ColorMethod;
            _colorStruct = settings.ColorStruct;
            _colorString = settings.ColorString;
            _colorComment = settings.ColorComment;
            _colorNumber = settings.ColorNumber;
            _colorPreprocessor = settings.ColorPreprocessor;
        }

		/// <summary>Перезапустить таймер. Вызывается из TextChanged.</summary>
		public void RequestHighlight()
		{
			RequestHighlight(false);
		}

		/// <summary>
		/// Перезапустить таймер подсветки.
		/// <paramref name="fast"/> = true → короткая задержка (для Backspace/Delete/Enter).
		/// </summary>
		public void RequestHighlight(bool fast)
		{
			_debounce.Stop();
			_debounce.Interval = fast ? _fastInterval : _slowInterval;
			_debounce.Start();
		}

		/// <summary>Применить подсветку немедленно.</summary>
		public void Apply()
		{
			if (IsApplying)
				return;
			if (_editor.IsDisposed)
				return;

			if (_editor.Capture && Control.MouseButtons == MouseButtons.Left)
			{
				_debounce.Stop();
				_debounce.Start();
				return;
			}

			IsApplying = true;

			ScrollRichTextBox scrollEditor = _editor as ScrollRichTextBox;
			if (scrollEditor != null)
				scrollEditor.SuppressTextChanged = true;

			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			// Перевести редактор в «тихий» режим: ничего не мигает.
			SendMessage(_editor.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

			IntPtr scrollBuf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(POINT)));
			try
			{
				// Запомнили позицию скролла.
				SendMessage(_editor.Handle, EM_GETSCROLLPOS, IntPtr.Zero, scrollBuf);

				// ---- 1. Сбросить цвет всего текста на Default ----
				_editor.Select(0, _editor.TextLength);
				SetSelectionColorNoUndo(_colorDefault);

				// ---- 2. Разложить текст на токены ----
				string text = _editor.Text;
				List<Token> tokens = CSharpLexer.Tokenize(text);

				// ---- 3. Раскрасить токены ----
				// Идём слева направо — так SelectionStart принимает корректные значения.
				for (int t = 0; t < tokens.Count; t++)
				{
					Token tok = tokens[t];
					if (tok.Start < 0 || tok.Start + tok.Length > _editor.TextLength)
						continue;

					_editor.Select(tok.Start, tok.Length);
					SetSelectionColorNoUndo(ColorFor(tok.Type));
				}

				// ---- 4. Восстановить позицию скролла ----
				SendMessage(_editor.Handle, EM_SETSCROLLPOS, IntPtr.Zero, scrollBuf);

				// ---- 5. Восстановить выделение и цвет ввода ----
				// Сначала снимаем выделение, чтобы установка цвета не перекрасила
				// случайно выделенный символ, потом возвращаем.
				if (selStart > _editor.TextLength)
					selStart = _editor.TextLength;
				if (selStart + selLength > _editor.TextLength)
					selLength = _editor.TextLength - selStart;

				_editor.SelectionStart = selStart;
				_editor.SelectionLength = 0;
				SetSelectionColorNoUndo(_colorDefault); // цвет для следующего набранного символа
				_editor.SelectionLength = selLength;
			}
			catch
			{

			}
			finally
			{
				Marshal.FreeHGlobal(scrollBuf);

				SendMessage(_editor.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
				if (scrollEditor != null)
					scrollEditor.SuppressTextChanged = false;
				_editor.Invalidate();
				IsApplying = false;
			}
		}

		private Color ColorFor(TokenType type)
		{
			switch (type)
            {
                case TokenType.Keyword: return _colorKeyword;
                case TokenType.ControlFlow: return _colorControlFlow;
                case TokenType.Type: return _colorType;
                case TokenType.Interface: return _colorInterface;
                case TokenType.Method: return _colorMethod;
                case TokenType.Struct: return _colorStruct;
                case TokenType.String: return _colorString;
                case TokenType.Comment: return _colorComment;
                case TokenType.Number: return _colorNumber;
                case TokenType.Preprocessor: return _colorPreprocessor;
                default: return _colorDefault;
            }
		}

		public void Dispose()
		{
			if (_debounce != null)
			{
				_debounce.Stop();
				_debounce.Dispose();
			}
		}
	}
}
