using DisplayNodes.Fluent;
using DisplayNodes.Gdi;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Точка входа для инициализации DisplayNodes с бэкендом WinForms.
    /// </summary>
    public static class Adapter
    {
        /// <summary>
        /// Инициализирует UI-фабрики WinForms-реализациями.
        /// Должен быть вызван один раз перед использованием UI.*
        /// </summary>
        public static void Initialize()
        {
            UI.Factory = new WidgetFactory();
            UI.Measurer = new GdiTextMeasurer();
            UI.BrushFactory = new GdiBrushFactory();
            UI.FontFactory = new GdiFontFactory();
            UI.ImageFactory = new GdiImageFactory();
        }
    }
}
