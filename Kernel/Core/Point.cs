using System;
using System.Diagnostics;

namespace DisplayNodes.Core
{
	/// <summary>Структура, представляющая точку в 2D-пространстве.</summary>
	[Serializable]
	[DebuggerDisplay("({X}:{Y})")]
	public readonly struct Point : IEquatable<Point>
	{
		/// <summary>Координата X.</summary>
		public readonly int X;

		/// <summary>Координата Y.</summary>
		public readonly int Y;

		/// <summary>Создаёт точку с заданными координатами.</summary>
		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		/// <summary>Точка (0, 0).</summary>
		public static Point Empty => new Point(0, 0);

		/// <summary>Точка с максимально возможными координатами.</summary>
		public static Point Infinity => new Point(int.MaxValue, int.MaxValue);

		/// <summary>
		/// Создаёт новую точку, смещённую относительно данной на заданные значения.
		/// </summary>
		/// <param name="dx">Смещение по оси X.</param>
		/// <param name="dy">Смещение по оси Y.</param>
		/// <returns>Новая точка с координатами (X + <paramref name="dx"/>, Y + <paramref name="dy"/>).</returns>
		public Point Offset(int dx, int dy) => new Point(X + dx, Y + dy);

		/// <summary>
		/// Создаёт новую точку, смещённую относительно данной на заданный отступ.
		/// </summary>
		/// <param name="t">Отступ для смещения</param>
		/// <returns>Новая точка с координатами (X + <see cref="Thickness.Left"/>, Y + <see cref="Thickness.Top"/>).</returns>
		public Point Offset(Thickness t)
		{
			if (t.IsZero)
				return this;

			return Offset(t.Left, t.Top);
		}

		/// <summary>
		/// Складывает точку и размер, создавая новую точку.
		/// </summary>
		/// <param name="p">Исходная точка.</param>
		/// <param name="s">Размер для добавления.</param>
		/// <returns>Новая точка с координатами (p.X + s.Width, p.Y + s.Height).</returns>
		public static Point operator +(Point p, Size s) => new Point(p.X + s.Width, p.Y + s.Height);

		/// <summary>
		/// Вычитает размер из точки, создавая новую точку.
		/// </summary>
		/// <param name="p">Исходная точка.</param>
		/// <param name="s">Размер для вычитания.</param>
		/// <returns>Новая точка с координатами (p.X - s.Width, p.Y - s.Height).</returns>
		public static Point operator -(Point p, Size s) => new Point(p.X - s.Width, p.Y - s.Height);

		/// <summary>
		/// Проверяет равенство двух точек
		/// </summary>
		public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;

		/// <summary>
		/// Проверяет неравенство двух точек
		/// </summary>
		public static bool operator !=(Point a, Point b) => !(a == b);

		/// <inheritdoc/>
		public override string ToString() => $"{X}:{Y}";

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is Point point && this == point;

		/// <inheritdoc/>
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

		/// <inheritdoc/>
		public bool Equals(Point other) => this == other;
	}
}
