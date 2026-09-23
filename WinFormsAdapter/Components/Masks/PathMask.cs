///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

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
