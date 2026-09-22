using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using DisplayNodes.WinFormsAdapter.Components;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Реализация <see cref="IRenderRoot"/> для WinForms.
    /// </summary>
    internal sealed class RenderRoot : IRenderRoot
    {
        private readonly Control _parent;
        private Layout _rootAdapter;

        public RenderRoot(Control parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ILayoutComponent Root => _rootAdapter;

        public void Build(LayoutNode node, Point location, Size size)
        {
            Clear();

            var panel = new Panel
            {
                Location = new System.Drawing.Point(location.X, location.Y),
                Size = new System.Drawing.Size(size.Width, size.Height),
                BackColor = System.Drawing.Color.Transparent,
            };

            _rootAdapter = new Layout(panel);
            node.Apply(_rootAdapter, location, size);
            _parent.Controls.Add(panel);
        }

        public void Clear()
        {
            if (_rootAdapter == null)
                return;

            _parent.Controls.Remove((Panel)_rootAdapter.Inner);
            _rootAdapter.Dispose();
            _rootAdapter = null;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
