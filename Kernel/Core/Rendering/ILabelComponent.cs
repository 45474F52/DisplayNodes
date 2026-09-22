namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт компонента текстовой метки.
	/// </summary>
	public interface ILabelComponent : IRenderComponent, IEffectComponent
	{
		/// <summary>Отображаемый текст.</summary>
		string Text { get; set; }

		/// <summary>
		/// Шрифт текста.
		/// </summary>
		IFont Font { get; set; }

		/// <summary>
		/// Кисть для цвета текста.
		/// </summary>
		IBrush ForegroundBrush { get; set; }

		/// <summary>
		/// Кисть для цвета фона метки.
		/// </summary>
		IBrush BackgroundBrush { get; set; }

		/// <summary>
		/// Форматирование текста (выравнивание, направление и т.д.).
		/// </summary>
		ITextFormat Format { get; set; }
	}
}