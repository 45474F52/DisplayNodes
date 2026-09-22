using System.Windows.Forms;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter.Component.Masks;

namespace DisplayNodes.WinFormsAdapter.Components
{
	internal abstract class ComponentBase : IRenderComponent
	{
		public Control Inner { get; }

		protected ComponentBase(Control inner)
		{
			Inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
		}

		private IRenderComponent _parentAdapter;
		public IRenderComponent Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				Inner.Parent = ComponentHelper.ExtractInner(value);
			}
		}

		public Point Location
		{
			get => new Point(Inner.Location.X, Inner.Location.Y);
			set => Inner.Location = new System.Drawing.Point(value.X, value.Y);
		}

		public Size Size
		{
			get => new Size(Inner.Size.Width, Inner.Size.Height);
			set => Inner.Size = new System.Drawing.Size(value.Width, value.Height);
		}

		public bool Visible
		{
			get => Inner.Visible;
			set => Inner.Visible = value;
		}
	}

	/// <summary>Хелпер для извлечения внутреннего <see cref="Control"/> из адаптера.</summary>
	internal static class ComponentHelper
	{
		/// <summary>
		/// Извлекает внутренний <see cref="Control"/> из адаптера.
		/// Возвращает <c>null</c>, если <paramref name="component"/> не является WinForms-адаптером.
		/// </summary>
		public static Control ExtractInner(IRenderComponent component)
		{
			if (component is ComponentBase c)
				return c.Inner;
			if (component is MaskBase m)
				return m;
			return null;
		}
	}
}
