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
	/// Контейнер, располагающий дочерние элементы в строку или столбец.
	/// </summary>
	/// <remarks>
	/// <para>Размер контейнера вычисляется как сумма размеров детей + <see cref="Spacing"/>
	/// + <see cref="LayoutNode.Padding"/>.</para>
	/// <para><b>Главная ось</b> — направление расположения (вертикаль/горизонталь).<br/>
	/// <b>Поперечная ось</b> — перпендикулярное направление.
	/// Выравнивание по ней задаётся самими детьми через <see cref="LayoutNode.HAlignment"/>
	/// / <see cref="LayoutNode.VAlignment"/>.</para>
	/// <para><b>Flex-дети.</b> Дети с <see cref="LayoutNode.FlexWeight"/> &gt; 0 участвуют в
	/// распределении свободного пространства вдоль главной оси пропорционально весам.
	/// При нехватке места flex-дети сжимаются пропорционально весам (fixed-дети не сжимаются).</para>
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

		/// <summary>
		/// Контейнеры всегда занимают весь предоставленный слот (с учётом Margin).
		/// <see cref="MainAxisAlignment"/> и распределение пространства работают
		/// внутри этого слота, а не в рамках <see cref="LayoutNode.DesiredSize"/>.
		/// </summary>
		protected override void ArrangeOverride(Rect finalRect)
		{
			Rect slot = finalRect.Deflate(Padding);

			int mainSize = IsVertical ? slot.Size.Height : slot.Size.Width;
			int totalNatural = 0;
			double weightSum = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				int natural = IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
				totalNatural += natural;
				if (child.FlexWeight > 0)
				{
					weightSum += child.FlexWeight;
				}
				if (i > 0)
					totalNatural += Spacing;
			}

			int[] childMain = new int[Children.Count];
			int totalMain = 0;
			int free = mainSize - totalNatural;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				int natural = IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
				int cm;
				if (child.FlexWeight > 0 && weightSum > 0)
				{
					int share = free >= 0
						? (int)Math.Round(free * (child.FlexWeight / weightSum))
						: -(int)Math.Round(-free * (child.FlexWeight / weightSum));
					cm = Math.Max(0, natural + share);
				}
				else
				{
					cm = natural;
				}
				childMain[i] = cm;
				totalMain += cm;
				if (i > 0)
					totalMain += Spacing;
			}

			int actualFree = Math.Max(0, mainSize - totalMain);

			int offset;
			int extraGap;
			switch (MainAxisAlignment)
			{
				case MainAxisAlignment.Center:
				offset = actualFree / 2;
				extraGap = 0;
				break;
				case MainAxisAlignment.End:
				offset = actualFree;
				extraGap = 0;
				break;
				case MainAxisAlignment.SpaceBetween when Children.Count > 1:
				offset = 0;
				extraGap = actualFree / (Children.Count - 1);
				break;
				default:
				offset = 0;
				extraGap = 0;
				break;
			}

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				int cm = childMain[i];

				Rect rect = IsVertical
					? new Rect(slot.Point.X, slot.Point.Y + offset, slot.Size.Width, cm)
					: new Rect(slot.Point.X + offset, slot.Point.Y, cm, slot.Size.Height);

				child.Arrange(rect);
				offset += cm + Spacing + extraGap;
			}
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available)
		{
			var inner = available.Deflate(Padding);
			int availableMain = IsVertical ? inner.Height : inner.Width;
			bool infinite = availableMain == int.MaxValue || availableMain <= 0;

			int totalNatural = 0;
			int cross = 0;
			int flexCount = 0;
			double weightSum = 0;

			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				Size childSize = child.Measure(inner);
				int mainSize = IsVertical ? childSize.Height : childSize.Width;
				int crossSize = IsVertical ? childSize.Width : childSize.Height;
				cross = Math.Max(cross, crossSize);
				totalNatural += mainSize;
				if (child.FlexWeight > 0)
				{
					flexCount++;
					weightSum += child.FlexWeight;
				}
				if (i > 0)
					totalNatural += Spacing;
			}

			if (flexCount == 0 || infinite || weightSum <= 0)
			{
				return IsVertical
					? new Size(cross + Padding.Horizontal, totalNatural + Padding.Vertical)
					: new Size(totalNatural + Padding.Horizontal, cross + Padding.Vertical);
			}

			int free = availableMain - totalNatural;

			int finalMain = 0;
			for (int i = 0; i < Children.Count; i++)
			{
				LayoutNode child = Children[i];
				int natural = IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
				if (child.FlexWeight > 0)
				{
					int share = free >= 0
						? (int)Math.Round(free * (child.FlexWeight / weightSum))
						: -(int)Math.Round(-free * (child.FlexWeight / weightSum));
					finalMain += Math.Max(0, natural + share);
				}
				else
				{
					finalMain += natural;
				}
				if (i > 0)
					finalMain += Spacing;
			}

			return IsVertical
				? new Size(cross + Padding.Horizontal, finalMain + Padding.Vertical)
				: new Size(finalMain + Padding.Horizontal, cross + Padding.Vertical);
		}
	}
}
