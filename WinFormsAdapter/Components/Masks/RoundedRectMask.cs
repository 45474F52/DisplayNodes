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
            // Радиус не должен превращаться в ноль: AddArc с нулевым размером дуги — ArgumentException.
            float r = Math.Max(0.5f, Math.Min(CornerRadius, Math.Min(Width, Height) / 2f));
            if (r >= Math.Min(Width, Height) / 2f)
            {
                // Вырожденный случай (высота <= 2*радиуса): рисуем эллипс целиком, без дуг с d >= стороны.
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, Width, Height);
                    return new Region(path);
                }
            }

            float d = r * 2;
            var rect = new RectangleF(0, 0, Width, Height);

            using (var path = new GraphicsPath())
            {
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
