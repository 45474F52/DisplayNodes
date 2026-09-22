namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Режим растягивания текста в <see cref="ILabelComponent"/>.
	/// Определяет, как текст масштабируется относительно границ метки.
	/// </summary>
	public enum LabelStretch
	{
		/// <summary>Без растягивания. Текст отображается в исходном размере.</summary>
		None,

		/// <summary>Растягивание только по горизонтали.</summary>
		Horizontal,

		/// <summary>Растягивание только по вертикали.</summary>
		Vertical,

		/// <summary>Растягивание по обеим осям.</summary>
		Full
	}
}