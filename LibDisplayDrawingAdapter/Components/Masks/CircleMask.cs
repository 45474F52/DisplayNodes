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
	/// <summary>Адаптер круговой маски.</summary>
	internal sealed class CircleMask : MaskBase
	{
		/// <inheritdoc/>
		protected override Region CreateRegion(RectangleF bounds)
		{
			using (var path = new GraphicsPath())
			{
				float diameter = Math.Min(bounds.Width, bounds.Height);
				float x = bounds.X + (bounds.Width - diameter) / 2;
				float y = bounds.Y + (bounds.Height - diameter) / 2;
				path.AddEllipse(x, y, diameter, diameter);
				return new Region(path);
			}
		}
	}
}