using System;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Контейнер, размещающий всех детей в одном слоте (друг поверх друга).
	/// </summary>
	/// <remarks>
	/// Размер контейнера равен размеру наибольшего ребёнка + <see cref="LayoutNode.Padding"/>.<br/>
	/// Каждый ребёнок выравнивается внутри общего слота через свои 
	/// <see cref="LayoutNode.HAlignment"/> и <see cref="LayoutNode.VAlignment"/>.
	/// </remarks>
	public class OverlayNode : LayoutNode
	{
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
		protected override Size MeasureOverride(Size available)
		{
			Size inner = available.Deflate(Padding);
			int maxWidth = 0, maxHeight = 0;

			foreach (LayoutNode child in Children)
			{
				Size size = child.Measure(inner);
				maxWidth = Math.Max(maxWidth, size.Width);
				maxHeight = Math.Max(maxHeight, size.Height);
			}

			return new Size(maxWidth + Padding.Horizontal, maxHeight + Padding.Vertical);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect)
		{
			Rect slot = finalRect.Deflate(Padding);

			foreach (LayoutNode child in Children)
			{
				child.Arrange(slot);
			}
		}
	}
}