using DisplayNodes.WinFormsAdapter.Component.Masks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DisplayNodes.WinFormsAdapter.Components.Masks
{
    internal sealed class EllipseMask : MaskBase
    {
        protected override Region CreateRegion()
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, Width, Height);
                return new Region(path);
            }
        }
    }
}
