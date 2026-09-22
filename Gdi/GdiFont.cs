using System;
using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IFont"/>. Обёртка над <see cref="Font"/>.
	/// </summary>
	public sealed class GdiFont : IFont
	{
		/// <summary>Внутренний объект GDI+ <see cref="Font"/>.</summary>
		public Font Inner { get; }

		/// <summary>
		/// Создаёт обёртку над шрифтом GDI+.
		/// </summary>
		/// <param name="font">Исходный шрифт GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="font"/> равен null.</exception>
		public GdiFont(Font font)
		{
			Inner = font ?? throw new ArgumentNullException(nameof(font));
		}
	}
}