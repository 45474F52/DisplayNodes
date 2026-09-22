using DisplayNodes.Core.Rendering;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Фабрика <see cref="IRenderRoot"/> для WinForms.
    /// </summary>
    public sealed class RenderRootFactory : IRenderRootFactory
    {
        private readonly Control _parent;

        /// <summary>Создаёт фабрику корневых контейнеров.</summary>
        /// <param name="parent">Родительский компонент.</param>
        public RenderRootFactory(Control parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <inheritdoc/>
        public IRenderRoot Create() => new RenderRoot(_parent);
    }
}
