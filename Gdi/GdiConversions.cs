using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// Методы конвертации между абстрактными типами Core и GDI-типами.
	/// </summary>
	public static class GdiConversions
	{
		/// <summary>Извлекает GDI Font из IFont.</summary>
		public static Font ToGdi(this IFont font) => (font as GdiFont)?.Inner;

		/// <summary>Оборачивает <see cref="System.Drawing.Font"/></summary>
		public static IFont Wrap(this Font font) => new GdiFont(font);

		/// <summary>Извлекает GDI SolidBrush из IBrush.</summary>
		public static SolidBrush ToGdi(this IBrush brush) => (brush as GdiBrush)?.Inner;

        /// <summary>Оборачивает <see cref="System.Drawing.SolidBrush"/></summary>
        public static IBrush Wrap(this SolidBrush brush) => new GdiBrush(brush);

        /// <summary>Извлекает GDI Bitmap из IImage.</summary>
        public static Bitmap ToGdi(this IImage image) => (image as GdiImage)?.Inner;

        /// <summary>Оборачивает <see cref="System.Drawing.Bitmap"/></summary>
        public static IImage Wrap(this Bitmap image) => new GdiImage(image);

        /// <summary>Извлекает GDI StringFormat из ITextFormat.</summary>
        public static StringFormat ToGdi(this ITextFormat format) => (format as GdiTextFormat)?.Inner;

        /// <summary>Оборачивает <see cref="System.Drawing.StringFormat"/></summary>
        public static ITextFormat Wrap(this StringFormat format) => new GdiTextFormat(format);

        /// <summary>Извлекает GDI GraphicsPath из IGraphicsPath.</summary>
        public static GraphicsPath ToGdi(this IGraphicsPath path) => (path as GdiGraphicsPath)?.Inner;

        /// <summary>Оборачивает <see cref="System.Drawing.Drawing2D.GraphicsPath"/></summary>
        public static IGraphicsPath Wrap(this GraphicsPath path) => new GdiGraphicsPath(path);

        /// <summary>Конвертирует DisplayNodes.Core.Color в System.Drawing.Color.</summary>
        public static System.Drawing.Color ToGdi(this DisplayNodes.Core.Color c)
			=> System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

		/// <summary>Конвертирует System.Drawing.Color в DisplayNodes.Core.Color.</summary>
		public static DisplayNodes.Core.Color FromGdi(this System.Drawing.Color c)
			=> new DisplayNodes.Core.Color(c.R, c.G, c.B, c.A);

		/// <summary>Конвертирует Rect в RectangleF.</summary>
		public static RectangleF ToGdi(this Rect r)
			=> new RectangleF(r.X, r.Y, r.Width, r.Height);

		/// <summary>Конвертирует RectangleF в Rect.</summary>
		/// <exception cref="OverflowException"></exception>
		public static Rect FromGdi(this RectangleF r)
			=> new Rect(Convert.ToInt32(r.X), Convert.ToInt32(r.Y), Convert.ToInt32(r.Width), Convert.ToInt32(r.Height));
	}
}