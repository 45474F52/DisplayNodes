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

using System;
using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-реализация <see cref="IBrushFactory"/>. Создаёт кисти из абстрактного цвета.
    /// </summary>
    public sealed class GdiBrushFactory : IBrushFactory
    {
        /// <inheritdoc/>
        public IBrush CreateSolidBrush(Core.Color color)
        {
            var gdiColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            return new GdiBrush(new SolidBrush(gdiColor));
        }

        /// <inheritdoc/>
        public IBrush CreateLinearGradient(
            Core.Point start,
            Core.Point end,
            params Core.GradientStop[] stops)
        {
            ValidateStops(stops);
            return new GdiLinearGradientBrush(start, end, (Core.GradientStop[])stops.Clone());
        }

        /// <inheritdoc/>
        public IBrush CreateRadialGradient(
            Core.Point center,
            Core.Percent radius,
            params Core.GradientStop[] stops)
        {
            ValidateStops(stops);
            return new GdiRadialGradientBrush(center, radius, (Core.GradientStop[])stops.Clone());
        }

        private static void ValidateStops(Core.GradientStop[] stops)
        {
            if (stops == null)
                throw new ArgumentNullException(nameof(stops));
            if (stops.Length < 2)
                throw new ArgumentException("At least two gradient stops are required.", nameof(stops));
        }
    }
}