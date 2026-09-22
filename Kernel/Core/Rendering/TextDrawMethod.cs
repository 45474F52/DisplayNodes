namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Способ отрисовки текста в <see cref="ILabelComponent"/>.
	/// Определяет поведение масштабирования и переноса текста.
	/// </summary>
	public enum TextDrawMethod
	{
		/// <summary>Обычная отрисовка без масштабирования.</summary>
		Normal,

		/// <summary>Автомасштабирование под размер текста.</summary>
		AutoSizeAccordingToText,

		/// <summary>Автомасштабирование по прямоугольнику без переноса строк.</summary>
		AutoFitInConstantRectangleWithoutWrap,

		/// <summary>Автоматический перенос строк в пределах прямоугольника.</summary>
		AutoWrapInConstantRectangle,

		/// <summary>Автомасштабирование с переносом строк в пределах прямоугольника.</summary>
		AutoFitInConstantRectangleWithWrap
	}
}