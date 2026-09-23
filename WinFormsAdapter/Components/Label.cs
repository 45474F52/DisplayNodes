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
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using System;
using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace DisplayNodes.WinFormsAdapter.Components
{
    internal sealed class Label : ComponentBase, ILabelComponent, IEffectComponent, IDisposable
    {
        private readonly System.Windows.Forms.Label _label;

        // Владеет клонами Font и StringFormat (WinForms не диспоузит их сам).
        private Font _ownedFont;
        private StringFormat _ownedFormat;

        // Кэши для чтения — инвалидируются при изменении источника.
        private SolidBrush _cachedForeground;
        private SolidBrush _cachedBackground;
        private Color _cachedForeColor;
        private Color _cachedBackColor;
        private ContentAlignment _cachedAlign;

        public Label() : base(new System.Windows.Forms.Label())
        {
            _label = (System.Windows.Forms.Label)Inner;

            _label.BackColor = Color.Transparent;
            _label.FlatStyle = FlatStyle.System;
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public IFont Font
        {
            get => (_ownedFont ?? _label.Font).Wrap();
            set
            {
                _ownedFont?.Dispose();
                var gdi = value.ToGdi();
                _ownedFont = gdi != null ? (Font)gdi.Clone() : null;
                _label.Font = _ownedFont ?? _label.Font;
            }
        }

        public IBrush ForegroundBrush
        {
            get
            {
                if (_cachedForeground == null || _cachedForeColor != _label.ForeColor)
                {
                    _cachedForeground?.Dispose();
                    _cachedForeColor = _label.ForeColor;
                    _cachedForeground = new SolidBrush(_cachedForeColor);
                }
                return _cachedForeground.Wrap();
            }
            set
            {
                var gdi = value.ToGdi();
                _label.ForeColor = gdi?.Color ?? Color.Empty;
                _cachedForeColor = default;
            }
        }

        public IBrush BackgroundBrush
        {
            get
            {
                if (_cachedBackground == null || _cachedBackColor != _label.BackColor)
                {
                    _cachedBackground?.Dispose();
                    _cachedBackColor = _label.BackColor;
                    _cachedBackground = new SolidBrush(_cachedBackColor);
                }
                return _cachedBackground.Wrap();
            }
            set
            {
                var gdi = value.ToGdi();
                _label.BackColor = gdi?.Color ?? Color.Empty;
                _cachedBackColor = default;
            }
        }

        public ITextFormat Format
        {
            get
            {
                if (_ownedFormat == null || _cachedAlign != _label.TextAlign)
                {
                    _ownedFormat?.Dispose();
                    _cachedAlign = _label.TextAlign;
                    _ownedFormat = new StringFormat { Alignment = ToAlignment(_label.TextAlign) };
                }
                return _ownedFormat.Wrap();
            }
            set
            {
                var gdi = value.ToGdi();
                if (gdi == null) return;
                _label.TextAlign = ToContentAlignment(gdi.Alignment);
                _cachedAlign = default;
            }
        }

        [Obsolete("Не поддерживается WinForms")]
        public TextDrawMethod DrawMethod { get; set; }

        [Obsolete("Не поддерживается WinForms")]
        public LabelStretch Stretch { get; set; }

        public double Opacity
        {
            get => _label.BackColor.A / 255.0 * 100;
            set
            {
                int alpha = (int)(value / 100.0 * 255);
                alpha = Math.Max(0, Math.Min(255, alpha));
                var c = _label.BackColor;
                _label.BackColor = Color.FromArgb(alpha, c.R, c.G, c.B);
                _cachedBackColor = default;
            }
        }

        [Obsolete("Не поддерживается WinForms")]
        public double Brightness { get; set; }

        [Obsolete("Не поддерживается WinForms")]
        public double Contrast { get; set; }

        private Shadow? _shadow;
        public Shadow? Shadow
        {
            get => _shadow;
            set
            {
                if (value.HasValue)
                    throw new NotSupportedException(
                        "Shadow is not supported by WinFormsAdapter yet.");
                _shadow = null;
            }
        }

        public void Dispose()
        {
            _ownedFont?.Dispose();
            _ownedFormat?.Dispose();
            _cachedForeground?.Dispose();
            _cachedBackground?.Dispose();
            _label.Dispose();
        }

        private static StringAlignment ToAlignment(ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return StringAlignment.Near;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    return StringAlignment.Center;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Near;
            }
        }

        private static ContentAlignment ToContentAlignment(StringAlignment align)
        {
            switch (align)
            {
                case StringAlignment.Near: return ContentAlignment.TopLeft;
                case StringAlignment.Center: return ContentAlignment.TopCenter;
                case StringAlignment.Far: return ContentAlignment.TopRight;
                default: return ContentAlignment.TopLeft;
            }
        }
    }
}