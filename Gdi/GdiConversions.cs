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
using System.Drawing.Drawing2D;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// Методы конвертации между абстрактными типами Core и GDI-типами.
	/// </summary>
	public static class GdiConversions
	{
		/// <summary>Извлекает GDI Font из IFont.</summary>
		public static Font ToGdi(this IFont font) => (font as GdiFont)?.Inner;

        /// <summary>Оборачивает <see cref="Font"/></summary>
        public static IFont Wrap(this Font font) => new GdiFont(font);

        /// <summary>
        /// Извлекает GDI <see cref="SolidBrush"/> из <see cref="IBrush"/>.
        /// </summary>
        /// <remarks>
        /// Градиентные кисти (<see cref="GdiLinearGradientBrush"/>, <see cref="GdiRadialGradientBrush"/>)
        /// описаны в API, но адаптеры их пока не поддерживают.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// Если <paramref name="brush"/> не является <see cref="GdiBrush"/> —
        /// градиентные и другие составные кисти пока не поддерживаются адаптерами.
        /// </exception>
        public static SolidBrush ToGdi(this IBrush brush)
        {
            if (brush == null)
                return null;

            if (brush is GdiBrush solid)
                return solid.Inner;

            throw new NotSupportedException(
                "Brush type '" + brush.GetType().Name + "' is not supported by this adapter. " +
                "Only GdiBrush (solid) is supported at the moment.");
        }

        /// <summary>Оборачивает <see cref="SolidBrush"/></summary>
        public static IBrush Wrap(this SolidBrush brush) => new GdiBrush(brush);

        /// <summary>Извлекает GDI Bitmap из IImage.</summary>
        public static Bitmap ToGdi(this IImage image) => (image as GdiImage)?.Inner;

        /// <summary>Оборачивает <see cref="Bitmap"/></summary>
        public static IImage Wrap(this Bitmap image) => new GdiImage(image);

        /// <summary>Извлекает GDI StringFormat из ITextFormat.</summary>
        public static StringFormat ToGdi(this ITextFormat format) => (format as GdiTextFormat)?.Inner;

        /// <summary>Оборачивает <see cref="StringFormat"/></summary>
        public static ITextFormat Wrap(this StringFormat format) => new GdiTextFormat(format);

        /// <summary>Извлекает GDI GraphicsPath из IGraphicsPath.</summary>
        public static GraphicsPath ToGdi(this IGraphicsPath path) => (path as GdiGraphicsPath)?.Inner;

        /// <summary>Оборачивает <see cref="GraphicsPath"/></summary>
        public static IGraphicsPath Wrap(this GraphicsPath path) => new GdiGraphicsPath(path);

        /// <summary>Конвертирует DisplayNodes.Core.Color в System.Drawing.Color.</summary>
        public static System.Drawing.Color ToGdi(this Core.Color c)
			=> System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

		/// <summary>Конвертирует System.Drawing.Color в DisplayNodes.Core.Color.</summary>
		public static Core.Color FromGdi(this System.Drawing.Color c)
			=> new Core.Color(c.R, c.G, c.B, c.A);

		/// <summary>Конвертирует Rect в RectangleF.</summary>
		public static RectangleF ToGdi(this Rect r)
			=> new RectangleF(r.X, r.Y, r.Width, r.Height);

		/// <summary>Конвертирует RectangleF в Rect.</summary>
		/// <exception cref="OverflowException"></exception>
		public static Rect FromGdi(this RectangleF r)
			=> new Rect(Convert.ToInt32(r.X), Convert.ToInt32(r.Y), Convert.ToInt32(r.Width), Convert.ToInt32(r.Height));

        /// <summary>Конвертирует DisplayNodes.Core.Point в System.Drawing.Point.</summary>
        public static System.Drawing.Point ToGdi(this Point p)
            => new System.Drawing.Point(p.X, p.Y);

        /// <summary>Конвертирует System.Drawing.Point в DisplayNodes.Core.Point.</summary>
        public static Point FromGdi(this System.Drawing.Point p)
            => new Point(p.X, p.Y);

        /// <summary>Конвертирует DisplayNodes.Core.Size в System.Drawing.Size.</summary>
        public static System.Drawing.Size ToGdi(this Size p)
            => new System.Drawing.Size(p.Width, p.Height);

        /// <summary>Конвертирует System.Drawing.Size в DisplayNodes.Core.Size.</summary>
        public static Size FromGdi(this System.Drawing.Size p)
            => new Size(p.Width, p.Height);

        /// <summary>
        /// Создаёт GDI-кисть для линейного градиента под заданный прямоугольник.
        /// </summary>
        public static LinearGradientBrush CreateGdiBrush(this GdiLinearGradientBrush brush, RectangleF bounds)
        {
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            float x1 = bounds.X + bounds.Width * brush.Start.X / 100f;
            float y1 = bounds.Y + bounds.Height * brush.Start.Y / 100f;
            float x2 = bounds.X + bounds.Width * brush.End.X / 100f;
            float y2 = bounds.Y + bounds.Height * brush.End.Y / 100f;

            var gdiBrush = new LinearGradientBrush(
                new PointF(x1, y1),
                new PointF(x2, y2),
                brush.Stops[0].Color.ToGdi(),
                brush.Stops[brush.Stops.Length - 1].Color.ToGdi());

            var blend = new ColorBlend(brush.Stops.Length)
            {
                Positions = new float[brush.Stops.Length],
                Colors = new System.Drawing.Color[brush.Stops.Length]
            };

            for (int i = 0; i < brush.Stops.Length; i++)
            {
                blend.Positions[i] = brush.Stops[i].Offset.Value / 100f;
                blend.Colors[i] = brush.Stops[i].Color.ToGdi();
            }

            gdiBrush.InterpolationColors = blend;

            return gdiBrush;
        }

        /// <summary>
        /// Создаёт GDI-кисть для радиального градиента под заданный прямоугольник.
        /// </summary>
        public static PathGradientBrush CreateGdiBrush(this GdiRadialGradientBrush brush, RectangleF bounds)
        {
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            float cx = bounds.X + bounds.Width * brush.Center.X / 100f;
            float cy = bounds.Y + bounds.Height * brush.Center.Y / 100f;
            float minSide = Math.Min(bounds.Width, bounds.Height);
            float r = minSide * brush.Radius.Value / 100f;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - r, cy - r, r * 2, r * 2);

                var pathBrush = new PathGradientBrush(path)
                {
                    CenterPoint = new PointF(cx, cy),
                    CenterColor = brush.Stops[0].Color.ToGdi()
                };

                // GDI+ PathGradientBrush поддерживает либо CenterColor + SurroundColors,
                // либо InterpolationColors. Для N стопов используем InterpolationColors.

                var blend = new ColorBlend(brush.Stops.Length)
                {
                    Positions = new float[brush.Stops.Length],
                    Colors = new System.Drawing.Color[brush.Stops.Length]
                };

                for (int i = 0; i < brush.Stops.Length; i++)
                {
                    blend.Positions[i] = brush.Stops[i].Offset.Value / 100f;
                    blend.Colors[i] = brush.Stops[i].Color.ToGdi();
                }

                pathBrush.InterpolationColors = blend;

                return pathBrush;
            }
        }
    }
}