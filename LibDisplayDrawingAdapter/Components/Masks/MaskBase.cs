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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

using LibDisplayDrawing;

namespace DisplayNodes.LibDisplayDrawingAdapter.Components.Masks
{
	/// <summary>
	/// Базовый класс для LibDisplayDrawing-адаптеров масок. Наследуется от <see cref="MaskClippingBase"/>
	/// (который сам является <see cref="IComponent"/> через <see cref="Mask2D"/>).
	/// </summary>
	/// <remarks>
	/// Реализует <see cref="IMaskComponent"/> через явную реализацию интерфейса,
	/// т.к. типы <c>Location</c>/<c>Size</c>/<c>Parent</c> в легаси (<c>Point2D</c>/<c>Size2D</c>/<c>IComponent</c>)
	/// отличаются от типов в Core (<see cref="Point"/>/<see cref="Size"/>/<see cref="IRenderComponent"/>).
	/// </remarks>
	internal abstract class MaskBase : MaskClippingBase, IMaskComponent
	{
		private IRenderComponent _parentAdapter;
		private readonly IComponent<Size2D, Point2D, Graphic2D> _typedThis;

		/// <summary>Создаёт адаптер маски.</summary>
		protected MaskBase() : base(null)
		{
			_typedThis = this as IComponent<Size2D, Point2D, Graphic2D>;
		}

		/// <inheritdoc/>
		IRenderComponent IRenderComponent.Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				base.Parent = ComponentHelper.ExtractInner(value);
			}
		}

		/// <inheritdoc/>
		Point IRenderComponent.Location
		{
			get => new Point(_typedThis.Location.X, _typedThis.Location.Y);
			set => _typedThis.Location = new Point2D(value.X, value.Y);
		}

		/// <inheritdoc/>
		Size IRenderComponent.Size
		{
			get => new Size(_typedThis.Size.Width, _typedThis.Size.Height);
			set => _typedThis.Size = new Size2D(value.Width, value.Height);
		}
	}
}