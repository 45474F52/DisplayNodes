using DisplayNodes.WinFormsAdapter.Component.Masks;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DisplayNodes.WinFormsAdapter.Components.Masks
{
    internal sealed class RoundedRectMask : MaskBase
    {
        public float CornerRadius { get; }

        public RoundedRectMask(float cornerRadius)
        {
            CornerRadius = cornerRadius;
        }

        protected override Region CreateRegion()
        {
            using (var path = new GraphicsPath())
            {
                float r = Math.Min(CornerRadius, Math.Min(Width, Height) / 2);
                float d = r * 2;
                var rect = new RectangleF(0, 0, Width, Height);

                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                return new Region(path);
            }
        }
    }
}
