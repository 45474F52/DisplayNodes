using System;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Fluent
{
    /// <summary>
    /// Управляет жизненным циклом корневого контейнера.
    /// </summary>
    public sealed class DisplayRoot : IDisposable
    {
        private readonly IRenderRoot _root;

        /// <summary>Инициализирует <see cref="DisplayRoot"/> через фабрику рендер-корня.</summary>
        /// <param name="factory">
        /// Фабрика корневого контейнера. Инкапсулирует знание о parent-компоненте и специфике бэкенда.
        /// </param>
        public DisplayRoot(IRenderRootFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _root = factory.Create();
        }

        /// <summary>Корневой компонент.</summary>
        public ILayoutComponent Root => _root.Root;

        /// <summary>Пересобирает UI-дерево. Автоматически удаляет предыдущий корень.</summary>
        public void Build(LayoutNode node, Point location, Size size)
        {
            _root.Clear();
            _root.Build(node, location, size);
        }

        /// <summary>Освобождает корневой компонент.</summary>
        public void Dispose() => _root.Dispose();
    }
}