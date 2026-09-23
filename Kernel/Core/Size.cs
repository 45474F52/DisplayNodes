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
	/// <summary>Структура, представляющая размер в 2D-пространстве.</summary>
	[Serializable]
	[DebuggerDisplay("({Width}x{Height})")]
	public readonly struct Size : IEquatable<Size>
	{
		/// <summary>Ширина.</summary>
		public readonly int Width;

		/// <summary>Высота.</summary>
		public readonly int Height;

		/// <summary>Создаёт размер с заданными шириной и высотой.</summary>
		public Size(int width, int height)
		{
			this.Width = width;
			this.Height = height;
		}

		/// <summary>Пустой размер (0, 0).</summary>
		public static Size Empty => new Size(0, 0);

		/// <summary>Размер с максимально возможными значениями.</summary>
		public static Size Infinity => new Size(int.MaxValue, int.MaxValue);

		/// <summary>Возвращает новый размер с изменённой шириной.</summary>
		/// <param name="w">Новая ширина.</param>
		/// <returns>Новый <see cref="Size"/>.</returns>
		public Size WithWidth(int w) => new Size(w, Height);

		/// <summary>Возвращает новый размер с изменённой высотой.</summary>
		/// <param name="h">Новая высота.</param>
		/// <returns>Новый <see cref="Size"/>.</returns>
		public Size WithHeight(int h) => new Size(Width, h);

		/// <summary>
		/// Возвращает увеличенный на заданный отступ размер <see cref="Size"/>.
		/// </summary>
		/// <param name="t">Отступ для увеличения</param>
		/// <returns>Новый размер (Width + <see cref="Thickness.Horizontal"/>, Height + <see cref="Thickness.Vertical"/>)</returns>
		public Size Inflate(Thickness t)
		{
			if (t.IsZero)
				return this;

			return new Size(Width + t.Horizontal, Height + t.Vertical);
		}

		/// <summary>
		/// Возвращает уменьшенный на заданный отступ размер <see cref="Size"/>.
		/// </summary>
		/// <param name="t">Отступ для уменьшения</param>
		/// <remarks>
		/// Если <c><see cref="Width"/> - <see cref="Thickness.Horizontal"/> &lt; 0</c>, то результат будет 0
		/// </remarks>
		/// <returns>Новый размер (Width - <see cref="Thickness.Horizontal"/>, Height - <see cref="Thickness.Vertical"/>)</returns>
		public Size Deflate(Thickness t)
		{
			if (t.IsZero)
				return this;

			return new Size(Math.Max(0, Width - t.Horizontal), Math.Max(0, Height - t.Vertical));
		}

		/// <summary>
		/// Проверяет равенство двух размеров
		/// </summary>
		public static bool operator ==(Size a, Size b) => a.Width == b.Width && a.Height == b.Height;

		/// <summary>
		/// Проверяет неравенство двух размеров
		/// </summary>
		public static bool operator !=(Size a, Size b) => !(a == b);

		/// <summary>
		/// Складывает два размера, создавая новый размер.
		/// </summary>
		/// <param name="a">Первый размер.</param>
		/// <param name="b">Второй размер.</param>
		/// <returns>Новый размер с шириной (a.Width + b.Width) и высотой (a.Height + b.Height).</returns>
		public static Size operator +(Size a, Size b) => new Size(a.Width + b.Width, a.Height + b.Height);

		/// <summary>
		/// Вычитает один размер из другого, создавая новый размер.
		/// </summary>
		/// <param name="a">Уменьшаемый размер.</param>
		/// <param name="b">Вычитаемый размер.</param>
		/// <returns>Новый размер с шириной (a.Width - b.Width) и высотой (a.Height - b.Height).</returns>
		public static Size operator -(Size a, Size b) => new Size(a.Width - b.Width, a.Height - b.Height);

		/// <inheritdoc/>
		public override string ToString() => $"{Width}x{Height}";

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is Size size && this == size;

		/// <inheritdoc/>
		public override int GetHashCode() => Width.GetHashCode() ^ Height.GetHashCode();

		/// <inheritdoc/>
		public bool Equals(Size other) => this == other;
	}
}
