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
using DisplayNodes.LibDisplayDrawingAdapter.Components.Masks;

using LibDisplayDrawing;

namespace DisplayNodes.LibDisplayDrawingAdapter.Components
{
	/// <summary>
	/// Базовый класс для LibDisplayDrawingAdapter-адаптеров. Маппит <see cref="IRenderComponent"/>
	/// на внутренний <see cref="IComponent"/> из <c>LibDisplayDrawing</c>.
	/// </summary>
	internal abstract class ComponentBase : IRenderComponent
	{
		/// <summary>Внутренний компонент легаси-библиотеки.</summary>
		public IComponent Inner { get; }

		/// <summary>
		/// Внутренний компонент, приведённый к обобщённому интерфейсу
		/// для доступа к <see cref="IComponent{T1,T2,T3}.Location"/> и <see cref="IComponent{T1,T2,T3}.Size"/>.
		/// </summary>
		private readonly IComponent<Size2D, Point2D, Graphic2D> _typedInner;

		private IRenderComponent _parentAdapter;

		/// <summary>Создаёт адаптер над заданным внутренним компонентом.</summary>
		protected ComponentBase(IComponent inner)
		{
			Inner = inner ?? throw new ArgumentNullException(nameof(inner));
			_typedInner = inner as IComponent<Size2D, Point2D, Graphic2D>
				?? throw new ArgumentException(
					$"Component must implement IComponent<Size2D, Point2D, Graphic2D>. Got: {inner.GetType().Name}",
					nameof(inner));
		}

		/// <inheritdoc/>
		public IRenderComponent Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				Inner.Parent = ComponentHelper.ExtractInner(value);
			}
		}

		/// <inheritdoc/>
		public Point Location
		{
			get => new Point(_typedInner.Location.X, _typedInner.Location.Y);
			set => _typedInner.Location = new Point2D(value.X, value.Y);
		}

		/// <inheritdoc/>
		public Size Size
		{
			get => new Size(_typedInner.Size.Width, _typedInner.Size.Height);
			set => _typedInner.Size = new Size2D(value.Width, value.Height);
		}

		/// <inheritdoc/>
		public bool Visible
		{
			get => Inner.Visible;
			set => Inner.Visible = value;
		}
	}

	/// <summary>Хелпер для извлечения внутреннего <see cref="IComponent"/> из адаптера.</summary>
	internal static class ComponentHelper
	{
		/// <summary>
		/// Извлекает внутренний <see cref="IComponent"/> из адаптера.
		/// Возвращает <c>null</c>, если <paramref name="component"/> не является LibDisplayDrawing-адаптером.
		/// </summary>
		public static IComponent ExtractInner(IRenderComponent component)
		{
			if (component is ComponentBase c)
				return c.Inner;
			if (component is MaskBase m)
				return m;
			return null;
		}
	}
}