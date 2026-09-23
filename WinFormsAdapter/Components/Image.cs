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
	internal sealed class Image : ComponentBase, IImageComponent, IEffectComponent, IDisposable
	{
		private readonly PictureBox _pictureBox;

		private Bitmap _ownedBitmap;
		private GdiImage _cachedImage;

		public Image() : base(new PictureBox())
		{
			_pictureBox = (PictureBox)Inner;

			_pictureBox.BackColor = Color.Transparent;
		}

		IImage IImageComponent.Image
        {
            get
            {
                if (_cachedImage == null && _ownedBitmap != null)
                    _cachedImage = new GdiImage(_ownedBitmap);
                return _cachedImage;
            }
            set
            {
                _ownedBitmap?.Dispose();
                _cachedImage = null;

                var gdi = value.ToGdi();
                _ownedBitmap = gdi != null ? (Bitmap)gdi.Clone() : null;
                _pictureBox.Image = _ownedBitmap;
            }
        }

		public ImageSizeMode SizeMode
		{
			get
			{
				switch (_pictureBox.SizeMode)
				{
					case PictureBoxSizeMode.StretchImage:
					return ImageSizeMode.Stretch;
					default:
					return ImageSizeMode.Normal;
				}
			}
			set
			{
				switch (value)
				{
					case ImageSizeMode.Normal:
					_pictureBox.SizeMode = PictureBoxSizeMode.Normal;
					break;
					case ImageSizeMode.Stretch:
					_pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
					break;
					default:
					throw new ArgumentException(nameof(value));
				}
			}
		}

        public double Opacity
        {
            get => _pictureBox.BackColor.A / 255.0 * 100;
            set
            {
                int alpha = (int)(value / 100.0 * 255);
                alpha = Math.Max(0, Math.Min(255, alpha));
                _pictureBox.BackColor = Color.FromArgb(alpha, _pictureBox.BackColor);
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
            _ownedBitmap?.Dispose();
            _ownedBitmap = null;
            _cachedImage = null;
            _pictureBox.Dispose();
        }
    }
}
