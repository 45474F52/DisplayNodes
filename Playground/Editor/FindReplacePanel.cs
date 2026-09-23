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

using DisplayNodes.Playground.Properties;

namespace DisplayNodes.Playground.Editor
{
	internal sealed class FindReplacePanel : Panel
	{
		/// <summary>
		/// Набор иконок для панели поиска/замены
		/// </summary>
		internal sealed class FindReplaceIcons
		{
			public Image CaseSensitive { get; } = Resources.Match_Case;
			public Image Regex { get; } = Resources.RegExp;
			public Image Next { get; } = Resources.Arrow_Down;
			public Image Prev { get; } = Resources.Arrow_Up;
			public Image Replace { get; } = Resources.Replace;
			public Image ReplaceAll { get; } = Resources.Replace_All;
			public Image Close { get; } = Resources.CloseTab;
			public Image CaseSensitiveOff { get; } = Resources.Match_Case_Off;
			public Image RegexOff { get; } = Resources.RegExp_Off;
		}

		private readonly TextBox _txtFind;
		private readonly TextBox _txtReplace;
		private readonly CheckBox _chkCase;
		private readonly CheckBox _chkRegex;
		private readonly Button _btnNext;
		private readonly Button _btnPrev;
		private readonly Button _btnReplace;
		private readonly Button _btnReplaceAll;
		private readonly Button _btnClose;
		private readonly Label _lblStatus;

		private readonly SearchEngine _searchEngine = new SearchEngine();
		private readonly FindReplaceIcons _icons;

		private RichTextBox _editor;

		private const int RowHeight = 22;
		private const int RowGap = 6;
		private const int PanelPadding = 6;
		private const int StatusHeight = 18;

		private const int SingleRowHeight = PanelPadding * 2 + RowHeight + RowGap + StatusHeight;
		private const int DoubleRowHeight = PanelPadding * 2 + RowHeight * 2 + RowGap * 2 + StatusHeight;

		// Размер квадратной иконки-кнопки. Подберите под ваши картинки
		// (16×16, 20×20, 24×24 — что у вас есть).
		private const int IconSize = 16;
		private const int IconButtonWidth = 26;
		private const int IconButtonWideWidth = 34;

		public FindReplacePanel()
		{
			_icons = new FindReplaceIcons();

			BackColor = Color.FromArgb(45, 45, 48);
			Padding = new Padding(PanelPadding);
			Size = new Size(420, DoubleRowHeight);

			_txtFind = new TextBox
			{
				Width = 180,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro,
				BorderStyle = BorderStyle.FixedSingle
			};

			_txtReplace = new TextBox
			{
				Width = 180,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro,
				BorderStyle = BorderStyle.FixedSingle
			};

			// ---- чекбоксы с иконками ----
			// Appearance = Button + FlatStyle = Flat даёт квадратную кнопку,
			// которая визуально "вжата" при Checked = true.
			_chkCase = MakeIconToggle(_icons.CaseSensitive, _icons.CaseSensitiveOff, "Учитывать регистр");
			_chkRegex = MakeIconToggle(_icons.Regex, _icons.RegexOff, "Регулярное выражение");

			// ---- кнопки с иконками ----
			_btnNext = MakeIconButton(_icons.Next, "Далее (Enter)", IconButtonWidth);
			_btnPrev = MakeIconButton(_icons.Prev, "Назад (Shift+Enter)", IconButtonWidth);
			_btnReplace = MakeIconButton(_icons.Replace, "Заменить", IconButtonWidth);
			_btnReplaceAll = MakeIconButton(_icons.ReplaceAll, "Заменить всё", IconButtonWideWidth);
			_btnClose = MakeIconButton(_icons.Close, "Закрыть (Esc)", IconButtonWidth);

			_lblStatus = new Label
			{
				Size = new Size(90, RowHeight),
				ForeColor = Color.Gray,
				Text = "",
				TextAlign = ContentAlignment.MiddleLeft
			};

			Controls.Add(_txtFind);
			Controls.Add(_txtReplace);
			Controls.Add(_chkCase);
			Controls.Add(_chkRegex);
			Controls.Add(_btnNext);
			Controls.Add(_btnPrev);
			Controls.Add(_btnReplace);
			Controls.Add(_btnReplaceAll);
			Controls.Add(_btnClose);
			Controls.Add(_lblStatus);

			// Смена иконки чекбокса при переключении состояния
			// (если заданы отдельные Off-иконки).
			_chkCase.CheckedChanged += (s, e) => UpdateToggleImage(_chkCase, _icons.CaseSensitiveOff, _icons.CaseSensitive);
			_chkRegex.CheckedChanged += (s, e) => UpdateToggleImage(_chkRegex, _icons.RegexOff, _icons.Regex);

			_txtFind.KeyDown += TxtFind_KeyDown;
			_txtFind.TextChanged += (s, e) => AutoFindNext();
			_txtReplace.KeyDown += TxtFind_KeyDown;
			_btnNext.Click += (s, e) => FindNext();
			_btnPrev.Click += (s, e) => FindPrev();
			_btnReplace.Click += (s, e) => ReplaceCurrent();
			_btnReplaceAll.Click += (s, e) => ReplaceAll();
			_btnClose.Click += (s, e) => ClosePanel();
		}

