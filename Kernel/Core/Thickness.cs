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
	/// <summary>Структура, представляющая отступы с четырёх сторон.</summary>
	[Serializable]
	[DebuggerDisplay("(L={Left} T={Top} R={Right} B={Bottom})")]
	public readonly struct Thickness : IEquatable<Thickness>
	{
		/// <summary>Отступ слева</summary>
		public readonly int Left;

		/// <summary>Отступ сверху</summary>
		public readonly int Top;

		/// <summary>Отступ справа</summary>
		public readonly int Right;

		/// <summary>Отступ снизу</summary>
		public readonly int Bottom;

		/// <summary>Создаёт равномерные отступы со всех сторон.</summary>
		/// <param name="uniform">Размер отступа для всех сторон.</param>
		public Thickness(int uniform) => Left = Top = Right = Bottom = uniform;

		/// <summary>Создаёт отступы: по горизонтали и по вертикали.</summary>
		/// <param name="h">Горизонтальный отступ (Left и Right).</param>
		/// <param name="v">Вертикальный отступ (Top и Bottom).</param>
		public Thickness(int h, int v)
		{
			Left = Right = h;
			Top = Bottom = v;
		}

		/// <summary>Создаёт отступы для каждой стороны отдельно.</summary>
		/// <param name="l">Отступ слева.</param>
		/// <param name="t">Отступ сверху.</param>
		/// <param name="r">Отступ справа.</param>
		/// <param name="b">Отступ снизу.</param>
		public Thickness(int l, int t, int r, int b)
		{
			Left = l;
			Top = t;
			Right = r;
			Bottom = b;
		}

		/// <summary>Нулевые отступы (0, 0, 0, 0).</summary>
		public static Thickness Zero => default;

		/// <summary>Сумма горизонтальных отступов (Left + Right).</summary>
		public int Horizontal => Left + Right;

		/// <summary>Сумма вертикальных отступов (Top + Bottom).</summary>
		public int Vertical => Top + Bottom;

		/// <summary>True, если все отступы равны нулю.</summary>
		public bool IsZero => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;

		/// <summary>Проверяет равенство двух значений <see cref="Thickness"/>.</summary>
		public static bool operator ==(Thickness a, Thickness b)
			=> a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;

		/// <summary>Проверяет неравенство двух значений <see cref="Thickness"/>.</summary>
		public static bool operator !=(Thickness a, Thickness b) => !(a == b);

		/// <summary>
		/// Складывает два набора отступов, создавая новый.
		/// </summary>
		/// <param name="a">Первый набор отступов.</param>
		/// <param name="b">Второй набор отступов.</param>
		/// <returns>
		/// Новый <see cref="Thickness"/>, где каждая сторона равна сумме соответствующих сторон
		/// <paramref name="a"/> и <paramref name="b"/>.
		/// </returns>
		public static Thickness operator +(Thickness a, Thickness b) =>
			new Thickness(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);

		/// <inheritdoc/>
		public override string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is Thickness thickness && this == thickness;

		/// <inheritdoc/>
		public override int GetHashCode()
			=> Left.GetHashCode() ^ Top.GetHashCode() ^ Right.GetHashCode() ^ Bottom.GetHashCode();

		/// <inheritdoc/>
		public bool Equals(Thickness other) => this == other;
	}
}