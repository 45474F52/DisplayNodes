using System;
using System.Drawing;
using System.Windows.Forms;

using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;

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

        public void Dispose()
        {
            _ownedBitmap?.Dispose();
            _ownedBitmap = null;
            _cachedImage = null;
            _pictureBox.Dispose();
        }
    }
}
