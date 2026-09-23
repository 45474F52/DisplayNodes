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
using System.Diagnostics;

namespace DisplayNodes.Core
{
	/// <summary>Структура, представляющая прямоугольник.</summary>
	[Serializable]
	[DebuggerDisplay("({X}:{Y}, {Width}x{Height})")]
	public readonly struct Rect : IEquatable<Rect>
	{
		/// <summary>Позиция по X</summary>
		public readonly int X;

		/// <summary>Позиция по Y</summary>
		public readonly int Y;

		/// <summary>Ширина</summary>
		public readonly int Width;

		/// <summary>Высота</summary>
		public readonly int Height;

		/// <summary>Создаёт прямоугольник с заданной позицией и размером.</summary>
		public Rect(int x, int y, int width, int height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		/// <summary>Создаёт прямоугольник с заданной позицией и размером.</summary>
		public Rect(Point point, Size size) : this(point.X, point.Y, size.Width, size.Height) { }

		/// <summary>Пустой прямоугольник (0,0,0,0).</summary>
		public static Rect Empty => new Rect(0, 0, 0, 0);

		/// <summary>Левый верхний угол.</summary>
		public Point Point => new Point(X, Y);

		/// <summary>Размер прямоугольника.</summary>
		public Size Size => new Size(Width, Height);

		/// <summary>Правая граница (X + Width).</summary>
		public int Right => X + Width;

		/// <summary>Нижняя граница (Y + Height).</summary>
		public int Bottom => Y + Height;

		/// <summary>True, если ширина или высота &lt;= 0.</summary>
		public bool IsEmpty => Width <= 0 || Height <= 0;

		/// <summary>
		/// Уменьшает прямоугольник на заданные отступы во все стороны.
		/// </summary>
		/// <param name="thickness">Отступы, на которые уменьшается прямоугольник.</param>
		/// <returns>
		/// Новый прямоугольник, уменьшенный на <see cref="Thickness.Left"/>, <see cref="Thickness.Top"/>,
		/// <see cref="Thickness.Right"/> и <see cref="Thickness.Bottom"/>.
		/// </returns>
		public Rect Deflate(Thickness thickness) => new Rect(
			X + thickness.Left, Y + thickness.Top,
			Math.Max(0, Width - thickness.Left - thickness.Right),
			Math.Max(0, Height - thickness.Top - thickness.Bottom)
		);

		/// <summary>
		/// Расширяет прямоугольник на заданные отступы во все стороны.
		/// </summary>
		/// <param name="thickness">Отступы, на которые расширяется прямоугольник.</param>
		/// <returns>
		/// Новый прямоугольник, расширенный на <see cref="Thickness.Left"/>, <see cref="Thickness.Top"/>,
		/// <see cref="Thickness.Right"/> и <see cref="Thickness.Bottom"/>.
		/// </returns>
		public Rect Inflate(Thickness thickness) => new Rect(
			X - thickness.Left,
			Y - thickness.Top,
			Width + thickness.Horizontal,
			Height + thickness.Vertical
		);

		/// <summary>
		/// Проверяет, содержит ли прямоугольник заданную точку.
		/// </summary>
		/// <param name="point">Точка для проверки.</param>
		/// <returns>
		/// <c>true</c>, если точка находится внутри прямоугольника (включая левую и верхнюю границы,
		/// но исключая правую и нижнюю); иначе <c>false</c>.
		/// </returns>
		public bool Contains(Point point) =>
			point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;

		/// <summary>
		/// Проверяет, содержит ли данный прямоугольник другой прямоугольник полностью.
		/// </summary>
		/// <param name="rect">Прямоугольник для проверки.</param>
		/// <returns>
		/// <c>true</c>, если <paramref name="rect"/> полностью находится внутри данного прямоугольника;
		/// иначе <c>false</c>.
		/// </returns>
		public bool Contains(Rect rect) =>
			rect.X >= X && rect.Right <= Right && rect.Y >= Y && rect.Bottom <= Bottom;

		/// <summary>
		/// Проверяет, пересекается ли данный прямоугольник с другим.
		/// </summary>
		/// <param name="rect">Прямоугольник для проверки пересечения.</param>
		/// <returns>
		/// <c>true</c>, если прямоугольники имеют общую область (даже если это только граница);
		/// иначе <c>false</c>.
		/// </returns>
		public bool IntersectsWith(Rect rect) =>
			rect.X < Right && rect.Right > X && rect.Y < Bottom && rect.Bottom > Y;

		/// <summary>
		/// Возвращает прямоугольник, представляющий область пересечения данного прямоугольника с другим.
		/// </summary>
		/// <param name="rect">Прямоугольник для пересечения.</param>
		/// <returns>
		/// Прямоугольник, представляющий область пересечения.
		/// Если прямоугольники не пересекаются, возвращается <see cref="Rect.Empty"/>.
		/// </returns>
		public Rect Intersect(Rect rect)
		{
			int x = Math.Max(this.X, rect.X);
			int y = Math.Max(this.Y, rect.Y);
			int w = Math.Min(this.Right, rect.Right) - x;
			int h = Math.Min(this.Bottom, rect.Bottom) - y;
			return (w > 0 && h > 0) ? new Rect(x, y, w, h) : Empty;
		}

		/// <summary>
		/// Проверяет равенство двух прямоугольников
		/// </summary>
		public static bool operator ==(Rect a, Rect b) => a.Point == b.Point && a.Size == b.Size;

		/// <summary>
		/// Проверяет неравенство двух прямоугольников
		/// </summary>
		public static bool operator !=(Rect a, Rect b) => !(a == b);

		/// <inheritdoc/>
		public override string ToString() => $"{Point} {Size}";

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is Rect rect && this == rect;

		/// <inheritdoc/>
		public override int GetHashCode() => Point.GetHashCode() ^ Size.GetHashCode();

		/// <inheritdoc/>
		public bool Equals(Rect other) => this == other;
	}
}
