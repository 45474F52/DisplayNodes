using System;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Контейнер, располагающий дочерние элементы в строку или столбец.
	/// </summary>
	/// <remarks>
	/// <para>Размер контейнера вычисляется как сумма размеров детей + <see cref="Spacing"/>
	/// + <see cref="LayoutNode.Padding"/>.</para>
	/// <para><b>Главная ось</b> — направление расположения (вертикаль/горизонталь).<br/>
	/// <b>Поперечная ось</b> — перпендикулярное направление.
	/// Выравнивание по ней задаётся самими детьми через <see cref="LayoutNode.HAlignment"/>
	/// / <see cref="LayoutNode.VAlignment"/>.</para>
	/// </remarks>
	public class StackLayoutNode : LayoutNode
	{
		/// <summary>
		/// Направление расположения: <c>true</c> — столбец (вертикально), <c>false</c> — строка (горизонтально).
		/// </summary>
		public bool IsVertical { get; set; }

		/// <summary>
		/// Способ распределения свободного пространства вдоль главной оси.
		/// </summary>
		public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

		/// <summary>
		/// Расстояние между дочерними элементами в пикселях.
		/// </summary>
		public int Spacing { get; set; }

		/// <summary>
		/// Контейнеры всегда занимают весь предоставленный слот (с учётом Margin).
		/// <see cref="MainAxisAlignment"/> и распределение пространства работают
		/// внутри этого слота, а не в рамках <see cref="LayoutNode.DesiredSize"/>.
		/// </summary>
		public sealed override void Arrange(Rect finalRect)
		{
			Rect inner = finalRect.Deflate(Margin);
			Bounds = inner;
			ArrangeOverride(inner);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect)
		{
			Rect slot = finalRect.Deflate(Padding);

			int totalMain = 0;

			foreach (LayoutNode c in Children)
				totalMain += IsVertical ? c.DesiredSize.Height : c.DesiredSize.Width;
			totalMain += Spacing * Math.Max(0, Children.Count - 1);

			int mainSize = IsVertical ? slot.Size.Height : slot.Size.Width;
			int free = Math.Max(0, mainSize - totalMain);

			int offset;
			switch (MainAxisAlignment)
			{
				case MainAxisAlignment.Center:
				offset = free / 2;
				break;
				case MainAxisAlignment.End:
				offset = free;
				break;
				case MainAxisAlignment.SpaceBetween when Children.Count > 1:
				offset = 0;
				break;
				default:
				offset = 0;
				break;
			}

			int extraGap = MainAxisAlignment == MainAxisAlignment.SpaceBetween && Children.Count > 1
				? free / (Children.Count - 1) : 0;

			foreach (LayoutNode child in Children)
			{
				Rect rect = IsVertical
					? new Rect(slot.Point.X, slot.Point.Y + offset, slot.Size.Width, child.DesiredSize.Height)
					: new Rect(slot.Point.X + offset, slot.Point.Y, child.DesiredSize.Width, slot.Size.Height);

				child.Arrange(rect);
				offset += (IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width) + Spacing + extraGap;
			}
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available)
		{
			var inner = available.Deflate(Padding);

			int main = 0, cross = 0;
			for (int i = 0; i < Children.Count; i++)
			{
				var s = Children[i].Measure(inner);
				main += IsVertical ? s.Height : s.Width;
				cross = Math.Max(cross, IsVertical ? s.Width : s.Height);
				if (i > 0)
					main += Spacing;
			}

			return IsVertical
				? new Size(cross + Padding.Horizontal, main + Padding.Vertical)
				: new Size(main + Padding.Horizontal, cross + Padding.Vertical);
		}
	}
}
