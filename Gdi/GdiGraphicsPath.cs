using System;
using System.Drawing.Drawing2D;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IGraphicsPath"/>. Обёртка над <see cref="GraphicsPath"/>.
	/// </summary>
	public sealed class GdiGraphicsPath : IGraphicsPath
	{
		/// <summary>Внутренний объект GDI+ <see cref="GraphicsPath"/>.</summary>
		public GraphicsPath Inner { get; }

		/// <summary>
		/// Создаёт обёртку над графическим путём GDI+.
		/// </summary>
		/// <param name="path">Исходный путь GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="path"/> равен null.</exception>
		public GdiGraphicsPath(GraphicsPath path)
		{
			Inner = path ?? throw new ArgumentNullException(nameof(path));
		}
	}
}