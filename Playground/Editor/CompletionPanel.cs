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
using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
    /// <summary>
    /// Всплывающий список автодополнения с tooltip'ом сигнатуры.
    /// </summary>
    internal sealed class CompletionPanel : Panel
    {
        private readonly ListBox _list;
        private readonly CompletionTooltip _tooltip;
        private List<CompletionItem> _all = new List<CompletionItem>();
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
            _list.SelectedIndexChanged += (s, e) => UpdateTooltip();
            Controls.Add(_list);

            _tooltip = new CompletionTooltip();
        }

        /// <summary>
        /// Открыть панель с полным списком и пустым фильтром.
        /// </summary>
        public void Open(List<CompletionItem> items)
        {
            _all = items ?? new List<CompletionItem>();
            _prefix = "";
            RebuildList();
            _list.SelectedIndex = _list.Items.Count > 0 ? 0 : -1;
            Visible = true;
            BringToFront();
            UpdateTooltip();
        }

        public void ClosePanel()
        {
            Visible = false;
            _tooltip.Hide();
        }

        /// <summary>
        /// Добавить символ к фильтру.
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
            UpdateTooltip();
            return true;
        }

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
            UpdateTooltip();
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
            UpdateTooltip();
        }

        public void Accept()
        {
            if (!Visible)
                return;
            if (_list.SelectedIndex < 0)
                return;

            CompletionItem item = _list.Items[_list.SelectedIndex] as CompletionItem;
            if (item == null || string.IsNullOrEmpty(item.Name))
                return;

            var handler = ItemAccepted;
            ClosePanel();
            if (handler != null)
                handler(item.Name);
        }

        private void RebuildList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();

            for (int i = 0; i < _all.Count; i++)
            {
                CompletionItem item = _all[i];
                if (_prefix.Length == 0
                    || item.Name.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
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

            CompletionItem item = _list.Items[e.Index] as CompletionItem;
            string text = item?.Name ?? "";
            TextRenderer.DrawText(e.Graphics, text, _list.Font, e.Bounds,
                Color.Gainsboro,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private void UpdateTooltip()
        {
            if (!Visible || _list.SelectedIndex < 0)
            {
                _tooltip.Hide();
                return;
            }

            CompletionItem item = _list.Items[_list.SelectedIndex] as CompletionItem;
            if (item == null || string.IsNullOrEmpty(item.Signature))
            {
                _tooltip.Hide();
                return;
            }

            // 1. Размер tooltip — измеряем заранее.
            Size tooltipSize = _tooltip.Measure(item);
            if (tooltipSize.Width <= 0 || tooltipSize.Height <= 0)
            {
                // Не смогли построить контент — показываем как есть.
                _tooltip.Show(item, PointToScreen(new Point(Width, 0)));
                return;
            }

            // 2. Точки привязки в экранных координатах.
            Rectangle itemRect = _list.GetItemRectangle(_list.SelectedIndex);
            Point itemAnchorInPanel = new Point(_list.Left, _list.Top + itemRect.Y);
            Point itemScreenAnchor = PointToScreen(itemAnchorInPanel);

            Point panelTopRight = PointToScreen(new Point(Width, 0));
            Point panelTopLeft = PointToScreen(new Point(0, 0));

            // 3. Рабочая область монитора.
            Screen screen = Screen.FromPoint(panelTopRight);
            Rectangle workArea = screen.WorkingArea;

            const int gap = 4;

            // 4. Справа от панели по умолчанию.
            int x = panelTopRight.X + gap;
            int y = itemScreenAnchor.Y;

            // 5. Если не влезает справа — слева от панели.
            if (x + tooltipSize.Width > workArea.Right)
                x = panelTopLeft.X - tooltipSize.Width - gap;

            // 6. Если всё ещё не влезает — прижимаем к правому краю.
            if (x + tooltipSize.Width > workArea.Right)
                x = workArea.Right - tooltipSize.Width;

            // 7. Если уходит влево — прижимаем к левому краю.
            if (x < workArea.Left)
                x = workArea.Left;

            // 8. По вертикали: если не влезает снизу — поднимаем.
            if (y + tooltipSize.Height > workArea.Bottom)
                y = workArea.Bottom - tooltipSize.Height;

            // 9. Если уходит вверх — прижимаем к верхнему краю.
            if (y < workArea.Top)
                y = workArea.Top;

            _tooltip.Show(item, new Point(x, y));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}