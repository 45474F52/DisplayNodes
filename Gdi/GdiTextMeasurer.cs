using System;
using System.Drawing;

using DisplayNodes.Core.Rendering;

using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="ITextMeasurer"/>.
	/// Использует кэшированные Bitmap/Graphics для производительности.
	/// </summary>
	public sealed class GdiTextMeasurer : ITextMeasurer
	{
		private const float WIDTH_OFFSET = 5f;
		private const float HEIGHT_OFFSET = 1f;

		[ThreadStatic] private static Bitmap _bmp;
		[ThreadStatic] private static Graphics _g;

		private static Graphics GetGraphics()
		{
			if (_g == null)
			{
				_bmp = new Bitmap(1, 1);
				_g = Graphics.FromImage(_bmp);
			}
			return _g;
		}

		/// <inheritdoc/>
		public Size MeasureArea(string text, IFont font, int maxWidth = 0)
		{
			if (string.IsNullOrEmpty(text))
				return Size.Empty;

			var gdiFont = (font as GdiFont)?.Inner;
			if (gdiFont == null)
				return Size.Empty;

			var g = GetGraphics();
			using (var sf = new StringFormat())
			{
				var size = maxWidth > 0
					? g.MeasureString(text, gdiFont, maxWidth, sf)
					: g.MeasureString(text, gdiFont, int.MaxValue, sf);

				return new Size(
					(int)Math.Ceiling(size.Width + WIDTH_OFFSET),
					(int)Math.Ceiling(size.Height + HEIGHT_OFFSET));
			}
		}
	}
}