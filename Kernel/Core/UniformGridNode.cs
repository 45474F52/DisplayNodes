using System;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Контейнер, располагающий детей в равномерной сетке с фиксированным числом строк и колонок.
	/// <br/>Все ячейки имеют одинаковый размер.
	/// </summary>
	/// <remarks>
	/// Размер ячейки вычисляется как максимум из:
	/// <br/>1) Доступного пространства, делённого на количество колонок/строк.
	/// <br/>2) Желаемого размера (<see cref="LayoutNode.DesiredSize"/>) самого крупного ребёнка.
	/// </remarks>
	public class UniformGridNode : LayoutNode
	{
		/// <summary>Количество строк в сетке (минимум 1).</summary>
		public int Rows { get; }

		/// <summary>Количество колонок в сетке (минимум 1).</summary>
		public int Columns { get; }

		/// <summary>Расстояние между ячейками в пикселях.</summary>
		public int Spacing { get; set; }

		/// <summary>
		/// Создаёт узел равномерной сетки.
		/// </summary>
		/// <param name="rows">Количество строк.</param>
		/// <param name="columns">Количество колонок.</param>
		/// <param name="spacing">Расстояние между ячейками.</param>
		public UniformGridNode(int rows, int columns, int spacing = 0)
		{
			Rows = Math.Max(1, rows);
			Columns = Math.Max(1, columns);
			Spacing = spacing;
		}

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
			var inner = available.Deflate(Padding);

			// 1. Сначала измеряем детей с Infinity, чтобы узнать их реальный размер
			int maxChildWidth = 0, maxChildHeight = 0;
			foreach (var child in Children)
			{
				var childSize = child.Measure(Size.Infinity);
				maxChildWidth = Math.Max(maxChildWidth, childSize.Width);
				maxChildHeight = Math.Max(maxChildHeight, childSize.Height);
			}

			// 2. Вычисляем размер ячейки
			int cellWidth, cellHeight;

			// ВАЖНО: Проверяем available, а не inner, т.к. inner уже уменьшен на Padding
			if (available.Width == int.MaxValue)
				cellWidth = maxChildWidth; // Классическое WPF-поведение при бесконечном пространстве
			else
				cellWidth = Math.Max(maxChildWidth, Math.Max(0, (inner.Width - Spacing * (Columns - 1)) / Columns));

			if (available.Height == int.MaxValue)
				cellHeight = maxChildHeight;
			else
				cellHeight = Math.Max(maxChildHeight, Math.Max(0, (inner.Height - Spacing * (Rows - 1)) / Rows));

			// 3. Переизмеряем детей с реальным cell size
			foreach (var child in Children)
				_ = child.Measure(new Size(cellWidth, cellHeight));

			int totalWidth = cellWidth * Columns + Spacing * Math.Max(0, Columns - 1) + Padding.Horizontal;
			int totalHeight = cellHeight * Rows + Spacing * Math.Max(0, Rows - 1) + Padding.Vertical;

			return new Size(totalWidth, totalHeight);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect)
		{
			var slot = new Rect(
				finalRect.Point.Offset(Padding),
				finalRect.Size.Deflate(Padding)
			);

			int cellWidth = Math.Max(0, (slot.Size.Width - Spacing * (Columns - 1)) / Columns);
			int cellHeight = Math.Max(0, (slot.Size.Height - Spacing * (Rows - 1)) / Rows);

			for (int i = 0; i < Children.Count; i++)
			{
				int row = i / Columns;
				int col = i % Columns;

				int x = slot.Point.X + col * (cellWidth + Spacing);
				int y = slot.Point.Y + row * (cellHeight + Spacing);

				var rect = new Rect(x, y, cellWidth, cellHeight);
				Children[i].Arrange(rect);
			}
		}
	}
}