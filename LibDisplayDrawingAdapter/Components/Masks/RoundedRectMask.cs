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
			using (var path = new GraphicsPath())
			{
				float r = Math.Min(CornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);
				float d = r * 2;
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