		private static Button MakeIconButton(Image image, string tooltip, int width)
		{
			if (image != null)
				image = new Bitmap(image, new Size(IconSize, IconSize));

			var btn = new Button
			{
				Size = new Size(width, RowHeight),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro,
				TabStop = false,
				Image = image,
				ImageAlign = ContentAlignment.MiddleCenter,
				Text = "",
				TextImageRelation = TextImageRelation.Overlay
			};
			btn.FlatAppearance.BorderSize = 0;
			if (!string.IsNullOrEmpty(tooltip))
				new ToolTip().SetToolTip(btn, tooltip);
			return btn;
		}

		private static Bitmap ResizeImage(Image image) => new Bitmap(image, new Size(IconSize, IconSize));

		private static CheckBox MakeIconToggle(Image onImage, Image offImage, string tooltip)
		{
			if (onImage != null)
				onImage = ResizeImage(onImage);
			if (offImage != null)
				offImage = ResizeImage(offImage);

			var chk = new CheckBox
			{
				Size = new Size(IconButtonWidth, RowHeight),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro,
				TabStop = false,
				Appearance = Appearance.Button,
				TextAlign = ContentAlignment.MiddleCenter,
				ImageAlign = ContentAlignment.MiddleCenter,
				Text = ""
			};
			chk.FlatAppearance.BorderSize = 0;
			chk.FlatAppearance.CheckedBackColor = Color.FromArgb(80, 90, 110);
			chk.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 74);

			chk.Image = offImage ?? onImage;
			chk.ImageAlign = ContentAlignment.MiddleCenter;

			if (!string.IsNullOrEmpty(tooltip))
				new ToolTip().SetToolTip(chk, tooltip);

			return chk;
		}

		/// <summary>
		/// Если заданы отдельные иконки для Off/On — переключает их.
		/// Если Off-иконки нет, состояние показывается только фоном
		/// (CheckedBackColor) — иконка остаётся одной и той же.
		/// </summary>
		private static void UpdateToggleImage(CheckBox chk, Image off, Image on)
		{
			if (off == null)
				return;
			chk.Image = new Bitmap(chk.Checked ? on : off, new Size(IconSize, IconSize));
		}

		public void ShowPanel(RichTextBox editor, bool replaceMode, string initialFind, string initialReplace)
		{
			_editor = editor;

			_txtFind.Text = initialFind ?? "";
			_txtReplace.Text = initialReplace ?? "";

			LayoutForMode(replaceMode);

			Visible = true;
			BringToFront();
			_txtFind.Focus();
			_txtFind.SelectAll();

			Status("");
		}

