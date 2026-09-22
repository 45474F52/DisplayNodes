namespace DisplayNodes.Playground
{
	partial class AppForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppForm));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.BtnFile = new System.Windows.Forms.ToolStripSplitButton();
            this.BtnLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.BtnSave = new System.Windows.Forms.ToolStripMenuItem();
            this.BtnRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.BtnExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAutoRun = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLineCol = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblCompilerMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainSplitter = new System.Windows.Forms.SplitContainer();
            this.viewLogsSplitter = new System.Windows.Forms.SplitContainer();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.errorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.errorCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.errorCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            this.lstErrors = new DisplayNodes.Playground.Editor.SafeListBox();
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.mainSplitter.Panel1.SuspendLayout();
            this.mainSplitter.SuspendLayout();
            this.viewLogsSplitter.Panel1.SuspendLayout();
            this.viewLogsSplitter.Panel2.SuspendLayout();
            this.viewLogsSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.errorContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnFile,
            this.toolStripSeparator3,
            this.btnSettings,
            this.toolStripSeparator4,
            this.btnRun,
            this.btnClear,
            this.toolStripSeparator1,
            this.btnAutoRun});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip.Size = new System.Drawing.Size(946, 25);
            this.toolStrip.TabIndex = 0;
            // 
            // BtnFile
            // 
            this.BtnFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnLoad,
            this.BtnSave,
            this.BtnRecent,
            this.toolStripSeparator2,
            this.BtnExit});
            this.BtnFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnFile.Name = "BtnFile";
            this.BtnFile.Size = new System.Drawing.Size(52, 22);
            this.BtnFile.Text = "Файл";
            // 
            // BtnLoad
            // 
            this.BtnLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnLoad.Name = "BtnLoad";
            this.BtnLoad.ShortcutKeyDisplayString = "Ctrl + O";
            this.BtnLoad.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.BtnLoad.Size = new System.Drawing.Size(179, 22);
            this.BtnLoad.Text = "Загрузить";
            this.BtnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.ShortcutKeyDisplayString = "Ctrl + S";
            this.BtnSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.BtnSave.Size = new System.Drawing.Size(179, 22);
            this.BtnSave.Text = "Сохранить";
            this.BtnSave.ToolTipText = "Сохранить код";
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnRecent
            // 
            this.BtnRecent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnRecent.Name = "BtnRecent";
            this.BtnRecent.Size = new System.Drawing.Size(179, 22);
            this.BtnRecent.Text = "Недавние";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(176, 6);
            // 
            // BtnExit
            // 
            this.BtnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.ShortcutKeyDisplayString = "Alt + F4";
            this.BtnExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.BtnExit.Size = new System.Drawing.Size(179, 22);
            this.BtnExit.Text = "Выход";
            this.BtnExit.ToolTipText = "Закрыть приложение";
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(72, 22);
            this.btnRun.Text = "Запуск (F5)";
            this.btnRun.Click += new System.EventHandler(this.BtnRun_Click);
            // 
            // btnClear
            // 
            this.btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(84, 22);
            this.btnClear.Text = "Очистить всё";
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAutoRun
            // 
            this.btnAutoRun.Checked = true;
            this.btnAutoRun.CheckOnClick = true;
            this.btnAutoRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnAutoRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAutoRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAutoRun.Name = "btnAutoRun";
            this.btnAutoRun.Size = new System.Drawing.Size(73, 22);
            this.btnAutoRun.Text = "Автозапуск";
            this.btnAutoRun.CheckedChanged += new System.EventHandler(this.BtnAutoRun_CheckedChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblLineCol,
            this.lblCompilerMode});
            this.statusStrip.Location = new System.Drawing.Point(0, 588);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip.Size = new System.Drawing.Size(946, 24);
            this.statusStrip.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 19);
            this.lblStatus.Text = "Статус";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLineCol
            // 
            this.lblLineCol.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblLineCol.Name = "lblLineCol";
            this.lblLineCol.Size = new System.Drawing.Size(62, 19);
            this.lblLineCol.Text = "Ln 1, Col 1";
            // 
            // lblCompilerMode
            // 
            this.lblCompilerMode.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblCompilerMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblCompilerMode.Name = "lblCompilerMode";
            this.lblCompilerMode.Size = new System.Drawing.Size(98, 19);
            this.lblCompilerMode.Text = "Компилятор: —";
            this.lblCompilerMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mainSplitter
            // 
            this.mainSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitter.Location = new System.Drawing.Point(0, 25);
            this.mainSplitter.Margin = new System.Windows.Forms.Padding(2);
            this.mainSplitter.Name = "mainSplitter";
            // 
            // mainSplitter.Panel1
            // 
            this.mainSplitter.Panel1.Controls.Add(this.viewLogsSplitter);
            this.mainSplitter.Size = new System.Drawing.Size(946, 563);
            this.mainSplitter.SplitterDistance = 448;
            this.mainSplitter.TabIndex = 2;
            // 
            // viewLogsSplitter
            // 
            this.viewLogsSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewLogsSplitter.Location = new System.Drawing.Point(0, 0);
            this.viewLogsSplitter.Margin = new System.Windows.Forms.Padding(2);
            this.viewLogsSplitter.Name = "viewLogsSplitter";
            this.viewLogsSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // viewLogsSplitter.Panel1
            // 
            this.viewLogsSplitter.Panel1.Controls.Add(this.picPreview);
            // 
            // viewLogsSplitter.Panel2
            // 
            this.viewLogsSplitter.Panel2.Controls.Add(this.lstErrors);
            this.viewLogsSplitter.Size = new System.Drawing.Size(448, 563);
            this.viewLogsSplitter.SplitterDistance = 435;
            this.viewLogsSplitter.SplitterWidth = 3;
            this.viewLogsSplitter.TabIndex = 0;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Margin = new System.Windows.Forms.Padding(2);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(448, 435);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // errorContextMenu
            // 
            this.errorContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.errorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.errorCopy,
            this.errorCopyAll});
            this.errorContextMenu.Name = "errorContextMenu";
            this.errorContextMenu.Size = new System.Drawing.Size(235, 48);
            // 
            // errorCopy
            // 
            this.errorCopy.Name = "errorCopy";
            this.errorCopy.ShortcutKeyDisplayString = "Ctrl+C";
            this.errorCopy.Size = new System.Drawing.Size(234, 22);
            this.errorCopy.Text = "Копировать";
            this.errorCopy.Click += new System.EventHandler(this.ErrorCopy_Click);
            // 
            // errorCopyAll
            // 
            this.errorCopyAll.Name = "errorCopyAll";
            this.errorCopyAll.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            this.errorCopyAll.Size = new System.Drawing.Size(234, 22);
            this.errorCopyAll.Text = "Копировать всё";
            this.errorCopyAll.Click += new System.EventHandler(this.ErrorCopyAll_Click);
            // 
            // lstErrors
            // 
            this.lstErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lstErrors.ContextMenuStrip = this.errorContextMenu;
            this.lstErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstErrors.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstErrors.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lstErrors.FormattingEnabled = true;
            this.lstErrors.ItemHeight = 14;
            this.lstErrors.Location = new System.Drawing.Point(0, 0);
            this.lstErrors.Margin = new System.Windows.Forms.Padding(2);
            this.lstErrors.Name = "lstErrors";
            this.lstErrors.Size = new System.Drawing.Size(448, 125);
            this.lstErrors.TabIndex = 0;
            this.lstErrors.DoubleClick += new System.EventHandler(this.LstErrors_DoubleClick);
            // 
            // btnSettings
            // 
            this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnSettings.Image")));
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(71, 22);
            this.btnSettings.Text = "Настройки";
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // AppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 612);
            this.Controls.Add(this.mainSplitter);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AppForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DisplayNodes Playground";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainSplitter.Panel1.ResumeLayout(false);
            this.mainSplitter.ResumeLayout(false);
            this.viewLogsSplitter.Panel1.ResumeLayout(false);
            this.viewLogsSplitter.Panel2.ResumeLayout(false);
            this.viewLogsSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.errorContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.SplitContainer mainSplitter;
		private System.Windows.Forms.SplitContainer viewLogsSplitter;
		private DisplayNodes.Playground.Editor.CodeEditor codeEditor;
		private System.Windows.Forms.PictureBox picPreview;
		private DisplayNodes.Playground.Editor.SafeListBox lstErrors;
		private System.Windows.Forms.ToolStripButton btnRun;
		private System.Windows.Forms.ToolStripButton btnClear;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton btnAutoRun;
		private System.Windows.Forms.ToolStripStatusLabel lblStatus;
		private System.Windows.Forms.ToolStripStatusLabel lblLineCol;
		private System.Windows.Forms.ToolStripStatusLabel lblCompilerMode;
		private System.Windows.Forms.ToolStripSplitButton BtnFile;
		private System.Windows.Forms.ToolStripMenuItem BtnLoad;
		private System.Windows.Forms.ToolStripMenuItem BtnSave;
		private System.Windows.Forms.ToolStripMenuItem BtnRecent;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem BtnExit;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ContextMenuStrip errorContextMenu;
		private System.Windows.Forms.ToolStripMenuItem errorCopy;
		private System.Windows.Forms.ToolStripMenuItem errorCopyAll;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}