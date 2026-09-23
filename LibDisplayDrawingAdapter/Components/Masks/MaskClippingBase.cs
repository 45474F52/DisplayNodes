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

using LibDisplayDrawing;

namespace DisplayNodes.LibDisplayDrawingAdapter.Components.Masks
{
	/// <summary>
	/// Базовый класс для масок с произвольной формой обрезки.
	/// </summary>
	public abstract class MaskClippingBase : Mask2D
	{
		/// <summary>Инициализирует базовый класс <see cref="Mask2D"/>.</summary>
		/// <param name="parent">Родительский компонент.</param>
		public MaskClippingBase(IComponent parent) : base(parent) { }

		/// <summary>
		/// Создаёт область обрезки. Вызывается при каждом <see cref="DrawBitmap"/>.
		/// Наследники переопределяют этот метод, чтобы задать форму (круг, эллипс, путь и т.д.).
		/// </summary>
		/// <param name="bounds">Глобальные границы маски.</param>
		/// <returns>Region, ограничивающий область отрисовки.</returns>
		protected abstract Region CreateRegion(RectangleF bounds);

		/// <inheritdoc/>
		public override void DrawBitmap(ref Graphics graphics, ref Action<Graphics, bool> callBack)
		{
			var bounds = new RectangleF(
				(float)base.GlobalLocation.X,
				(float)base.GlobalLocation.Y,
				(float)base.Size.Width,
				(float)base.Size.Height
			);

			Region oldClip = graphics.Clip;

			try
			{
				using (var newRegion = CreateRegion(bounds))
				{
					newRegion.Intersect(oldClip);
					graphics.Clip = newRegion;
					base.DrawBitmap(ref graphics, ref callBack);
				}
			}
			finally
			{
				graphics.Clip = oldClip;
			}
		}
	}
}