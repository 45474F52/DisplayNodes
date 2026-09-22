using System;
using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="ITextFormat"/>. Обёртка над <see cref="StringFormat"/>.
	/// </summary>
	public sealed class GdiTextFormat : ITextFormat
	{
		/// <summary>Внутренний объект GDI+ <see cref="StringFormat"/>.</summary>
		public StringFormat Inner { get; }

		/// <summary>
		/// Создаёт обёртку над форматом строки GDI+.
		/// </summary>
		/// <param name="format">Исходный формат строки GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="format"/> равен null.</exception>
		public GdiTextFormat(StringFormat format)
		{
			Inner = format ?? throw new ArgumentNullException(nameof(format));
		}
	}
}