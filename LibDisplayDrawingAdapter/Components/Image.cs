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
using IImage = DisplayNodes.Core.Rendering.IImage;

namespace DisplayNodes.LibDisplayDrawingAdapter.Components
{
	/// <summary>
	/// Адаптер для изображения. Обёртка над <see cref="Image2D"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="Image2D"/> владеет переданным <see cref="Bitmap"/> (диспоузит при замене и в Dispose).
	/// Адаптер клонирует Bitmap при установке, чтобы защитить consumer-код.
	/// </remarks>
	internal sealed class Image : ComponentBase, IImageComponent, IDisposable
	{
		private readonly Image2D _image;
		private Bitmap _ownedBitmap;

		/// <summary>Создаёт адаптер изображения.</summary>
		public Image() : base(new Image2D(null))
		{
			_image = (Image2D)Inner;
		}

		/// <summary>Отображаемое изображение</summary>
		/// <remarks>
		/// При установке изображение клонируется — <see cref="Image2D"/> диспоузит переданный объект.
		/// Consumer-код сохраняет свой оригинал.
		/// </remarks>
		public IImage ImageData
		{
			get => _image.Bitmap != null ? new GdiImage(_image.Bitmap) : null;
			set
			{
				_ownedBitmap?.Dispose();
				var gdi = value.ToGdi();
				_ownedBitmap = gdi != null ? (Bitmap)gdi.Clone() : null;
				_image.Bitmap = _ownedBitmap;
			}
		}

		IImage IImageComponent.Image
		{
			get => ImageData;
			set => ImageData = value;
		}

		/// <inheritdoc/>
		public ImageSizeMode SizeMode
		{
			get => (ImageSizeMode)(int)_image.SizeMode;
			set => _image.SizeMode = (Image2D.CodeSizeMode)(int)value;
		}

		/// <inheritdoc/>
		public double Opacity
		{
			get => _image.Opacity;
			set => _image.Opacity = value;
		}

		/// <inheritdoc/>
		public double Brightness
		{
			get => _image.Brightness;
			set => _image.Brightness = value;
		}

		/// <inheritdoc/>
		public double Contrast
		{
			get => _image.Contrast;
			set => _image.Contrast = value;
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

        /// <summary>Освобождает ресурсы. <see cref="Image2D.Dispose"/> диспоузит внутренние клоны битмапов.</summary>
        public void Dispose()
		{
			_image.Dispose();

			_ownedBitmap?.Dispose();
			_ownedBitmap = null;
		}
	}
}