using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using DisplayNodes.WinFormsAdapter.Component.Masks;
using System;
using System.Drawing;

namespace DisplayNodes.WinFormsAdapter.Components.Masks
{
    internal sealed class PathMask : MaskBase
    {
        public Func<Rect, IGraphicsPath> PathBuilder { get; }

        public PathMask(Func<Rect, IGraphicsPath> pathBuilder)
        {
            PathBuilder = pathBuilder;
        }

        protected override Region CreateRegion()
        {
            if (PathBuilder == null)
                return BuildControlRegion();

            var rect = new Rect(0, 0, Width, Height);
            var abstractPath = PathBuilder(rect);
            var gdiPath = abstractPath.ToGdi();

            if (gdiPath == null)
                return BuildControlRegion();

            return new Region(gdiPath);
        }

        private Region BuildControlRegion() => new Region(new Rectangle(0, 0, Width, Height));
    }
}
