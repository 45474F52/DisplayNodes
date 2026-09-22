using DisplayNodes.Core.Rendering;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter.Components
{
    /// <summary>
    /// Адаптер корневого layout-контейнера. Обёртка над <see cref="Panel"/>.
    /// </summary>
	internal sealed class Layout : ComponentBase, ILayoutComponent, IDisposable
	{
        private readonly Panel _panel;

        public Layout(Panel panel) : base(panel)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
        }

        public void Refresh() => _panel.Invalidate();

        public void Dispose()
        {
            _panel.Dispose();
        }
    }
}
