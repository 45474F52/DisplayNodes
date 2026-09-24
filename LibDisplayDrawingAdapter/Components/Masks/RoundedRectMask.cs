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

namespace DisplayNodes.LibDisplayDrawingAdapter.Components.Masks
{
	/// <summary>Адаптер маски со скруглёнными углами.</summary>
	internal sealed class RoundedRectMask : MaskBase
	{
		/// <summary>Радиус скругления углов.</summary>
		public float CornerRadius { get; }

		/// <summary>Создаёт адаптер маски со скруглёнными углами.</summary>
		public RoundedRectMask(float cornerRadius)
		{
			CornerRadius = cornerRadius;
		}

		/// <inheritdoc/>
		protected override Region CreateRegion(RectangleF bounds)
		{
			// Нулевой размер на время лэйаута: возвращаем пустую область вместо недопустимой формы.
			if (bounds.Width <= 0 || bounds.Height <= 0)
				return new Region(RectangleF.Empty);

			// Радиус не должен превращаться в ноль: AddArc с нулевым размером дуги — ArgumentException.
			float r = Math.Max(0.5f, Math.Min(CornerRadius, Math.Min(bounds.Width, bounds.Height) / 2f));
			if (r >= Math.Min(bounds.Width, bounds.Height) / 2f)
			{
				// Вырожденный случай (сторона <= 2*радиуса): обрезаем эллипсом целиком.
				using (var path = new GraphicsPath())
				{
					path.AddEllipse(bounds);
					return new Region(path);
				}
			}

			float d = r * 2;
			using (var path = new GraphicsPath())
			{
				path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
				path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
				path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
				path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
				path.CloseFigure();
				return new Region(path);
			}
		}
	}
}