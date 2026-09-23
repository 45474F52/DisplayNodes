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
    /// GDI-описание линейного градиента. Хранит нормализованные координаты и стопы;
    /// конкретный <see cref="System.Drawing.Drawing2D.LinearGradientBrush"/> создаётся
    /// в момент отрисовки с учётом фактического прямоугольника.
    /// </summary>
    /// <remarks>
    /// <b>Ограничение.</b> GDI+ не имеет аналога WPF <c>RelativeToBoundingBox</c>,
    /// поэтому растяжение градиента под размер компонента должен реализовать адаптер.
    /// </remarks>
    public sealed class GdiLinearGradientBrush : IBrush
    {
        /// <summary>Начальная точка (0..100).</summary>
        public Point Start { get; }

        /// <summary>Конечная точка (0..100).</summary>
        public Point End { get; }

        /// <summary>Стопы градиента.</summary>
        public GradientStop[] Stops { get; }

        /// <summary>Создаёт описание линейного градиента.</summary>
        public GdiLinearGradientBrush(Point start, Point end, GradientStop[] stops)
        {
            Start = start;
            End = end;
            Stops = stops;
        }
    }
}