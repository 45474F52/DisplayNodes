using System;
using System.Collections.Generic;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;

namespace DisplayNodes.Widgets
{
    /// <summary>
    /// Базовый класс для виджетов — листовых узлов, оборачивающих <see cref="IRenderComponent"/>.
    /// </summary>
    /// <remarks>
    /// <para>Виджет не имеет собственных детей (хотя технически <see cref="LayoutNode.Children"/>
    /// доступен, контейнеры его не используют). На этапе Arrange вызывает
    /// <see cref="ApplyBounds"/>, который наследники переопределяют для записи
    /// вычисленных <see cref="LayoutNode.Bounds"/> в компонент.</para>
    /// <para>Реализует <see cref="IDisposable"/> для освобождения ресурсов компонента.
    /// Вызывается автоматически при <see cref="LayoutNodeFluent.Dispose(LayoutNode)"/>
    /// и <see cref="LayoutNodeFluent.Rebuild(LayoutNode, IRenderComponent, Point, Size)"/>.</para>
    /// </remarks>
    public abstract class WidgetNode : LayoutNode, IDisposable
    {
        private bool _disposed;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        /// <summary>Оборачиваемый компонент отображения.</summary>
        public IRenderComponent Component { get; }

        /// <summary>Создаёт виджет с заданным компонентом.</summary>
        /// <param name="component">Компонент, который будет добавлен в дерево отображения при Apply.</param>
        /// <exception cref="ArgumentNullException"><paramref name="component"/> is null.</exception>
        protected WidgetNode(IRenderComponent component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            HAlignment = Alignment.Start;
            VAlignment = Alignment.Start;
        }

        /// <summary>
        /// Добавляет подписку, которая будет автоматически отписана при Dispose.
        /// </summary>
        /// <param name="subscription">Подписка для отписки при удалении виджета.</param>
        /// <returns>Переданная подписка (для удобства chaining).</returns>
        public IDisposable AddSubscription(IDisposable subscription)
        {
            if (subscription != null)
            {
                lock (_subscriptions)
                    _subscriptions.Add(subscription);
            }
            return subscription;
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect) => ApplyBounds();

        /// <summary>
        /// Записывает вычисленные <see cref="LayoutNode.Bounds"/> в свойства компонента
        /// (Location, Size и т.д.).
        /// </summary>
        protected abstract void ApplyBounds();

        /// <summary>Освобождает ресурсы компонента и отписывается от всех Observable.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            lock (_subscriptions)
            {
                foreach (var s in _subscriptions)
                    s?.Dispose();
                _subscriptions.Clear();
            }
        }
    }
}