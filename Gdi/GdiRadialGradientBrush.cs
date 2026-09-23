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

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-описание радиального градиента.
    /// </summary>
    public sealed class GdiRadialGradientBrush : IBrush
    {
        /// <summary>Центр (0..100).</summary>
        public Point Center { get; }

        /// <summary>Радиус в процентах (0..100) от <c>min(width, height)</c>.</summary>
        public Percent Radius { get; }

        /// <summary>Стопы градиента.</summary>
        public GradientStop[] Stops { get; }

        /// <summary>Создаёт описание радиального градиента.</summary>
        public GdiRadialGradientBrush(Point center, Percent radius, GradientStop[] stops)
        {
            Center = center;
            Radius = radius;
            Stops = stops;
        }
    }
}