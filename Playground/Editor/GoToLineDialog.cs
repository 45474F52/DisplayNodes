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
	internal sealed class GoToLineDialog : Form
	{
		private readonly TextBox _txtLine;
		private readonly Label _lblHint;

		public int Result { get; private set; }

		public GoToLineDialog(int totalLines, int currentLine)
		{
			Text = "Перейти к строке";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			StartPosition = FormStartPosition.CenterParent;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(260, 100);
			BackColor = Color.FromArgb(45, 45, 48);
			ForeColor = Color.Gainsboro;
			Font = new Font("Segoe UI", 9f);

			_lblHint = new Label
			{
				Text = string.Format("Строка (1 - {0}):", totalLines),
				Location = new Point(12, 12),
				Size = new Size(236, 18),
				ForeColor = Color.Gainsboro
			};

			_txtLine = new TextBox
			{
				Location = new Point(12, 34),
				Size = new Size(236, 22),
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro,
				BorderStyle = BorderStyle.FixedSingle,
				Text = currentLine.ToString()
			};
			_txtLine.SelectAll();

			var btnOk = new Button
			{
				Text = "ОК",
				DialogResult = DialogResult.OK,
				Location = new Point(92, 66),
				Size = new Size(75, 24),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro
			};

			var btnCancel = new Button
			{
				Text = "Отмена",
				DialogResult = DialogResult.Cancel,
				Location = new Point(173, 66),
				Size = new Size(75, 24),
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(60, 60, 60),
				ForeColor = Color.Gainsboro
			};

			Controls.Add(_lblHint);
			Controls.Add(_txtLine);
			Controls.Add(btnOk);
			Controls.Add(btnCancel);

			AcceptButton = btnOk;
			CancelButton = btnCancel;

			// Кнопки OK — валидация номера до закрытия.
			btnOk.Click += BtnOk_Click;
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (!int.TryParse(_txtLine.Text.Trim(), out int value) || value < 1)
			{
				DialogResult = DialogResult.None;
				_txtLine.SelectAll();
				_txtLine.Focus();
				return;
			}

			Result = value;
		}
	}
}
