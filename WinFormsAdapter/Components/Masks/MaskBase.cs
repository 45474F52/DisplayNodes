using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter.Components;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter.Component.Masks
{
	internal class MaskBase : Control, IMaskComponent
	{
		private IRenderComponent _parentAdapter;

		IRenderComponent IRenderComponent.Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				Parent = ComponentHelper.ExtractInner(value);
			}
        }

        Point IRenderComponent.Location
        {
            get => new Point(Location.X, Location.Y);
            set => Location = new System.Drawing.Point(value.X, value.Y);
        }

        Size IRenderComponent.Size
        {
            get => new Size(Size.Width, Size.Height);
            set => Size = new System.Drawing.Size(value.Width, value.Height);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected virtual System.Drawing.Region CreateRegion()
        {
            return new System.Drawing.Region(new System.Drawing.Rectangle(0, 0, Width, Height));
        }

        private void UpdateRegion()
        {
            Region?.Dispose();
            Region = CreateRegion();
        }
    }
}
