namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Абстрактное представление растрового изображения.<br/>
	/// Конкретная реализация создаётся бэкендом.
	/// </summary>
	public interface IImage
	{
		/// <summary>Ширина изображения в пикселях.</summary>
		int Width { get; }

		/// <summary>Высота изображения в пикселях.</summary>
		int Height { get; }
	}
}