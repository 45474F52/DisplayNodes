using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
	/// <summary>Fluent-расширения для настройки <see cref="ImageNode"/>.</summary>
	public static class ImageNodeFluent
	{
		/// <summary>Устанавливает режим отображения изображения.</summary>
		public static ImageNode SizeMode(this ImageNode node, ImageSizeMode mode)
		{
			node.Component.SizeMode = mode;
			return node;
		}

		/// <summary>Заменяет исходное изображение.</summary>
		public static ImageNode Image(this ImageNode node, IImage image)
		{
			node.Component.Image = image;
			return node;
		}
	}
}