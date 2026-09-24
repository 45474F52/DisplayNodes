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

        // Полное двумерное выравнивание, заданное через Format.
        // WinForms Label.TextAlign хранит обе оси (9 значений ContentAlignment), но на
        // FlatStyle.System (нативный CONTROLTYPE_STATIC) ОС-рендерер поддерживает только
        // верхний ряд (TopLeft/TopCenter/TopRight): текст с MiddleCenter рисуется по центру
        // горизонтали, но прижимается к верху. Поэтому вертикальная составляющая учитывается
        // здесь, а фактическое позиционирование выполняет OwnerDraw-рендер (OnPaint),
        // использующий GDI+ TextRenderer.DrawText со StringFormat.Alignment + LineAlignment.
        private ContentAlignment _align = ContentAlignment.TopLeft;
        // true => Label.UseMnemonic (эмуляция FlatStyle.System: только верхний ряд),
        // false => полный двумерный рендер по _align.
        private bool _useMnemonic = true;

        public Label() : base(new System.Windows.Forms.Label())
        {
            _label = (System.Windows.Forms.Label)Inner;

            _label.BackColor = Color.Transparent;
            // Только UserPaint даёт нам полный контроль над позиционированием текста
            // (включая вертикальный центринг); нативная отрисовка системного Label его не поддерживает.
            _label.FlatStyle = FlatStyle.Standard;
            _label.AutoSize = false;
            _label.Paint += OnPaint;
        }

        // Собственная отрисовка текста: фон заливается в Rectangle (как делал системный рендерер),
        // текст рисуется через TextRenderer (ClearType/GDI, тот же рендер, что и у системного Label)
        // с точным двумерным выравниванием.
        private void OnPaint(object sender, PaintEventArgs e)
        {
            var bounds = _label.ClientRectangle;

            using (var bg = new SolidBrush(_label.BackColor))
                e.Graphics.FillRectangle(bg, bounds);

            if (_label.Text.Length == 0)
                return;

            // FlatStyle.System физически не может отцентрировать текст по вертикали,
            // поэтому эмулируем его «верхний» режим: Near -> Top, Center -> TopCenter, Far -> TopRight.
            ContentAlignment align = _useMnemonic ? EmulateSystemAlign(_align) : _align;

            TextRenderer.DrawText(
                e.Graphics,
                _label.Text,
                _label.Font,
                bounds,
                _label.ForeColor,
                Color.Empty,
                ToTextFormatFlags(align),
                _useMnemonic);
        }

        private static ContentAlignment EmulateSystemAlign(ContentAlignment align)
        {
            switch (ToAlignment(align))
            {
                case StringAlignment.Center: return ContentAlignment.TopCenter;
                case StringAlignment.Far: return ContentAlignment.TopRight;
                default: return ContentAlignment.TopLeft;
            }
        }

        private static TextFormatFlags ToTextFormatFlags(ContentAlignment align)
        {
            // HorizontalCenter/VerticalCenter соответствуют StringAlignment.Center / LineAlignment.Center.
            TextFormatFlags flags = TextFormatFlags.Default;

            switch (ToAlignment(align))
            {
                case StringAlignment.Center: flags |= TextFormatFlags.HorizontalCenter; break;
                case StringAlignment.Far: flags |= TextFormatFlags.Right; break;
            }

            switch (ToLineAlignment(align))
            {
                case StringAlignment.Center: flags |= TextFormatFlags.VerticalCenter; break;
                case StringAlignment.Far: flags |= TextFormatFlags.Bottom; break;
            }

            return flags;
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
                // Сохраняем ОБА измерения: Alignment — горизонталь, LineAlignment — вертикаль.
                // Источник истины — поле _align (двумерное), а не Label.TextAlign,
                // который при OwnerDraw-рендеринге не используется для позиционирования.
                if (_ownedFormat == null
                    || _ownedFormat.Alignment != ToAlignment(_align)
                    || _ownedFormat.LineAlignment != ToLineAlignment(_align))
                {
                    _ownedFormat?.Dispose();
                    _ownedFormat = new StringFormat
                    {
                        Alignment = ToAlignment(_align),
                        LineAlignment = ToLineAlignment(_align)
                    };
                }
                return _ownedFormat.Wrap();
            }
            set
            {
                var gdi = value.ToGdi();
                if (gdi == null) return;
                // Восстанавливаем полный ContentAlignment из пары (Alignment, LineAlignment).
                _align = ToContentAlignment(gdi.Alignment, gdi.LineAlignment);
                // Формат с ненулевым вертикальным смещением (Center/Far в LineAlignment)
                // невозможно выразить системным (нативным) рендером — переключаемся на
                // полноценный OwnerDraw-рендер. Near (верх) остаётся совместим с FlatStyle.System.
                if (gdi.LineAlignment != StringAlignment.Near)
                    _useMnemonic = false;
                _label.Invalidate();
            }
        }

        /// <summary>
        /// Управление mnemonics ('&amp;') и режимом рендера:
        /// true — эмуляция прежнего FlatStyle.System (текст всегда в верхнем ряду, '&amp;' как мнемоника),
        /// false — точное двумерное выравнивание текста по формату (устанавливается автоматически
        /// при задании вертикального выравнивания, отличного от «верх»).
        /// </summary>
        public bool UseMnemonic
        {
            get => _useMnemonic;
            set
            {
                _useMnemonic = value;
                _label.UseMnemonic = value;
                _label.Invalidate();
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

        // ContentAlignment (9 значений) <-> пара StringAlignment (горизонталь + вертикаль).
        // StringFormat в GDI+ имеет два независимых поля: Alignment (горизонтальное) и
        // LineAlignment (вертикальное). Терялось второе измерение — текст всегда прижимался
        // к верху. Ниже — биективное отображение между всеми 9 значениями ContentAlignment
        // и парами (Alignment, LineAlignment).

        /// <summary>Горизонтальная составляющая <see cref="ContentAlignment"/>.</summary>
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

        /// <summary>Вертикальная составляющая <see cref="ContentAlignment"/>.</summary>
        private static StringAlignment ToLineAlignment(ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    return StringAlignment.Near;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    return StringAlignment.Center;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Near;
            }
        }

        /// <summary>
        /// Собирает полный <see cref="ContentAlignment"/> из пары выравниваний
        /// <see cref="StringFormat"/> (горизонталь + вертикаль) без потери информации.
        /// </summary>
        private static ContentAlignment ToContentAlignment(
            StringAlignment horizontal, StringAlignment vertical)
        {
            switch (vertical)
            {
                case StringAlignment.Center when horizontal == StringAlignment.Near:
                    return ContentAlignment.MiddleLeft;
                case StringAlignment.Center when horizontal == StringAlignment.Center:
                    return ContentAlignment.MiddleCenter;
                case StringAlignment.Center when horizontal == StringAlignment.Far:
                    return ContentAlignment.MiddleRight;

                case StringAlignment.Far when horizontal == StringAlignment.Near:
                    return ContentAlignment.BottomLeft;
                case StringAlignment.Far when horizontal == StringAlignment.Center:
                    return ContentAlignment.BottomCenter;
                case StringAlignment.Far when horizontal == StringAlignment.Far:
                    return ContentAlignment.BottomRight;

                default: // StringAlignment.Near
                    switch (horizontal)
                    {
                        case StringAlignment.Center: return ContentAlignment.TopCenter;
                        case StringAlignment.Far: return ContentAlignment.TopRight;
                        default: return ContentAlignment.TopLeft;
                    }
            }
        }
    }
}