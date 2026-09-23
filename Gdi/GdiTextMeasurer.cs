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