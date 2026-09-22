using System;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Widgets
{
    /// <summary>
    /// Контейнер-маска. Обрезает содержимое по форме заданной маски.<br/>
    /// Дети привязываются к маске, а не к внешнему родителю.
    /// </summary>
    public class ClipNode : LayoutNode, IDisposable
    {
        /// <summary>Маска, ограничивающая область отрисовки.</summary>
        public IMaskComponent Mask { get; }

        private bool _disposed;

        /// <summary>Создаёт узел-маску с заданной формой.</summary>
        /// <param name="mask">Экземпляр маски (реализация <see cref="IMaskComponent"/>).</param>
        public ClipNode(IMaskComponent mask)
        {
            Mask = mask ?? throw new ArgumentNullException(nameof(mask));
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size available)
        {
            int maxWidth = 0, maxHeight = 0;
            foreach (LayoutNode child in Children)
            {
                Size size = child.Measure(available);
                maxWidth = Math.Max(maxWidth, size.Width);
                maxHeight = Math.Max(maxHeight, size.Height);
            }
            return new Size(maxWidth, maxHeight);
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect)
        {
            Mask.Location = finalRect.Point;
            Mask.Size = finalRect.Size;
            foreach (LayoutNode child in Children)
                child.Arrange(finalRect);
        }

        /// <summary>Освобождает ресурсы маски.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            (Mask as IDisposable)?.Dispose();
        }
    }
}