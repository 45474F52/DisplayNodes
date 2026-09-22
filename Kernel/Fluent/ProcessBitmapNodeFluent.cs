using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
    /// <summary>
    /// Fluent-расширения для настройки эффектов обработки изображений.<br/>
    /// Применяются к любым виджетам, чей компонент реализует <see cref="IEffectComponent"/>.
    /// </summary>
    public static class ProcessBitmapNodeFluent
    {
        /// <summary>Установить прозрачность (0–100).</summary>
        public static T Opacity<T>(this T node, double opacity) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Opacity = opacity;
            return node;
        }

        /// <summary>Установить яркость (-100…100).</summary>
        public static T Brightness<T>(this T node, double brightness) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Brightness = brightness;
            return node;
        }

        /// <summary>Установить контраст (-100…100).</summary>
        public static T Contrast<T>(this T node, double contrast) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Contrast = contrast;
            return node;
        }
    }
}