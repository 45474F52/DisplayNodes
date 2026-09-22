using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IImage"/>. Обёртка над <see cref="Bitmap"/>.
	/// </summary>
	public sealed class GdiImage : IImage
	{
		/// <summary>Внутренний объект GDI+ <see cref="Bitmap"/>.</summary>
		public Bitmap Inner { get; }

		/// <summary>Ширина изображения в пикселях. Возвращает 0, если изображение null.</summary>
		public int Width => Inner?.Width ?? 0;

		/// <summary>Высота изображения в пикселях. Возвращает 0, если изображение null.</summary>
		public int Height => Inner?.Height ?? 0;

		/// <summary>
		/// Создаёт обёртку над растровым изображением GDI+.
		/// </summary>
		/// <param name="bitmap">Исходное изображение GDI+. Может быть null.</param>
		public GdiImage(Bitmap bitmap)
		{
			Inner = bitmap;
		}
	}
}