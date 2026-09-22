using DisplayNodes.WinFormsAdapter.Component.Masks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DisplayNodes.WinFormsAdapter.Components.Masks
{
    internal sealed class CircleMask : MaskBase
    {
        protected override Region CreateRegion()
        {
            using (var path = new GraphicsPath())
            {
                float diameter = System.Math.Min(Width, Height);
                float x = (Width - diameter) / 2;
                float y = (Height - diameter) / 2;
                path.AddEllipse(x, y, diameter, diameter);
                return new Region(path);
            }
        }
    }
}
