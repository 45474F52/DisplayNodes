namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт для измерения размеров текста.
	/// Абстрагирует механизм измерения от конкретного бэкенда рендеринга.
	/// </summary>
	public interface ITextMeasurer
	{
		/// <summary>
		/// Измеряет площадь, занимаемую текстом при заданных параметрах.
		/// </summary>
		/// <param name="text">Измеряемый текст.</param>
		/// <param name="font">Шрифт, используемый для отрисовки.</param>
		/// <param name="maxWidth">
		/// Максимальная ширина для переноса текста.
		/// Если <c>0</c>, текст измеряется в одну строку без ограничений по ширине.
		/// </param>
		/// <returns>Размер (<see cref="Size"/>), занимаемый текстом.</returns>
		Size MeasureArea(string text, IFont font, int maxWidth = 0);
	}
}