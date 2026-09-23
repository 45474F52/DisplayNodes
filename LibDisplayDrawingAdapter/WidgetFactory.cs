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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using DisplayNodes.LibDisplayDrawingAdapter.Components;
using DisplayNodes.LibDisplayDrawingAdapter.Components.Masks;

using Image = DisplayNodes.LibDisplayDrawingAdapter.Components.Image;

namespace DisplayNodes.LibDisplayDrawingAdapter
{
	/// <summary>
	/// Реализация <see cref="IWidgetFactory"/>. Создаёт обёртки над <c>LibDisplayDrawing</c>.
	/// </summary>
	internal sealed class WidgetFactory : IWidgetFactory
	{
		/// <inheritdoc/>
		public ILabelComponent CreateLabel() => new Label();

		/// <inheritdoc/>
		public IImageComponent CreateImage() => new Image();

		/// <inheritdoc/>
		public IMaskComponent CreateRectMask() => new RectMask();

		/// <inheritdoc/>
		public IMaskComponent CreateCircleMask() => new CircleMask();

		/// <inheritdoc/>
		public IMaskComponent CreateEllipseMask() => new EllipseMask();

		/// <inheritdoc/>
		public IMaskComponent CreateRoundedRectMask(float cornerRadius)
			=> new RoundedRectMask(cornerRadius);

		/// <inheritdoc/>
		public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
		{
			return new PathMask(rectF =>
				{
					var rect = new Rect((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
					var abstractPath = pathBuilder(rect);
					return abstractPath.ToGdi();
				});
		}
	}
}