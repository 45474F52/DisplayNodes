namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Режим отображения изображения в <see cref="IImageComponent"/>.
	/// </summary>
	public enum ImageSizeMode
	{
		/// <summary>Отображение в исходном размере. Изображение центрируется в границах компонента.</summary>
		Normal,

		/// <summary>Растягивание изображения по границам компонента с сохранением пропорций.</summary>
		Stretch
	}
}