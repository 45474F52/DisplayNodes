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

using System.Drawing;
using System.Windows.Forms;

namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Диалог настроек приложения. Три вкладки: Редактор, Производительность, Интерфейс.
	/// </summary>
	public class SettingsDialog : Form
	{
		private readonly EditorSettings _settings;

		// Элементы управления: Редактор
		private ComboBox _cmbFontFamily;
		private NumericUpDown _numFontSize;
		private NumericUpDown _numIndentSize;
		private CheckBox _chkUseTabs;
		private CheckBox _chkLineNumbers;
		private CheckBox _chkIndentGuides;
		private CheckBox _chkErrorUnderlines;
		private CheckBox _chkCurrentLineHighlight;

		// Элементы управления: Производительность
		private NumericUpDown _numAutoRunDebounce;
		private NumericUpDown _numLayoutDebounce;
		private NumericUpDown _numHighlightSlow;
		private NumericUpDown _numHighlightFast;

		// Элементы управления: Интерфейс
		private NumericUpDown _numPreviewWidth;
		private NumericUpDown _numPreviewHeight;

		public SettingsDialog(EditorSettings settings)
		{
			_settings = settings;

			Text = "Настройки Playground";
			Size = new Size(520, 450);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			StartPosition = FormStartPosition.CenterParent;

			var tabs = new TabControl { Dock = DockStyle.Fill };
			tabs.TabPages.Add(CreateEditorTab());
			tabs.TabPages.Add(CreatePerformanceTab());
			tabs.TabPages.Add(CreateInterfaceTab());

			var btnPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Bottom,
				Height = 50,
				FlowDirection = FlowDirection.RightToLeft,
				Padding = new Padding(10)
			};
			var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Width = 80 };
			var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
			btnOk.Click += (s, e) => ApplySettings();

			btnPanel.Controls.Add(btnCancel);
			btnPanel.Controls.Add(btnOk);

			Controls.Add(tabs);
			Controls.Add(btnPanel);

			LoadSettings();
		}

		private TabPage CreateEditorTab()
		{
			var page = new TabPage("Редактор");
			var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(12) };
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

			int row = 0;

			panel.Controls.Add(new Label { Text = "Шрифт:", Dock = DockStyle.Fill }, 0, row);
			_cmbFontFamily = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			foreach (var f in FontFamily.Families)
				if (f.IsStyleAvailable(FontStyle.Regular))
					_cmbFontFamily.Items.Add(f.Name);
			panel.Controls.Add(_cmbFontFamily, 1, row++);

			panel.Controls.Add(new Label { Text = "Размер шрифта:", Dock = DockStyle.Fill }, 0, row);
			_numFontSize = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 6, Maximum = 72, DecimalPlaces = 1, Increment = 0.5m };
			panel.Controls.Add(_numFontSize, 1, row++);

			panel.Controls.Add(new Label { Text = "Размер отступа (пробелы):", Dock = DockStyle.Fill }, 0, row);
			_numIndentSize = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 8, Increment = 1 };
			panel.Controls.Add(_numIndentSize, 1, row++);

			panel.Controls.Add(new Label { Text = "Использовать табы:", Dock = DockStyle.Fill }, 0, row);
			_chkUseTabs = new CheckBox { Dock = DockStyle.Fill };
			panel.Controls.Add(_chkUseTabs, 1, row++);

			panel.Controls.Add(new Label { Text = "Номера строк:", Dock = DockStyle.Fill }, 0, row);
			_chkLineNumbers = new CheckBox { Dock = DockStyle.Fill };
			panel.Controls.Add(_chkLineNumbers, 1, row++);

			panel.Controls.Add(new Label { Text = "Направляющие отступа:", Dock = DockStyle.Fill }, 0, row);
			_chkIndentGuides = new CheckBox { Dock = DockStyle.Fill };
			panel.Controls.Add(_chkIndentGuides, 1, row++);

			panel.Controls.Add(new Label { Text = "Подчёркивание ошибок:", Dock = DockStyle.Fill }, 0, row);
			_chkErrorUnderlines = new CheckBox { Dock = DockStyle.Fill };
			panel.Controls.Add(_chkErrorUnderlines, 1, row++);

			panel.Controls.Add(new Label { Text = "Подсветка текущей строки:", Dock = DockStyle.Fill }, 0, row);
			_chkCurrentLineHighlight = new CheckBox { Dock = DockStyle.Fill };
			panel.Controls.Add(_chkCurrentLineHighlight, 1, row++);

			page.Controls.Add(panel);
			return page;
		}

		private TabPage CreatePerformanceTab()
		{
			var page = new TabPage("Производительность");
			var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

			int row = 0;
			panel.Controls.Add(new Label { Text = "Задержка автозапуска (мс):", Dock = DockStyle.Fill }, 0, row);
			_numAutoRunDebounce = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 50, Maximum = 2000, Increment = 50 };
			panel.Controls.Add(_numAutoRunDebounce, 1, row++);

			panel.Controls.Add(new Label { Text = "Задержка перерисовки layout (мс):", Dock = DockStyle.Fill }, 0, row);
			_numLayoutDebounce = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 10, Maximum = 1000, Increment = 10 };
			panel.Controls.Add(_numLayoutDebounce, 1, row++);

			panel.Controls.Add(new Label { Text = "Медленная подсветка синтаксиса (мс):", Dock = DockStyle.Fill }, 0, row);
			_numHighlightSlow = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 50, Maximum = 2000, Increment = 50 };
			panel.Controls.Add(_numHighlightSlow, 1, row++);

			panel.Controls.Add(new Label { Text = "Быстрая подсветка синтаксиса (мс):", Dock = DockStyle.Fill }, 0, row);
			_numHighlightFast = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 10, Maximum = 500, Increment = 10 };
			panel.Controls.Add(_numHighlightFast, 1, row++);

			page.Controls.Add(panel);
			return page;
		}

		private TabPage CreateInterfaceTab()
		{
			var page = new TabPage("Интерфейс");
			var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12) };
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

			int row = 0;
			panel.Controls.Add(new Label { Text = "Ширина preview по умолчанию:", Dock = DockStyle.Fill }, 0, row);
			_numPreviewWidth = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 100, Maximum = 4000, Increment = 10 };
			panel.Controls.Add(_numPreviewWidth, 1, row++);

			panel.Controls.Add(new Label { Text = "Высота preview по умолчанию:", Dock = DockStyle.Fill }, 0, row);
			_numPreviewHeight = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 100, Maximum = 4000, Increment = 10 };
			panel.Controls.Add(_numPreviewHeight, 1, row++);

			page.Controls.Add(panel);
			return page;
		}

		private void LoadSettings()
		{
			// Editor
			_cmbFontFamily.SelectedItem = _settings.FontFamily;
			_numFontSize.Value = (decimal)_settings.FontSize;
			_numIndentSize.Value = _settings.IndentSize;
			_chkUseTabs.Checked = _settings.UseTabs;
			_chkLineNumbers.Checked = _settings.ShowLineNumbers;
			_chkIndentGuides.Checked = _settings.ShowIndentGuides;
			_chkErrorUnderlines.Checked = _settings.ShowErrorUnderlines;
			_chkCurrentLineHighlight.Checked = _settings.ShowCurrentLineHighlight;

			// Performance
			_numAutoRunDebounce.Value = _settings.AutoRunDebounceMs;
			_numLayoutDebounce.Value = _settings.LayoutDebounceMs;
			_numHighlightSlow.Value = _settings.HighlightSlowMs;
			_numHighlightFast.Value = _settings.HighlightFastMs;

			// Interface
			_numPreviewWidth.Value = _settings.PreviewDefaultWidth;
			_numPreviewHeight.Value = _settings.PreviewDefaultHeight;
		}

		private void ApplySettings()
		{
			// Editor
			_settings.FontFamily = _cmbFontFamily.SelectedItem as string ?? "Consolas";
			_settings.FontSize = (float)_numFontSize.Value;
			_settings.IndentSize = (int)_numIndentSize.Value;
			_settings.UseTabs = _chkUseTabs.Checked;
			_settings.ShowLineNumbers = _chkLineNumbers.Checked;
			_settings.ShowIndentGuides = _chkIndentGuides.Checked;
			_settings.ShowErrorUnderlines = _chkErrorUnderlines.Checked;
			_settings.ShowCurrentLineHighlight = _chkCurrentLineHighlight.Checked;

			// Performance
			_settings.AutoRunDebounceMs = (int)_numAutoRunDebounce.Value;
			_settings.LayoutDebounceMs = (int)_numLayoutDebounce.Value;
			_settings.HighlightSlowMs = (int)_numHighlightSlow.Value;
			_settings.HighlightFastMs = (int)_numHighlightFast.Value;

			// Interface
			_settings.PreviewDefaultWidth = (int)_numPreviewWidth.Value;
			_settings.PreviewDefaultHeight = (int)_numPreviewHeight.Value;
		}
	}
}
