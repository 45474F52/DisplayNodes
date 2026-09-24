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

namespace DisplayNodes.Widgets
{
	/// <summary>
	/// Узел фона. Использует <see cref="ILabelComponent"/> с <see cref="LabelStretch.None"/>.
	/// Растягивается на весь доступный слот и не занимает место в контейнере
	/// (<see cref="LayoutNode.DesiredSize"/> = 0).
	/// </summary>
	public class BackgroundNode : WidgetNode
	{
		/// <summary>Базовый компонент метки, используемый как фон.</summary>
		public new ILabelComponent Component => (ILabelComponent)base.Component;

		/// <summary>Создаёт узел фона заданного цвета.</summary>
		/// <param name="color">Цвет фона (собственная структура Color из Core).</param>
		/// <param name="component">Компонент рендерера (создаётся фабрикой).</param>
		/// <param name="brushFactory">Фабрика кистей (создаётся адаптером).</param>
		public BackgroundNode(Color color, ILabelComponent component, Func<Color, IBrush> brushFactory)
			: this(brushFactory(color), component) { }

        /// <summary>
        /// Создаёт узел фона с готовой кистью.
        /// </summary>
        /// <param name="brush">Кисть фона. Не может быть <c>null</c>.</param>
        /// <param name="component">Компонент рендерера (создаётся фабрикой).</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="brush"/> равен <c>null</c>.</exception>
        public BackgroundNode(IBrush brush, ILabelComponent component)
            : base(component)
        {
            Component.BackgroundBrush = brush ?? throw new ArgumentNullException(nameof(brush));

            Component.Text = string.Empty;
            if (Component is ITextLayoutComponent t)
                t.Stretch = LabelStretch.None;

            HAlignment = Alignment.Stretch;
            VAlignment = Alignment.Stretch;
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size available) => Size.Empty;

        /// <summary>
        /// Фон всегда занимает весь предоставленный слот (с учётом Margin), независимо от
        /// DesiredSize = 0x0. Базовое выравнивание схлопнуло бы нулевой desired в точку,
        /// и «Border без контента» (UI.Border(...).Add(UI.Label(...)), где фон лежит в
        /// отдельном клипе под контентом) становился невидимым.
        /// </summary>
        public sealed override void Arrange(Rect finalRect)
        {
            Rect inner = finalRect.Deflate(Margin);
            Bounds = inner;
            ArrangeOverride(inner);
        }

		/// <inheritdoc/>
		protected override void ApplyBounds()
		{
			Component.Location = Bounds.Point;
			Component.Size = Bounds.Size;
		}
	}
}