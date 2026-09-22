namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт компонента изображения.
	/// </summary>
	public interface IImageComponent : IRenderComponent, IEffectComponent
	{
		/// <summary>
		/// Отображаемое изображение.
		/// </summary>
		IImage Image { get; set; }

		/// <summary>Режим отображения изображения (оригинальный размер или растягивание).</summary>
		ImageSizeMode SizeMode { get; set; }
	}
}