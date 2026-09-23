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

using DisplayNodes.Core;
using DisplayNodes.Playground.Compilation;
using DisplayNodes.Playground.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace DisplayNodes.Playground
{
	public partial class AppForm : Form
	{
		private readonly AppViewModel _vm;
		private readonly AppSettings _appSettings = new AppSettings();
		private readonly EditorSettings _settings = new EditorSettings();

		private IPreviewHost _preview;

		private Timer _autoRunTimer;
		private Timer _layoutDebounceTimer;

		private bool _restoringState;
		private bool _isRunning;

		private void InitializeCodeEditor()
        {
            codeEditor = new Editor.CodeEditor(_settings)
            {
                Dock = DockStyle.Fill,
                Text = DefaultScripts.HELLO,
				BackColor = Color.FromArgb(30, 30, 30),
				Location = new Point(0, 0),
				Margin = new Padding(4),
				Name = "codeEditor",
				Size = new Size(659, 696),
				TabIndex = 0,
            };

            mainSplitter.Panel2.Controls.Clear();
            mainSplitter.Panel2.Controls.Add(codeEditor);

            codeEditor.Editor.KeyDown += TxtEdit_KeyDown;
            codeEditor.Editor.KeyUp += TxtEdit_KeyUp;
            codeEditor.Editor.TextChanged += TxtEdit_TextChanged;
            codeEditor.Editor.MouseClick += TxtEdit_MouseClick;
        }

		public AppForm()
		{
			_settings.Load(_appSettings);

			var recent = new RecentFilesManager(_settings);
			var runner = new ScriptRunner(_settings);
			_vm = new AppViewModel(_appSettings, recent, runner);

			InitializeComponent();
			InitializeCodeEditor();

			_preview = new WinFormsPreviewHost(previewHost);

            _autoRunTimer = new Timer
            {
                Interval = _settings.AutoRunDebounceMs
            };
            _autoRunTimer.Tick += OnAutoRunTimerTick;

            _layoutDebounceTimer = new Timer
            {
                Interval = _settings.LayoutDebounceMs
            };
            _layoutDebounceTimer.Tick += OnLayoutDebounceTick;

			mainSplitter.SplitterMoved += OnSplitterMoved;
			viewLogsSplitter.SplitterMoved += OnSplitterMoved;

			this.Resize += OnFormResize;
			this.ResizeEnd += OnFormResizeEnd;

			codeEditor.Editor.KeyDown += TxtEdit_KeyDown;
			codeEditor.Editor.KeyUp += TxtEdit_KeyUp;
			codeEditor.Editor.TextChanged += TxtEdit_TextChanged;
			codeEditor.Editor.MouseClick += TxtEdit_MouseClick;
			codeEditor.Text = DefaultScripts.HELLO;

			BtnRecent.DropDownOpening += BtnRecent_DropDownOpening;
			this.errorContextMenu.Opening += ErrorContextMenu_Opening;

			_vm.OnIsDirtyChanged += isDirty =>
			{
				if (isDirty)
				{
					Text += "*";
				}
				else
				{
					Text = Text.Replace("*", string.Empty);
				}
			};

			UpdateLineCol();
			RestoreState();
			UpdateCompilerModeLabel();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.WindowsShutDown
				|| e.CloseReason == CloseReason.TaskManagerClosing)
			{

			}
			else if (_vm.IsDirty && !ConfirmClose())
			{
				e.Cancel = true;
				return;
			}

			SaveState();
			_settings.Save(_appSettings);
			_appSettings.Save();

			_preview?.Dispose();
			_preview = null;
			_layoutDebounceTimer?.Dispose();
			_layoutDebounceTimer = null;
			_autoRunTimer?.Dispose();
			_autoRunTimer = null;
			_vm?.Dispose();

			base.OnFormClosing(e);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			ApplySplitterState();

			statusStrip.PerformLayout();

			this.BeginInvoke(new Action(RunScript));
		}

		/// <summary>
		/// Событие Resize приходит десятки раз в секунду при перетаскивании
		/// рамки окна. Не запускаем RunScript сразу — только перезапускаем
		/// дебаунс-таймер. Он выстрелит, когда пользователь остановится.
		/// </summary>
		private void OnFormResize(object sender, EventArgs e)
		{
			if (_restoringState)
				return;
			RestartLayoutDebounce();
		}

		/// <summary>
		/// ResizeEnd срабатывает один раз, когда пользователь отпустил рамку.
		/// Мгновенно перезапускаем скрипт, отменяя дебаунс — финальный размер
		/// уже не изменится.
		/// </summary>
		private void OnFormResizeEnd(object sender, EventArgs e)
		{
			if (_restoringState)
				return;
			_layoutDebounceTimer.Stop();
			RelayoutPreview();
		}

		/// <summary>
		/// SplitterMoved срабатывает и при перетаскивании пользователем, и
		/// при программном изменении размеров. В обоих случаях достаточно
		/// дебаунса — если пользователь тащит сплиттер, скрипт выполнится
		/// после того, как он остановится.
		/// </summary>
		private void OnSplitterMoved(object sender, SplitterEventArgs e)
		{
			if (_restoringState)
				return;
			RestartLayoutDebounce();
		}

		private void RestartLayoutDebounce()
		{
			_layoutDebounceTimer.Stop();
			_layoutDebounceTimer.Start();
		}

		private void OnLayoutDebounceTick(object sender, EventArgs e)
		{
			_layoutDebounceTimer.Stop();

			if (_isRunning)
				return;

			_isRunning = true;

			try
			{
				RelayoutPreview();
			}
			finally
			{
				_isRunning = false;
			}
		}

		/// <summary>
		/// Быстрая перерисовка preview без перекомпиляции скрипта.
		/// Используется, когда изменился только размер области предпросмотра
		/// (ресайз окна, перетаскивание сплиттера), а сам код не менялся.
		/// Компиляция при этом не запускается — она не нужна, потому что
		/// LayoutNode уже построен и лежит в _currentRoot.
		/// </summary>
		private void RelayoutPreview()
        {
            if (_vm.CurrentRoot == null)
                return;

            Core.Size size = _preview.CurrentSize;
            _vm.CurrentRoot.Measure(size);
            _vm.CurrentRoot.Arrange(new Rect(Core.Point.Empty, size));

            _preview.Resize(size);
        }

		private void SaveState()
		{
			// Геометрия. Восстановление из Maximized идёт через RestoreBounds —
			// иначе при повторном запуске окно развернётся на весь экран,
			// но запомнит именно размер развёрнутого состояния, что нам не нужно.
			Rectangle bounds = (WindowState == FormWindowState.Normal)
				? Bounds
				: RestoreBounds;

			_vm.Settings.SetInt(StateKeys.WINDOW_X, bounds.X);
			_vm.Settings.SetInt(StateKeys.WINDOW_Y, bounds.Y);
			_vm.Settings.SetInt(StateKeys.WINDOW_WIDTH, bounds.Width);
			_vm.Settings.SetInt(StateKeys.WINDOW_HEIGHT, bounds.Height);

			_vm.Settings.SetString(StateKeys.WINDOW_STATE,
				WindowState == FormWindowState.Maximized ? "Maximized" : "Normal");

			// Сплиттеры.
			SaveSplitterRatio(mainSplitter, StateKeys.SPLITTER_MAIN_RATIO);
			SaveSplitterRatio(viewLogsSplitter, StateKeys.SPLITTER_VIEW_LOGS_RATIO);

			// AutoRun.
			_vm.Settings.SetBool(StateKeys.AUTO_RUN, btnAutoRun.Checked);

			// Последний файл. Сохраняем, если он есть в _recentFiles — то есть
			// пользователь недавно что-то сохранял/открывал. Берём первый элемент.
			IList<string> recent = _vm.RecentFiles.Items;
			if (recent != null && recent.Count > 0)
				_vm.Settings.SetString(StateKeys.LAST_FILE, recent[0]);

			_vm.Settings.Save();
			_vm.IsDirty = false;
		}

		private void RestoreState()
		{
			_restoringState = true;

			try
			{
				// Геометрия окна.
				int x = _vm.Settings.GetInt(StateKeys.WINDOW_X, int.MinValue);
				int y = _vm.Settings.GetInt(StateKeys.WINDOW_Y, int.MinValue);
				int w = _vm.Settings.GetInt(StateKeys.WINDOW_WIDTH, 0);
				int h = _vm.Settings.GetInt(StateKeys.WINDOW_HEIGHT, 0);

				if (w > 200 && h > 200)
				{
					// Проверяем, что окно влезает хоть в одну из доступных областей.
					// Если пользователь работал на другом мониторе, восстановление
					// за пределами экрана сделает окно недоступным.
					var bounds = new Rectangle(x, y, w, h);
					if (x != int.MinValue && y != int.MinValue && IsVisibleOnAnyScreen(bounds))
					{
						StartPosition = FormStartPosition.Manual;
						Location = new Point(x, y);
					}
					Size = new Size(w, h);
				}

				string windowState = _vm.Settings.GetString(StateKeys.WINDOW_STATE, "Normal");
				if (string.Equals(windowState, "Maximized", StringComparison.OrdinalIgnoreCase))
					WindowState = FormWindowState.Maximized;
				// Minimized не восстанавливаем — приложение должно открыться нормально.

				// Автозапуск.
				btnAutoRun.Checked = _vm.Settings.GetBool(StateKeys.AUTO_RUN, true);

				// Последний файл.
				string lastFile = _vm.Settings.GetString(StateKeys.LAST_FILE, null);
				if (!string.IsNullOrEmpty(lastFile) && File.Exists(lastFile))
				{
					string text = _vm.Load(lastFile);
					if (text != null)
					{
						codeEditor.Text = text;
						_vm.IsDirty = false;
						codeEditor.ResetUndo();
					}
				}
			}
			finally
			{
				_restoringState = false;
			}
		}

		/// <summary>
		/// Сохраняет отношение SplitterDistance / общий размер в долях (0..1).
		/// Если сплиттер ещё не разложен (размер 0), значение не сохраняется.
		/// </summary>
		private void SaveSplitterRatio(SplitContainer splitter, string key)
		{
			int total;
			int distance = splitter.SplitterDistance;

			if (splitter.Orientation == Orientation.Vertical)
				total = splitter.Width;
			else
				total = splitter.Height;

			if (total <= 0)
				return;

			double ratio = (double)distance / total;

			// Защита от невалидных значений.
			if (ratio <= 0 || ratio >= 1)
				return;

			_vm.Settings.SetString(key, ratio.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Восстанавливает позиции сплиттеров по сохранённым пропорциям.
		/// Вызывается после того, как форма прошла layout, — иначе размеры
		/// сплиттеров ещё не валидны.
		/// </summary>
		private void ApplySplitterState()
		{
			ApplySplitterRatio(mainSplitter, StateKeys.SPLITTER_MAIN_RATIO);
			ApplySplitterRatio(viewLogsSplitter, StateKeys.SPLITTER_VIEW_LOGS_RATIO);
		}

		private void ApplySplitterRatio(SplitContainer splitter, string key)
		{
			string raw = _vm.Settings.GetString(key, null);
			if (string.IsNullOrEmpty(raw))
				return;

			if (!double.TryParse(raw,
				System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture,
				out double ratio))
				return;

			if (ratio <= 0 || ratio >= 1)
				return;

			int total = (splitter.Orientation == Orientation.Vertical)
				? splitter.Width
				: splitter.Height;

			if (total <= 0)
				return;

			int distance = (int)(total * ratio);

			// Клампим на всякий случай.
			int min = splitter.Panel1MinSize;
			int max = total - splitter.Panel2MinSize - splitter.SplitterWidth;
			if (distance < min)
				distance = min;
			if (distance > max)
				distance = max;
			if (distance <= 0)
				return;

			try
			{
				splitter.SplitterDistance = distance;
			}
			catch (Exception ex)
			{
				AppLog.Error("AppForm.ApplySplitterRatio", ex);
			}
		}

		/// <summary>
		/// Проверяет, что прямоугольник bounds пересекается с одной из
		/// доступных рабочих областей хотя бы одним пикселем. Защита от
		/// ситуации, когда окно сохранилось на отключённом мониторе.
		/// </summary>
		private static bool IsVisibleOnAnyScreen(Rectangle bounds)
		{
			foreach (var screen in Screen.AllScreens)
			{
				if (screen.WorkingArea.IntersectsWith(bounds))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Спрашивает пользователя о несохранённых изменениях.
		/// Возвращает true, если можно закрывать форму.
		/// </summary>
		private bool ConfirmClose()
		{
			var result = MessageBox.Show(
				"В скрипте есть несохранённые изменения. Сохранить перед выходом?",
				"DisplayNodes Playground",
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question);

			switch (result)
			{
				case DialogResult.Yes:
				return SaveScript();
				case DialogResult.No:
				return true;
				default:
				return false;
			}
		}

		private void RunScript()
		{
			lstErrors.Items.Clear();

			var result = _vm.Run(codeEditor.Text);

			if (!result.Success)
			{
				string[] lines = result.ErrorOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
				var errorLineNumbers = new List<int>();
				foreach (string line in lines)
				{
					if (line.Length > 0)
					{
						lstErrors.Items.Add(line);
						if (TryParseErrorLine(line, out int userLine))
							errorLineNumbers.Add(userLine);
					}
				}
				codeEditor.SetErrorLines(errorLineNumbers);
				lblStatus.Text = "\u2716 Ошибка сборки";
				lblStatus.ForeColor = Color.FromArgb(240, 100, 100);
				return;
			}

			codeEditor.SetErrorLines(null);

			if (_vm.CurrentRoot != null)
            {
                _preview.Build(_vm.CurrentRoot);
            }

			lblStatus.Text = "\u2714 Сборка успешна";
			lblStatus.ForeColor = Color.FromArgb(100, 220, 100);
		}

		private void ClearAll()
        {
            codeEditor.ResetText();
            codeEditor.ResetUndo();
            codeEditor.SetErrorLines(null);
            lstErrors.Items.Clear();
            _preview.Clear();
            _vm.Clear();
            lblStatus.Text = "Очищено";
            lblStatus.ForeColor = SystemColors.ControlText;
            UpdateLineCol();
        }

		private bool SaveScript()
		{
			using (var dialog = new SaveFileDialog
			{
				Title = "Сохранение файла кода",
				Filter = "DisplayNodes Playground (*.dnp)|*.dnp",
				DefaultExt = "dnp",
				FileName = "code"
			})
			{
				if (dialog.ShowDialog() != DialogResult.OK)
					return false;

				if (!_vm.Save(dialog.FileName, codeEditor.Text))
				{
					_ = MessageBox.Show(
						"Не удалось сохранить файл.",
						"Ошибка сохранения",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return false;
				}

				return true;
			}
		}

		private void UpdateLineCol()
		{
			int index = codeEditor.Editor.SelectionStart;
			int line = codeEditor.Editor.GetLineFromCharIndex(index);
			int firstOfLine = codeEditor.Editor.GetFirstCharIndexFromLine(line);
			int col = index - firstOfLine;
			lblLineCol.Text = string.Format("Ln {0}, Col {1}", line + 1, col + 1);
		}

		private void OnAutoRunTimerTick(object sender, EventArgs e)
		{
			_autoRunTimer.Stop();

			if (_isRunning)
				return; // предыдущая компиляция ещё идёт — пропускаем тик

			_isRunning = true;

			try
			{
				RunScript();
			}
			finally
			{
				_isRunning = false;
			}
		}

		private void TxtEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				RunScript();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void TxtEdit_KeyUp(object sender, EventArgs e) => UpdateLineCol();

		private void TxtEdit_TextChanged(object sender, EventArgs e)
		{
			_vm.IsDirty = true;
			UpdateLineCol();

			if (btnAutoRun.Checked)
			{
				_autoRunTimer.Stop();
				_autoRunTimer.Start();
			}
		}

		private void TxtEdit_MouseClick(object sender, MouseEventArgs e)
		{
			UpdateLineCol();
		}

		private void BtnRun_Click(object sender, EventArgs e) => RunScript();

		private void BtnClear_Click(object sender, EventArgs e) => ClearAll();

		private void BtnSave_Click(object sender, EventArgs e) => SaveScript();

		private void BtnAutoRun_CheckedChanged(object sender, EventArgs e)
		{
			if (btnAutoRun.Checked)
				_autoRunTimer.Start();
			else
				_autoRunTimer.Stop();
		}

		private void BtnLoad_Click(object sender, EventArgs e) => LoadScript();

		private void LoadScript()
		{
			using (var dialog = new OpenFileDialog
			{
				Title = "Открытие файла кода",
				Filter = "DisplayNodes Playground (*.dnp)|*.dnp|Все файлы (*.*)|*.*",
				DefaultExt = "dnp",
				CheckFileExists = true,
				Multiselect = false
			})
			{
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					string text = _vm.Load(dialog.FileName);
					if (text == null)
					{
						_ = MessageBox.Show(
							"Не удалось загрузить файл",
							"Ошибка загрузки",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return;
					}
					codeEditor.Text = text;
					codeEditor.ResetUndo();
				}
			}
		}

		private void BtnExit_Click(object sender, EventArgs e) => Close();

		private void LstErrors_DoubleClick(object sender, EventArgs e)
		{
			if (lstErrors.Items.Count == 0 || lstErrors.SelectedIndex < 0)
				return;

			string item = lstErrors.Items[lstErrors.SelectedIndex] as string;
			if (string.IsNullOrEmpty(item))
				return;

			if (!TryParseErrorLine(item, out int userLine))
				return;

			lstErrors.ClearSelected();
			GoToLine(userLine);
		}

		/// <summary>
		/// Извлекает номер строки из строки вида "Line 12, Col 5: ...".
		/// Возвращает false, если формат не распознан.
		/// </summary>
		private static bool TryParseErrorLine(string errorText, out int line)
		{
			line = 0;
			if (string.IsNullOrEmpty(errorText))
				return false;

			// Ищем "Line" — регистронезависимо, но с учётом формата из ScriptRunner
			const string prefix = "Line ";
			int idx = errorText.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
			if (idx < 0)
				return false;

			int start = idx + prefix.Length;
			int end = start;
			while (end < errorText.Length && char.IsDigit(errorText[end]))
				end++;

			if (end == start)
				return false;

			return int.TryParse(errorText.Substring(start, end - start), out line) && line > 0;
		}

		/// <summary>
		/// Перевести каретку редактора на указанную (1-based) строку, выделить её
		/// и проскроллить в видимую область. Заодно перевести фокус в редактор.
		/// </summary>
		private void GoToLine(int line1Based)
		{
			int line = line1Based - 1;
			int lineCount = codeEditor.Editor.Lines.Length;
			if (line < 0)
				line = 0;
			if (line >= lineCount)
				line = lineCount - 1;

			int start = codeEditor.Editor.GetFirstCharIndexFromLine(line);
			if (start < 0)
				return;

			int nextStart = codeEditor.Editor.GetFirstCharIndexFromLine(line + 1);
			int end = (nextStart >= 0) ? nextStart : codeEditor.Editor.TextLength;

			// Выделяем строку вместе с переводом строки — как при клике по жёлобу.
			codeEditor.Editor.SelectionStart = start;
			codeEditor.Editor.SelectionLength = end - start;
			codeEditor.Editor.ScrollToCaret();
			codeEditor.Editor.Focus();
			UpdateLineCol();
		}

		private void UpdateCompilerModeLabel()
		{
			if (_vm.Runner.UsesExternalCompiler)
			{
				lblCompilerMode.Text = "C# 4.0+";
				lblCompilerMode.ForeColor = System.Drawing.Color.FromArgb(100, 220, 100);
				lblCompilerMode.ToolTipText =
					"Скрипты компилируются внешним csc.exe.\r\n" +
					"Доступны именованные и опциональные аргументы " +
					"(например, UI.Row(spacing: 0)).";
			}
			else
			{
				lblCompilerMode.Text = "C# 3.0";
				lblCompilerMode.ForeColor = System.Drawing.Color.FromArgb(200, 180, 100);
				lblCompilerMode.ToolTipText =
					"csc.exe 4.0+ не найден, используется встроенный компилятор C# 3.0.\r\n" +
					"Именованные и опциональные аргументы не поддерживаются.\r\n" +
					"Пишите UI.Row(0) вместо UI.Row(spacing: 0).";
			}
			statusStrip.PerformLayout();
		}

		private void BtnRecent_DropDownOpening(object sender, EventArgs e)
		{
			// Пересобрать список при каждом открытии меню —
			// так мы автоматически отражаем и удалённые файлы, и обновления.
			BtnRecent.DropDownItems.Clear();

			IList<string> items = _vm.RecentFiles.Items;

			if (items.Count == 0)
			{
				var empty = new ToolStripMenuItem("(нет файлов)")
				{
					Enabled = false
				};
				BtnRecent.DropDownItems.Add(empty);
				return;
			}

			for (int i = 0; i < items.Count; i++)
			{
				string path = items[i];

				// Нумерация 1..9 для быстрого доступа. 10-й без цифры.
				string label = (i < 9 ? "&" + (i + 1) + " " : "&0 ") + Path.GetFileName(path);

				var item = new ToolStripMenuItem(label)
				{
					ToolTipText = path,
					Tag = path
				};
				item.Click += RecentItem_Click;

				BtnRecent.DropDownItems.Add(item);
			}

			BtnRecent.DropDownItems.Add(new ToolStripSeparator());

			var clear = new ToolStripMenuItem("Очистить список");
			clear.Click += (s, ea) => _vm.RecentFiles.Clear();
			BtnRecent.DropDownItems.Add(clear);
		}

		private void RecentItem_Click(object sender, EventArgs e)
		{
			if (!(sender is ToolStripMenuItem item))
				return;

			string path = item.Tag as string;
			if (string.IsNullOrEmpty(path))
				return;

			// Файл мог быть удалён/перемещён с момента последнего открытия.
			if (!File.Exists(path))
			{
				_ = MessageBox.Show(
					"Файл не найден:\r\n" + path,
					"Recent file",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				_vm.RecentFiles.Remove(path);
				return;
			}

			// Не теряем несохранённые изменения.
			if (_vm.IsDirty && !ConfirmClose())
				return;

			string text = _vm.Load(path);
			if (text == null)
			{
				_ = MessageBox.Show(
					"Не удалось открыть файл.",
					"Open error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			codeEditor.Text = text;
			codeEditor.ResetUndo();
		}

		private void ErrorCopy_Click(object sender, EventArgs e)
		{
			if (lstErrors.SelectedIndex < 0)
				return;

			string item = lstErrors.Items[lstErrors.SelectedIndex] as string;
			if (string.IsNullOrEmpty(item))
				return;

			try
			{
				Clipboard.SetText(item);
			}
			catch
			{
				// Буфер обмена может быть занят другим процессом — игнорируем.
			}
		}

		private void ErrorCopyAll_Click(object sender, EventArgs e)
		{
			if (lstErrors.Items.Count == 0)
				return;

			var sb = new System.Text.StringBuilder();
			foreach (object item in lstErrors.Items)
			{
				string line = item as string;
				if (!string.IsNullOrEmpty(line))
					sb.AppendLine(line);
			}

			try
			{
				Clipboard.SetText(sb.ToString());
			}
			catch (Exception ex) { AppLog.Error("AppForm.ErrorCopyAll_Click", ex); }
		}

		private void ErrorContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			errorCopy.Enabled = lstErrors.SelectedIndex >= 0;
			errorCopyAll.Enabled = lstErrors.Items.Count > 0;
		}

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var dialog = new SettingsDialog(_settings))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _autoRunTimer.Interval = _settings.AutoRunDebounceMs;
                    _layoutDebounceTimer.Interval = _settings.LayoutDebounceMs;
                }
            }
        }

        private void BtnExportPreview_Click(object sender, EventArgs e) => ExportPreview();

        /// <summary>
        /// Сохраняет текущий preview в файл.
        /// </summary>
        /// <remarks>
        /// Сохраняется <b>то, что уже отрисовано</b> в <see cref="picPreview"/>.
        /// Размер изображения равен размеру <see cref="picPreview"/>.
        /// </remarks>
        private void ExportPreview()
        {
            Bitmap bmp = null;

            try
            {
                bmp = _preview.ExportToBitmap();
            }
            catch (Exception ex)
            {
                AppLog.Error("AppForm.ExportPreview", ex);
            }

            if (bmp == null)
            {
                _ = MessageBox.Show(
                    "Нет изображения для экспорта. Сначала выполните скрипт.",
                    "Экспорт preview",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (bmp)
            using (var dialog = new SaveFileDialog
            {
                Title = "Экспорт preview",
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp",
                DefaultExt = "png",
                FileName = "preview",
                AddExtension = true,
                OverwritePrompt = true
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    ImageFormat format = GetImageFormatFromPath(dialog.FileName);
                    bmp.Save(dialog.FileName, format);

                    lblStatus.Text = "Preview сохранён: " + Path.GetFileName(dialog.FileName);
                    lblStatus.ForeColor = SystemColors.ControlText;
                }
                catch (Exception ex)
                {
                    AppLog.Error("AppForm.ExportPreview", ex);
                    _ = MessageBox.Show(
                        "Не удалось сохранить файл:\r\n" + ex.Message,
                        "Ошибка экспорта",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Определяет формат изображения по расширению файла.
        /// </summary>
        private static ImageFormat GetImageFormatFromPath(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            switch (ext)
            {
                case ".png":
                    return ImageFormat.Png;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".bmp":
                    return ImageFormat.Bmp;
                default:
                    return ImageFormat.Png;
            }
        }
    }
}