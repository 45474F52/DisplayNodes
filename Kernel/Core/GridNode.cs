using System;
using System.Collections.Generic;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Контейнер-сетка, размещающий детей в ячейках, определённых строками и колонками.<br/>
	/// Поддерживает фиксированные (Pixel), автоматические (Auto) и пропорциональные (Star) размеры.
	/// </summary>
	/// <remarks>
	/// <para>Размер контейнера вычисляется как сумма размеров ячеек + <see cref="LayoutNode.Padding"/>.</para>
	/// <para>Для добавления элементов используется метод <see cref="Add"/>, указывая координаты ячейки.</para>
	/// </remarks>
	public class GridNode : LayoutNode
	{
		private sealed class GridChild
		{
			public LayoutNode Node;
			public int Row;
			public int Column;
			public GridChild(LayoutNode node, int row, int col) { Node = node; Row = row; Column = col; }
		}

		private readonly struct Dimensions
		{
			public readonly double[] RowHeights;
			public readonly double[] ColumnWidths;

			public Dimensions(double[] rowHeights, double[] columnWidths)
			{
				this.RowHeights = rowHeights;
				this.ColumnWidths = columnWidths;
			}
		}

		/// <summary>Определения высот строк сетки.</summary>
		public IList<RowDefinition> RowDefinitions { get; } = new List<RowDefinition>();

		/// <summary>Определения ширин колонок сетки.</summary>
		public IList<ColumnDefinition> ColumnDefinitions { get; } = new List<ColumnDefinition>();

		private readonly IList<GridChild> _gridChildren = new List<GridChild>();

		/// <summary>
		/// Добавляет дочерний узел в указанную ячейку сетки.
		/// </summary>
		/// <param name="child">Добавляемый узел.</param>
		/// <param name="row">Индекс строки (начиная с 0).</param>
		/// <param name="column">Индекс колонки (начиная с 0).</param>
		/// <returns>Ссылка на текущий <see cref="GridNode"/> для fluent-цепочек.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public GridNode Add(LayoutNode child, int row, int column)
		{
			if (child == null)
				throw new ArgumentNullException(nameof(child));

			if (row < 0)
				throw new ArgumentOutOfRangeException(nameof(row));

			if (column < 0)
				throw new ArgumentOutOfRangeException(nameof(column));

			if (row >= RowDefinitions.Count)
				throw new ArgumentOutOfRangeException(nameof(row), "Row index exceeds RowDefinitions");

			if (column >= ColumnDefinitions.Count)
				throw new ArgumentOutOfRangeException(nameof(column), "Column index exceeds ColumnDefinitions");

			_gridChildren.Add(new GridChild(child, row, column));

			// Синхронизирую с базовым списком, чтобы DisposeTree() и другие 
			// методы, работающие с базовым типом LayoutNode, корректно обходили дерево.
			base.Children.Add(child);

			return this;
		}

		/// <summary>
		/// Рассчитывает размеры строк и колонок на основе доступного пространства.
		/// Не сохраняет состояние в полях класса (чистая функция).
		/// </summary>
		private Dimensions CalculateDimensions(Size available)
		{
			if (RowDefinitions.Count == 0)
				RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			if (ColumnDefinitions.Count == 0)
				ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

			int rowCount = RowDefinitions.Count;
			int colCount = ColumnDefinitions.Count;

			double[] rowHeights = new double[rowCount];
			double[] colWidths = new double[colCount];

			// 0-й проход: инициализируем фиксированные (Pixel) размеры
			for (int i = 0; i < rowCount; i++)
				if (RowDefinitions[i].Height.IsAbsolute)
					rowHeights[i] = RowDefinitions[i].Height.Value;

			for (int i = 0; i < colCount; i++)
				if (ColumnDefinitions[i].Width.IsAbsolute)
					colWidths[i] = ColumnDefinitions[i].Width.Value;

			// 1-й проход: измеряем Auto-детей и обновляем их размеры
			foreach (var gc in _gridChildren)
			{
				if (gc.Row < 0 || gc.Column < 0 || gc.Row >= rowCount || gc.Column >= colCount)
					continue;

				double availW = ColumnDefinitions[gc.Column].Width.IsAbsolute
					? ColumnDefinitions[gc.Column].Width.Value
					: available.Width;
				double availH = RowDefinitions[gc.Row].Height.IsAbsolute
					? RowDefinitions[gc.Row].Height.Value
					: available.Height;

				availW = Math.Min(availW, available.Width);
				availH = Math.Min(availH, available.Height);

				var childSize = gc.Node.Measure(new Size((int)availW, (int)availH));

				if (RowDefinitions[gc.Row].Height.IsAuto)
					rowHeights[gc.Row] = Math.Max(rowHeights[gc.Row], childSize.Height);
				if (ColumnDefinitions[gc.Column].Width.IsAuto)
					colWidths[gc.Column] = Math.Max(colWidths[gc.Column], childSize.Width);
			}

			// 2-й проход: вычисляем коэффициенты для Star
			double totalStarH = 0, totalStarW = 0;
			for (int i = 0; i < rowCount; i++)
				if (RowDefinitions[i].Height.IsStar)
					totalStarH += RowDefinitions[i].Height.Value;

			for (int i = 0; i < colCount; i++)
				if (ColumnDefinitions[i].Width.IsStar)
					totalStarW += ColumnDefinitions[i].Width.Value;

			double maxW = available.Width == int.MaxValue ? 0 : available.Width;
			double maxH = available.Height == int.MaxValue ? 0 : available.Height;

			// ВАЖНО: SumArray уже включает в себя Absolute значения из 0-го прохода!
			// Поэтому вычитаем только SumArray, чтобы избежать двойного вычитания.
			double remainingH = Math.Max(0, maxH - SumArray(rowHeights));
			double remainingW = Math.Max(0, maxW - SumArray(colWidths));

			// 3-й проход: распределяем Star
			if (totalStarH > 0 && remainingH > 0)
			{
				for (int i = 0; i < rowCount; i++)
				{
					if (RowDefinitions[i].Height.IsStar)
						rowHeights[i] = (RowDefinitions[i].Height.Value / totalStarH) * remainingH;
				}
			}

			if (totalStarW > 0 && remainingW > 0)
			{
				for (int i = 0; i < colCount; i++)
				{
					if (ColumnDefinitions[i].Width.IsStar)
						colWidths[i] = (ColumnDefinitions[i].Width.Value / totalStarW) * remainingW;
				}
			}

			return new Dimensions(rowHeights, colWidths);
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
			var dims = CalculateDimensions(inner);

			int totalWidth = (int)SumArray(dims.ColumnWidths);
			int totalHeight = (int)SumArray(dims.RowHeights);

			return new Size(totalWidth + Padding.Horizontal, totalHeight + Padding.Vertical);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect)
		{
			Rect slot = finalRect.Deflate(Padding);
			Dimensions dims = CalculateDimensions(slot.Size);

			int rowCount = dims.RowHeights.Length;
			int colCount = dims.ColumnWidths.Length;

			// ПРЕДВАРИТЕЛЬНЫЙ РАСЧЁТ КООРДИНАТ (O(N) вместо O(N^3) с FirstOrDefault)
			double[] colX = new double[colCount];
			double currentX = slot.X;

			for (int c = 0; c < colCount; c++)
			{
				colX[c] = currentX;
				currentX += dims.ColumnWidths[c];
			}

			double[] rowY = new double[rowCount];
			double currentY = slot.Y;

			for (int r = 0; r < rowCount; r++)
			{
				rowY[r] = currentY;
				currentY += dims.RowHeights[r];
			}

			foreach (GridChild gc in _gridChildren)
			{
				if (gc.Row < 0 ||
					gc.Column < 0 ||
					gc.Row >= rowCount ||
					gc.Column >= colCount)
				{
					continue;
				}

				int x = (int)Math.Round(colX[gc.Column]);
				int y = (int)Math.Round(rowY[gc.Row]);

				int width = Math.Max(0, (int)Math.Round(dims.ColumnWidths[gc.Column]));
				int height = Math.Max(0, (int)Math.Round(dims.RowHeights[gc.Row]));

				gc.Node.Arrange(new Rect(x, y, width, height));
			}
		}

		// Небольшая оптимизация: свой метод суммы вместо LINQ .Sum() для double[], 
		// чтобы избежать аллокации enumerator-а на каждом Measure.
		private static double SumArray(double[] array)
		{
			double sum = 0;
			for (int i = 0; i < array.Length; i++)
				sum += array[i];
			return sum;
		}
	}
}