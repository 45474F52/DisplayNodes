using System;
using System.Collections.Generic;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Helpers
{
    /// <summary>
    /// Методы расширения для рекурсивного обхода и управления жизненным циклом деревьев <see cref="LayoutNode"/>.
    /// </summary>
    public static class LayoutNodeExtensions
    {
        /// <summary>
        /// Рекурсивно собирает все <see cref="IRenderComponent"/> в данном дереве узлов.
        /// </summary>
        public static void CollectComponents(this LayoutNode node, ICollection<IRenderComponent> list)
        {
            if (node is WidgetNode w)
                list.Add(w.Component);
            else if (node is ClipNode clip)
                list.Add(clip.Mask);

            foreach (var child in node.Children)
                child.CollectComponents(list);
        }

        /// <summary>
        /// Рекурсивно освобождает ресурсы всех узлов в дереве (в порядке «снизу вверх»).
        /// </summary>
        public static void DisposeTree(this LayoutNode node)
        {
            foreach (var child in node.Children)
                child.DisposeTree();
            if (node is IDisposable d)
                d.Dispose();
        }
    }
}