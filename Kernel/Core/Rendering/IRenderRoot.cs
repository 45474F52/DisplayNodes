using System;

namespace DisplayNodes.Core.Rendering
{
    /// <summary>Управляет жизненным циклом корневого контейнера.</summary>
    public interface IRenderRoot : IDisposable
    {
        /// <summary>Корневой layout-компонент.</summary>
        ILayoutComponent Root { get; }
        /// <summary>Пересобирает UI-дерево. Автоматически удаляет предыдущий корень.</summary>
        void Build(LayoutNode node, Point location, Size size);
        /// <summary>Удаляет корневой компонент без создания нового.</summary>
        void Clear();
    }
}