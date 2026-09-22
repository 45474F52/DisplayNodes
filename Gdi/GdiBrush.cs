using System;
using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IBrush"/>. Обёртка над <see cref="SolidBrush"/>.
	/// </summary>
	public sealed class GdiBrush : IBrush
	{
		/// <summary>Внутренний объект GDI+ <see cref="SolidBrush"/>.</summary>
		public SolidBrush Inner { get; }

		/// <summary>
		/// Создаёт обёртку над кистью GDI+.
		/// </summary>
		/// <param name="brush">Исходная кисть GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="brush"/> равен null.</exception>
		public GdiBrush(SolidBrush brush)
		{
			Inner = brush ?? throw new ArgumentNullException(nameof(brush));
		}
	}
}