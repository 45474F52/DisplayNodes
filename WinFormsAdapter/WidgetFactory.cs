using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter.Components;
using DisplayNodes.WinFormsAdapter.Components.Masks;
using System;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Реализация <see cref="IWidgetFactory"/>. Создаёт обёртки над WinForms-контролами.
    /// </summary>
    internal sealed class WidgetFactory : IWidgetFactory
    {
        public ILabelComponent CreateLabel() => new Label();

        public IImageComponent CreateImage() => new Image();

        public IMaskComponent CreateRectMask() => new RectMask();

        public IMaskComponent CreateCircleMask() => new CircleMask();

        public IMaskComponent CreateEllipseMask() => new EllipseMask();

        public IMaskComponent CreateRoundedRectMask(float cornerRadius)
            => new RoundedRectMask(cornerRadius);

        public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
            => new PathMask(pathBuilder);
    }
}
