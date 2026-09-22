using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IBrushFactory"/>. Создаёт кисти из абстрактного цвета.
	/// </summary>
	public sealed class GdiBrushFactory : IBrushFactory
	{
		/// <summary>
		/// Создаёт сплошную кисть заданного цвета.
		/// </summary>
		/// <param name="color">Абстрактный цвет из <see cref="DisplayNodes.Core.Color"/>.</param>
		/// <returns>Экземпляр <see cref="GdiBrush"/>, содержащий <see cref="SolidBrush"/>.</returns>
		public IBrush CreateSolidBrush(DisplayNodes.Core.Color color)
		{
			var gdiColor = Color.FromArgb(color.A, color.R, color.G, color.B);
			return new GdiBrush(new SolidBrush(gdiColor));
		}
	}
}