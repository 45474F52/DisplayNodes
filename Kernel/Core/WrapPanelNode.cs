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

namespace DisplayNodes.Core
{
	/// <summary>
	/// Контейнер с автоматическим переносом детей на следующую строку (или столбец)
	/// при нехватке места вдоль главной оси.
	/// </summary>
	/// <remarks>
	/// <para>В отличие от <see cref="StackLayoutNode"/>, не растягивает детей и не поддерживает
	/// <see cref="LayoutNode.FlexWeight"/>. Каждый ребёнок получает свой желаемый размер,
	/// но переносится на новую строку, если не помещается в текущую.</para>
	/// <para>Если ребёнок шире (в Horizontal) или выше (в Vertical) доступного пространства —
	/// он остаётся в текущей строке и переполняет её.</para>
	/// </remarks>
	public class WrapPanelNode : LayoutNode
	{
		/// <summary>
		/// Направление переноса.
		/// </summary>
		public WrapDirection Direction { get; set; } = WrapDirection.Horizontal;

		/// <summary>
		/// Расстояние между детьми вдоль главной оси (горизонтали для <see cref="WrapDirection.Horizontal"/>,
		/// вертикали для <see cref="WrapDirection.Vertical"/>).
		/// </summary>
		public int Spacing { get; set; }

		/// <summary>
		/// Расстояние между строками (или столбцами) — вдоль поперечной оси.
		/// </summary>
		public int LineSpacing { get; set; }

		/// <summary>
		/// Контейнеры всегда занимают весь предоставленный слот (с учётом Margin).
		/// Дети располагаются внутри слота с учётом <see cref="LayoutNode.Padding"/>.
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
			if (Children.Count == 0)
				return new Size(Padding.Horizontal, Padding.Vertical);

			Size inner = available.Deflate(Padding);

			return Direction == WrapDirection.Horizontal
				? MeasureHorizontal(inner)
				: MeasureVertical(inner);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect)
		{
			Rect slot = finalRect.Deflate(Padding);

			if (Direction == WrapDirection.Horizontal)
				ArrangeHorizontal(slot);
			else
				ArrangeVertical(slot);
		}

		private Size MeasureHorizontal(Size available)
		{
			int maxRowWidth = 0;
			int currentRowWidth = 0;
			int currentRowHeight = 0;

			int totalHeight = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				Size childSize = child.Measure(available);

				// Если строка не пуста — добавляем spacing.
				int addWidth = childSize.Width + (currentRowWidth > 0 ? Spacing : 0);

				bool fits = available.Width == int.MaxValue
					|| currentRowWidth + addWidth <= available.Width
					|| currentRowWidth == 0; // ребёнок шире строки — оставляем, переполняем

				if (fits)
				{
					currentRowWidth += addWidth;
					currentRowHeight = Math.Max(currentRowHeight, childSize.Height);
				}
				else
				{
					maxRowWidth = Math.Max(maxRowWidth, currentRowWidth);
					totalHeight += currentRowHeight + LineSpacing;

					currentRowWidth = childSize.Width;
					currentRowHeight = childSize.Height;
				}
			}

			maxRowWidth = Math.Max(maxRowWidth, currentRowWidth);
			totalHeight += currentRowHeight;

			return new Size(
				maxRowWidth + Padding.Horizontal,
				totalHeight + Padding.Vertical);
		}

		private void ArrangeHorizontal(Rect slot)
		{
			int availableWidth = slot.Size.Width;
			int x = slot.Point.X;
			int y = slot.Point.Y;
			int currentRowHeight = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				Size childSize = child.DesiredSize;

				int addWidth = childSize.Width + (x > slot.Point.X ? Spacing : 0);

				bool fits = availableWidth == int.MaxValue
					|| x + addWidth - slot.Point.X <= availableWidth
					|| x == slot.Point.X;

				if (!fits)
				{
					// Перенос на новую строку.
					y += currentRowHeight + LineSpacing;
					x = slot.Point.X;
					currentRowHeight = 0;
				}
				else
				{
					x += (x > slot.Point.X ? Spacing : 0);
				}

				Rect rect = new Rect(x, y, childSize.Width, childSize.Height);
				child.Arrange(rect);

				x += childSize.Width;
				currentRowHeight = Math.Max(currentRowHeight, childSize.Height);
			}
		}

		private Size MeasureVertical(Size available)
		{
			int maxColHeight = 0;
			int currentColHeight = 0;
			int currentColWidth = 0;

			int totalWidth = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				Size childSize = child.Measure(available);

				int addHeight = childSize.Height + (currentColHeight > 0 ? Spacing : 0);

				bool fits = available.Height == int.MaxValue
					|| currentColHeight + addHeight <= available.Height
					|| currentColHeight == 0;

				if (fits)
				{
					currentColHeight += addHeight;
					currentColWidth = Math.Max(currentColWidth, childSize.Width);
				}
				else
				{
					maxColHeight = Math.Max(maxColHeight, currentColHeight);
					totalWidth += currentColWidth + LineSpacing;

					currentColHeight = childSize.Height;
					currentColWidth = childSize.Width;
				}
			}

			maxColHeight = Math.Max(maxColHeight, currentColHeight);
			totalWidth += currentColWidth;

			return new Size(
				totalWidth + Padding.Horizontal,
				maxColHeight + Padding.Vertical);
		}

		private void ArrangeVertical(Rect slot)
		{
			int availableHeight = slot.Size.Height;
			int x = slot.Point.X;
			int y = slot.Point.Y;
			int currentColWidth = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				Size childSize = child.DesiredSize;

				int addHeight = childSize.Height + (y > slot.Point.Y ? Spacing : 0);

				bool fits = availableHeight == int.MaxValue
					|| y + addHeight - slot.Point.Y <= availableHeight
					|| y == slot.Point.Y;

				if (!fits)
				{
					x += currentColWidth + LineSpacing;
					y = slot.Point.Y;
					currentColWidth = 0;
				}
				else
				{
					y += (y > slot.Point.Y ? Spacing : 0);
				}

				Rect rect = new Rect(x, y, childSize.Width, childSize.Height);
				child.Arrange(rect);

				y += childSize.Height;
				currentColWidth = Math.Max(currentColWidth, childSize.Width);
			}
		}
	}
}