using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	/// <summary>
	/// Всплывающий список автодополнения. Показывается под кареткой,
	/// фильтруется по вводимым символам, вставляет выбранный вариант
	/// по Tab/Enter.
	/// </summary>
	internal sealed class CompletionPanel : Panel
	{
		private readonly ListBox _list;
		private List<string> _all = new List<string>();
		private string _prefix = "";

		public event Action<string> ItemAccepted;

		public CompletionPanel()
		{
			BackColor = Color.FromArgb(45, 45, 48);
			Padding = new Padding(1);
			BorderStyle = BorderStyle.FixedSingle;
			Size = new Size(220, 140);
			Visible = false;

			_list = new ListBox
			{
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None,
				BackColor = Color.FromArgb(45, 45, 48),
				ForeColor = Color.Gainsboro,
				Font = new Font("Consolas", 10f),
				IntegralHeight = false,
				DrawMode = DrawMode.OwnerDrawFixed,
				ItemHeight = 18
			};
			_list.DrawItem += List_DrawItem;
			_list.DoubleClick += (s, e) => Accept();
			Controls.Add(_list);
		}

		/// <summary>
		/// Открыть панель с полным списком и пустым фильтром.
		/// </summary>
		public void Open(List<string> items)
		{
			_all = items ?? new List<string>();
			_prefix = "";
			RebuildList();
			_list.SelectedIndex = _list.Items.Count > 0 ? 0 : -1;
			Visible = true;
			BringToFront();
		}

		public void ClosePanel()
		{
			Visible = false;
		}

		/// <summary>
		/// Добавить символ к фильтру. Возвращает false, если после фильтрации
		/// список пуст — вызывающий код должен закрыть панель.
		/// </summary>
		public bool TypeChar(char c)
		{
			if (!Visible)
				return false;
			_prefix += c;
			RebuildList();

			if (_list.Items.Count == 0)
				return false;

			_list.SelectedIndex = 0;
			return true;
		}

		/// <summary>Убрать последний символ из фильтра (Backspace).</summary>
		public bool BackspaceChar()
		{
			if (!Visible)
				return false;
			if (_prefix.Length == 0)
				return false;

			_prefix = _prefix.Substring(0, _prefix.Length - 1);
			RebuildList();

			if (_list.Items.Count == 0)
				return false;

			_list.SelectedIndex = 0;
			return true;
		}

		public void MoveSelection(int delta)
		{
			if (_list.Items.Count == 0)
				return;
			int idx = _list.SelectedIndex + delta;
			if (idx < 0)
				idx = _list.Items.Count - 1;
			if (idx >= _list.Items.Count)
				idx = 0;
			_list.SelectedIndex = idx;
		}

		public void Accept()
		{
			if (!Visible)
				return;
			if (_list.SelectedIndex < 0)
				return;

			string value = _list.Items[_list.SelectedIndex] as string;
			if (string.IsNullOrEmpty(value))
				return;

			var handler = ItemAccepted;
			ClosePanel();
			if (handler != null)
				handler(value);
		}

		private void RebuildList()
		{
			_list.BeginUpdate();
			_list.Items.Clear();

			for (int i = 0; i < _all.Count; i++)
			{
				string item = _all[i];
				if (_prefix.Length == 0
					|| item.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
				{
					_list.Items.Add(item);
				}
			}

			_list.EndUpdate();
		}

		private void List_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0)
				return;

			bool selected = (e.State & DrawItemState.Selected) != 0;
			Color back = selected ? Color.FromArgb(70, 80, 96) : _list.BackColor;

			using (var b = new SolidBrush(back))
				e.Graphics.FillRectangle(b, e.Bounds);

			string text = _list.Items[e.Index].ToString();
			TextRenderer.DrawText(e.Graphics, text, _list.Font, e.Bounds,
				Color.Gainsboro,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}
}
