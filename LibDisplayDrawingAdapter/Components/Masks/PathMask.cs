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
	/// <summary>Адаптер маски с произвольной формой.</summary>
	internal sealed class PathMask : MaskBase
	{
		/// <summary>Функция, создающая путь для каждой отрисовки.</summary>
		public Func<RectangleF, GraphicsPath> PathBuilder { get; }

		/// <summary>Создаёт адаптер маски с произвольной формой.</summary>
		public PathMask(Func<RectangleF, GraphicsPath> pathBuilder)
		{
			PathBuilder = pathBuilder;
		}

		/// <inheritdoc/>
		protected override Region CreateRegion(RectangleF bounds)
		{
			if (PathBuilder == null)
				return new Region(bounds);

			using (var path = PathBuilder(bounds))
				return new Region(path);
		}
	}
}