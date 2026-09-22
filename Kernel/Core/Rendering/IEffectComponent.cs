namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Контракт для компонентов, поддерживающих эффекты обработки изображения.
    /// </summary>
    public interface IEffectComponent
    {
        /// <summary>Прозрачность</summary>
        double Opacity { get; set; }

        /// <summary>Яркость</summary>
        double Brightness { get; set; }

        /// <summary>Контраст</summary>
        double Contrast { get; set; }
    }
}
