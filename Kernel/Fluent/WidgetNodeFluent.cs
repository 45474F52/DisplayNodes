using System;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
    /// <summary>Fluent-расширения для базовых свойств <see cref="WidgetNode"/>.</summary>
    public static class WidgetNodeFluent
    {
        /// <summary>Привязывает видимость компонента к реактивному источнику.</summary>
        public static T BindVisible<T>(this T node, Observable<bool> source) where T : WidgetNode
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            node.Component.Visible = source.Value;
			_ = node.AddSubscription(source.Subscribe(v => node.Component.Visible = v));
            return node;
        }

        /// <summary>Привязывает прозрачность к реактивному источнику.</summary>
        public static T BindOpacity<T>(this T node, Observable<double> source) where T : WidgetNode
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (node.Component is IEffectComponent p)
            {
                p.Opacity = source.Value;
				_ = node.AddSubscription(source.Subscribe(v => p.Opacity = v));
            }
            return node;
        }

        /// <summary>Привязывает яркость к реактивному источнику.</summary>
        public static T BindBrightness<T>(this T node, Observable<double> source) where T : WidgetNode
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (node.Component is IEffectComponent p)
            {
                p.Brightness = source.Value;
				_ = node.AddSubscription(source.Subscribe(v => p.Brightness = v));
            }
            return node;
        }

        /// <summary>Привязывает контрастность к реактивному источнику.</summary>
        public static T BindContrast<T>(this T node, Observable<double> source) where T : WidgetNode
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (node.Component is IEffectComponent p)
            {
                p.Contrast = source.Value;
				_ = node.AddSubscription(source.Subscribe(v => p.Contrast = v));
            }
            return node;
        }
    }
}