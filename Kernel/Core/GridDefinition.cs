using System;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Тип единицы измерения для GridLength.
	/// </summary>
	public enum GridUnitType
	{
		/// <summary>Фиксированный размер в пикселях.</summary>
		Pixel,
		/// <summary>Размер по содержимому.</summary>
		Auto,
		/// <summary>Пропорционально свободному месту.</summary>
		Star,
	}

	/// <summary>
	/// Определяет длину строки или колонки в Grid.
	/// </summary>
	public readonly struct GridLength : IEquatable<GridLength>
	{
		/// <summary>
		/// Значение длины.
		/// </summary>
		public readonly double Value;

		/// <summary>
		/// Тип единицы измерения.
		/// </summary>
		public readonly GridUnitType UnitType;

		/// <summary>
		/// Создаёт новый GridLength.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="type">Тип единицы измерения.</param>
		/// <exception cref="ArgumentOutOfRangeException">Значение меньше 0.</exception>
		public GridLength(double value, GridUnitType type)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value));

			this.Value = value;
			this.UnitType = type;
		}

		/// <summary>
		/// Проверяет, является ли тип Absolute (Pixel).
		/// </summary>
		public bool IsAbsolute => UnitType == GridUnitType.Pixel;

		/// <summary>
		/// Проверяет, является ли тип Auto.
		/// </summary>
		public bool IsAuto => UnitType == GridUnitType.Auto;

		/// <summary>
		/// Проверяет, является ли тип Star.
		/// </summary>
		public bool IsStar => UnitType == GridUnitType.Star;

		/// <summary>
		/// Создаёт GridLength с фиксированным размером в пикселях.
		/// </summary>
		/// <param name="value">Количество пикселей.</param>
		/// <returns>GridLength с типом Pixel.</returns>
		public static GridLength Pixels(double value) => new GridLength(value, GridUnitType.Pixel);

		/// <summary>
		/// Создаёт GridLength с типом Auto.
		/// </summary>
		/// <returns>GridLength с типом Auto.</returns>
		public static GridLength Auto => new GridLength(0d, GridUnitType.Auto);

		/// <summary>
		/// Создаёт GridLength с типом Star.
		/// </summary>
		/// <param name="value">Коэффициент пропорциональности (по умолчанию 1).</param>
		/// <returns>GridLength с типом Star.</returns>
		public static GridLength Star(double value = 1d) => new GridLength(value, GridUnitType.Star);

		/// <summary>
		/// Проверяет равенство двух GridLength.
		/// </summary>
		public static bool operator ==(GridLength a, GridLength b) => a.Value == b.Value && a.UnitType == b.UnitType;

		/// <summary>
		/// Проверяет неравенство двух GridLength.
		/// </summary>
		public static bool operator !=(GridLength a, GridLength b) => !(a == b);

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is GridLength gl && this == gl;

		/// <inheritdoc/>
		public override int GetHashCode() => Value.GetHashCode() ^ UnitType.GetHashCode();

		/// <inheritdoc/>
		public bool Equals(GridLength other) => this == other;

		/// <inheritdoc/>
		public override string ToString()
		{
			switch (UnitType)
			{
				case GridUnitType.Auto:
				return "Auto";
				case GridUnitType.Star:
				return Value == 1f ? "*" : $"{Value}*";
				default:
				return Value.ToString();
			}
		}
	}

	/// <summary>
	/// Определение строки в Grid.
	/// </summary>
	public class RowDefinition
	{
		/// <summary>
		/// Инициализирует определение строки в Grid.
		/// </summary>
		/// <param name="height">Высота строки</param>
		public RowDefinition(GridLength height) => Height = height;

		/// <summary>
		/// Высота строки.
		/// </summary>
		public GridLength Height { get; set; } = GridLength.Star(1);
	}

	/// <summary>
	/// Определение колонки в Grid.
	/// </summary>
	public class ColumnDefinition
	{
		/// <summary>
		/// Инициализирует определение колонки в Grid.
		/// </summary>
		/// <param name="width">Ширина колонки</param>
		public ColumnDefinition(GridLength width) => Width = width;

		/// <summary>
		/// Ширина колонки.
		/// </summary>
		public GridLength Width { get; set; } = GridLength.Star(1);
	}
}
