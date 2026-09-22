namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Контракт для компонентов с расширенным управлением текстом.
    /// </summary>
    public interface ITextLayoutComponent
    {
        /// <summary>Способ отрисовки текста.</summary>
        TextDrawMethod DrawMethod { get; set; }

        /// <summary>Режим растягивания текста.</summary>
        LabelStretch Stretch { get; set; }
    }
}
