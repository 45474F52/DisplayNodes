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
	/// <summary>
	/// Структура, представляющая цвет в формате RGBA.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("({R},{G},{B}. A={A})")]
	public readonly struct Color : IEquatable<Color>
	{
		/// <summary>Красный цветовой канал</summary>
		public byte R { get; }

		/// <summary>Зелёный цветовой канал</summary>
		public byte G { get; }

		/// <summary>Синий цветовой канал</summary>
		public byte B { get; }

		/// <summary>Альфа канал (непрозрачность)</summary>
		public byte A { get; }

		/// <summary>
		/// Создаёт структуру цвета
		/// </summary>
		/// <param name="r">Красный</param>
		/// <param name="g">Зелёный</param>
		/// <param name="b">Синий</param>
		/// <param name="a">Альфа</param>
		public Color(byte r, byte g, byte b, byte a = 255)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		/// <summary>
		/// Создаёт структуру цвета с учётом альфа-канала
		/// </summary>
		/// <param name="alpha">Непрозрачность</param>
		/// <param name="red">Красный</param>
		/// <param name="green">Зелёный</param>
		/// <param name="blue">Синий</param>
		/// <returns>Возвращает структуру цвета с переданными цветовыми каналами</returns>
		public static Color FromArgb(int alpha, int red, int green, int blue)
			=> new Color((byte)red, (byte)green, (byte)blue, (byte)alpha);

		/// <summary>
		/// Создаёт структуру цвета без учёта альфа-канала
		/// </summary>
		/// <param name="red">Красный</param>
		/// <param name="green">Зелёный</param>
		/// <param name="blue">Синий</param>
		/// <remarks>
		/// Значение альфа-канала равно 255 (максимум непрозрачности)
		/// </remarks>
		/// <returns>Возвращает структуру цвета с переданными цветовыми каналами</returns>
		public static Color FromRgb(int red, int green, int blue)
			=> new Color((byte)red, (byte)green, (byte)blue, 255);

		/// <summary>Прозрачный цвет</summary>
		public static Color Transparent => new Color(0, 0, 0, 0);

		/// <summary>Чёрный цвет</summary>
		public static Color Black => new Color(0, 0, 0, 255);

		/// <summary>Белый цвет</summary>
		public static Color White => new Color(255, 255, 255, 255);

		/// <summary>Красный цвет</summary>
		public static Color Red => new Color(255, 0, 0, 255);

		/// <summary>Зелёный цвет</summary>
		public static Color Green => new Color(0, 128, 0, 255);

		/// <summary>Синий цвет</summary>
		public static Color Blue => new Color(0, 0, 255, 255);

		/// <inheritdoc/>
		public override string ToString() => $"({R},{G},{B}. A={A})";

		/// <inheritdoc/>
		public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is Color color && Equals(color);

		/// <inheritdoc/>
		public override int GetHashCode() => R ^ G ^ B ^ A;

		/// <summary>Проверяет равенство каналов двух цветов</summary>
		public static bool operator ==(Color left, Color right) => left.Equals(right);

		/// <summary>Проверяет неравенство каналов двух цветов</summary>
		public static bool operator !=(Color left, Color right) => !(left == right);
	}
}