		private void LayoutForMode(bool replaceMode)
		{
			int left = Padding.Left;
			int top = Padding.Top;
			int row2 = top + RowHeight + RowGap;

			_txtFind.Location = new Point(left, top);
			_txtFind.Width = 180;

			int x = left + _txtFind.Width + RowGap;

			_chkCase.Location = new Point(x, top);
			x += _chkCase.Width;

			_chkRegex.Location = new Point(x, top);
			x += _chkRegex.Width;

			int statusTop;
			if (replaceMode)
			{
				_btnNext.Location = new Point(left + _txtFind.Width + RowGap, row2);
				_btnPrev.Location = new Point(_btnNext.Right, row2);
				_btnReplace.Location = new Point(_btnPrev.Right + RowGap, row2);
				_btnReplaceAll.Location = new Point(_btnReplace.Right, row2);

				_btnClose.Location = new Point(x, top);

				_txtReplace.Location = new Point(left, row2);

				_txtReplace.Visible = true;
				_btnReplace.Visible = true;
				_btnReplaceAll.Visible = true;
				_btnNext.Visible = true;
				_btnPrev.Visible = true;

				statusTop = row2 + RowHeight + RowGap;
				Height = DoubleRowHeight;
			}
			else
			{
				_btnNext.Location = new Point(x, top);
				x += _btnNext.Width;

				_btnPrev.Location = new Point(x, top);
				x += _btnPrev.Width;

				_btnClose.Location = new Point(x, top);
				x += _btnClose.Width;

				_txtReplace.Visible = false;
				_btnReplace.Visible = false;
				_btnReplaceAll.Visible = false;
				_btnNext.Visible = true;
				_btnPrev.Visible = true;

				statusTop = top + RowHeight + RowGap;
				Height = SingleRowHeight;
			}

			_lblStatus.Location = new Point(left, statusTop);
			_lblStatus.Size = new Size(
				ClientSize.Width - Padding.Left - Padding.Right,
				StatusHeight);
			_lblStatus.TextAlign = ContentAlignment.MiddleLeft;
		}

		public void HidePanel() { Visible = false; }

		private void ClosePanel()
		{
			Visible = false;
			if (_editor != null && !_editor.IsDisposed)
				_editor.Focus();
		}

		public bool ReplaceMode { get { return _txtReplace.Visible; } }
		public string FindText { get { return _txtFind.Text; } }
		public string ReplaceText { get { return _txtReplace.Text; } }

		private void TxtFind_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (e.Shift)
					FindPrev();
				else
					FindNext();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				ClosePanel();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void AutoFindNext() { FindInternal(forward: true, select: false); }

		public void FindNext() { FindInternal(forward: true, select: true); }
		public void FindPrev() { FindInternal(forward: false, select: true); }

		private bool FindInternal(bool forward, bool select)
		{
			if (_editor == null || _editor.IsDisposed)
				return false;

			string query = _txtFind.Text;
			if (string.IsNullOrEmpty(query))
			{
				Status("");
				return false;
			}

			bool matchCase = _chkCase.Checked;
			bool regex = _chkRegex.Checked;

			int selStart = _editor.SelectionStart;
			int selLength = _editor.SelectionLength;

			// Граница поиска.
			// Вперёд: строго после текущего выделения.
			// Назад:  строго до начала текущего выделения.
			int start = forward ? selStart + selLength : selStart;

			int found;
			try
			{
				found = _searchEngine.FindNext(
					_editor.Text, query, start, regex, matchCase, forward);
			}
			catch (ArgumentException)
			{
				Status("ошибка regex");
				return false;
			}

			if (found < 0)
			{
				try
				{
					found = forward
						? _searchEngine.FindNext(_editor.Text, query, 0, regex, matchCase, true)
						: _searchEngine.FindNext(_editor.Text, query, _editor.TextLength, regex, matchCase, false);
				}
				catch (ArgumentException)
				{
					Status("ошибка regex");
					return false;
				}

				if (found < 0)
				{
					Status("не найдено");
					return false;
				}
			}

			if (select)
			{
				_editor.Select(found, query.Length);
				_editor.ScrollToCaret();
			}
			Status("");
			return true;
		}

		private void ReplaceCurrent()
		{
			if (_editor == null)
				return;
			if (_editor.SelectionLength == 0)
			{
				if (!FindInternal(true, true))
					return;
			}

			string selected = _editor.SelectedText;
			string query = _txtFind.Text;
			bool matches = _chkCase.Checked
				? selected == query
				: string.Equals(selected, query, StringComparison.OrdinalIgnoreCase);

			if (!matches)
			{
				if (!FindInternal(true, true))
					return;
			}

			_editor.SelectedText = _txtReplace.Text;
			FindInternal(true, true);
		}

		private void ReplaceAll()
		{
			if (_editor == null)
				return;

			string query = _txtFind.Text;
			if (string.IsNullOrEmpty(query))
				return;

			string text = _editor.Text;
			string replacement = _txtReplace.Text;
			bool matchCase = _chkCase.Checked;

			string newText = _searchEngine.ReplaceAll(text, query, replacement, _chkRegex.Checked, matchCase, out int count);

			if (count == 0)
			{ Status("не найдено"); return; }

			_editor.Text = newText;
			Status(string.Format("заменено {0}", count));
		}

		private void Status(string msg) { _lblStatus.Text = msg; }
	}
}
