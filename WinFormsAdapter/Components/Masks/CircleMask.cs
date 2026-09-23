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
