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
    internal sealed class CompletionTooltip : IDisposable
    {
        private readonly NoActivateForm _form;
        private readonly Panel _content;

        private Size _lastMeasuredSize = Size.Empty;
        private bool _disposed;

        public CompletionTooltip()
        {
            _form = new NoActivateForm
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.FromArgb(35, 35, 38),
                Padding = new Padding(1),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _content = new Panel
            {
                BackColor = Color.FromArgb(35, 35, 38),
                Padding = new Padding(8),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill
            };

            _form.Controls.Add(_content);
        }

        public Size GetContentSize() => _lastMeasuredSize;

        public Size Measure(CompletionItem item)
        {
            if (item == null)
                return Size.Empty;

            Rebuild(item);
            return _lastMeasuredSize;
        }

        public void Show(CompletionItem item, Point screenLocation)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            Rebuild(item);

            _form.Location = screenLocation;
            _form.Size = _lastMeasuredSize;

            if (!_form.Visible)
                _form.Show();
            else
                _form.Invalidate();
        }

        public void Hide()
        {
            if (_form.Visible)
                _form.Hide();
        }

        private void Rebuild(CompletionItem item)
        {
            _content.SuspendLayout();
            try
            {
                _content.Controls.Clear();

                int maxWidth = 400;
                int y = 0;

                var lblSignature = new Label
                {
                    Text = item.Signature ?? item.Name ?? "",
                    ForeColor = Color.FromArgb(220, 220, 220),
                    Font = new Font("Consolas", 10f, FontStyle.Bold),
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Location = new Point(0, y)
                };
                _content.Controls.Add(lblSignature);
                y += lblSignature.PreferredHeight + 4;

                var lblKind = new Label
                {
                    Text = KindLabel(item.Kind) + "  →  " + (item.ReturnType ?? ""),
                    ForeColor = Color.FromArgb(150, 150, 160),
                    Font = new Font("Consolas", 9f),
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Location = new Point(0, y)
                };
                _content.Controls.Add(lblKind);
                y += lblKind.PreferredHeight + 6;

                if (!string.IsNullOrEmpty(item.Documentation))
                {
                    var lblDoc = new Label
                    {
                        Text = item.Documentation,
                        ForeColor = Color.FromArgb(180, 180, 180),
                        Font = new Font("Segoe UI", 9f),
                        AutoSize = true,
                        MaximumSize = new Size(maxWidth, 0),
                        Location = new Point(0, y)
                    };
                    _content.Controls.Add(lblDoc);
                    y += lblDoc.PreferredHeight;
                }

                int contentWidth = Math.Max(200, _content.PreferredSize.Width + 16);
                int contentHeight = y + 8;
                _content.Size = new Size(contentWidth, contentHeight);

                _lastMeasuredSize = new Size(contentWidth + 2, contentHeight + 2);
            }
            finally
            {
                _content.ResumeLayout();
            }
        }

        private static string KindLabel(CompletionItemKind kind)
        {
            switch (kind)
            {
                case CompletionItemKind.Method: return "метод";
                case CompletionItemKind.Property: return "свойство";
                case CompletionItemKind.Field: return "поле";
                case CompletionItemKind.Type: return "тип";
                default: return "";
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _form?.Dispose();
            _content?.Dispose();
        }

        /// <summary>
        /// Form без активации — показывается, не забирая фокус.
        /// </summary>
        private sealed class NoActivateForm : Form
        {
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                    cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                    return cp;
                }
            }
        }
    }
}