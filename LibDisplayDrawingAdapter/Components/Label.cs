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
using LibDisplayDrawing;
using System;
using System.Drawing;

namespace DisplayNodes.LibDisplayDrawingAdapter.Components
{
	/// <summary>
	/// Адаптер для текстовой метки. Обёртка над <see cref="Label2D"/>.
	/// </summary>
	/// <remarks>
	/// <para>Инкапсулирует странности владения ресурсами в <see cref="Label2D"/>:
	/// легаси-компонент клонирует <see cref="SolidBrush"/> (и владеет клоном),
	/// но НЕ клонирует <see cref="Font"/> и <see cref="StringFormat"/> (и диспоузит их при Dispose).</para>
	/// <para>Адаптер клонирует <see cref="Font"/> и <see cref="StringFormat"/> при установке,
	/// чтобы защитить consumer-код от неожиданного освобождения ресурсов.</para>
	/// </remarks>
	internal sealed class Label : ComponentBase, ILabelComponent, ITextLayoutComponent, IDisposable
	{
		private readonly Label2D _label;

		private Font _ownedFont;
		private StringFormat _ownedFormat;
		private Thickness _padding;

		/// <summary>Создаёт адаптер текстовой метки.</summary>
		public Label() : base(new Label2D(null))
		{
			_label = (Label2D)Inner;
		}

		/// <inheritdoc/>
		public string Text
		{
			get => _label.Text;
			set => _label.Text = value;
		}

		/// <inheritdoc/>
		public IFont Font
		{
			get => new GdiFont(_label.Font);
			set
			{
				_ownedFont?.Dispose();
				var gdi = value.ToGdi();
				_ownedFont = gdi != null ? (Font)gdi.Clone() : null;
				_label.Font = _ownedFont;
			}
		}

		/// <inheritdoc/>
		public IBrush ForegroundBrush
		{
			get => new GdiBrush(_label.Brush);
			set => _label.Brush = value.ToGdi();
		}

		/// <inheritdoc/>
		public IBrush BackgroundBrush
		{
			get => _label.FullBrush != null ? new GdiBrush(_label.FullBrush) : null;
			set => _label.FullBrush = value.ToGdi();
		}

		/// <inheritdoc/>
		public ITextFormat Format
		{
			get => new GdiTextFormat(_label.Format);
			set
			{
				_ownedFormat?.Dispose();
				var gdi = value.ToGdi();
				_ownedFormat = gdi != null ? (StringFormat)gdi.Clone() : null;
				_label.Format = _ownedFormat;
			}
		}

		/// <inheritdoc/>
		/// <remarks>Режим mnemonics компонентом <c>Label2D</c> не поддерживается.</remarks>
		/// <exception cref="NotSupportedException">Всегда.</exception>
		public bool UseMnemonic
		{
				get => throw new NotSupportedException("UseMnemonic is not supported by LibDisplayDrawingAdapter.");
				set => throw new NotSupportedException("UseMnemonic is not supported by LibDisplayDrawingAdapter.");
		}

		/// <summary>
		/// Не поддерживается: native-контроль Label2D форматирует текстовую область сам.
		/// </summary>
		public Thickness Padding
		{
				get => throw new NotSupportedException("Padding is not supported by LibDisplayDrawingAdapter.");
				set => throw new NotSupportedException("Padding is not supported by LibDisplayDrawingAdapter.");
		}

		/// <inheritdoc/>
		public TextDrawMethod DrawMethod
		{
			get => (TextDrawMethod)(int)_label.DrawMethod;
			set => _label.DrawMethod = (TextDrawing.DrawMethod)(int)value;
		}

		/// <inheritdoc/>
		public LabelStretch Stretch
		{
			get => (LabelStretch)(int)_label.Stretch;
			set => _label.Stretch = (Label2D.CodeStretch)(int)value;
		}

		/// <inheritdoc/>
		public double Opacity
		{
			get => _label.Opacity;
			set => _label.Opacity = value;
		}

		/// <inheritdoc/>
		public double Brightness
		{
			get => _label.Brightness;
			set => _label.Brightness = value;
		}

		/// <inheritdoc/>
		public double Contrast
		{
			get => _label.Contrast;
			set => _label.Contrast = value;
		}

        private Shadow? _shadow;
        public Shadow? Shadow
        {
            get => _shadow;
            set
            {
                if (value.HasValue)
                    throw new NotSupportedException(
                        "Shadow is not supported by LibDisplayDrawingAdapter yet.");
                _shadow = null;
            }
        }

        /// <summary>
        /// Освобождает ресурсы. <see cref="Label2D.Dispose"/> диспоузит внутренние клоны
        /// шрифта, кистей, формата и битмапа.
        /// </summary>
        public void Dispose()
		{
			_label.Dispose();

			_ownedFont?.Dispose();
			_ownedFormat?.Dispose();

			_ownedFont = null;
			_ownedFormat = null;
		}
	}
}