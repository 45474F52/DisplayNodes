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
			: base(component)
		{
			Component.Text = string.Empty;
			if (Component is ITextLayoutComponent t) t.Stretch = LabelStretch.None;
			HAlignment = Alignment.Stretch;
			VAlignment = Alignment.Stretch;

			Component.BackgroundBrush = brushFactory(color);
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available) => Size.Empty;

		/// <inheritdoc/>
		protected override void ApplyBounds()
		{
			Component.Location = Bounds.Point;
			Component.Size = Bounds.Size;
		}
	}
}