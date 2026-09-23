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
using System.Diagnostics;

namespace DisplayNodes.Core
{
    /// <summary>
    /// Описание тени для визуального эффекта.
    /// </summary>
    /// <remarks>
    /// <para>Тень <b>не участвует в layout</b> — она не занимает место в контейнере
    /// и не влияет на <see cref="LayoutNode.DesiredSize"/>. Она рисуется поверх
    /// или под компонентом в зависимости от реализации адаптера.</para>
    /// <para><b>Ограничение.</b> В текущей версии адаптеры не поддерживают
    /// тень — при попытке её установить бросается <see cref="System.NotSupportedException"/>.
    /// API подготовлен для будущей реализации.</para>
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("Shadow ({OffsetX}:{OffsetY}), R={BlurRadius}, {Color})")]
    public readonly struct Shadow
    {
        /// <summary>
        /// Смещение тени по оси X
        /// </summary>
        public readonly int OffsetX;

        /// <summary>
        /// Смещение тени по оси Y
        /// </summary>
        public readonly int OffsetY;

        /// <summary>
        /// Радиус размытия тени
        /// </summary>
        /// <remarks>
        /// Значение <c>0</c> — резкая тень без размытия
        /// </remarks>
        public readonly int BlurRadius;

        /// <summary>
        /// Цвет тени
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// Создаёт описание тени.
        /// </summary>
        /// <param name="offsetX">Смещение по X.</param>
        /// <param name="offsetY">Смещение по Y.</param>
        /// <param name="blurRadius">Радиус размытия (0 — без размытия).</param>
        /// <param name="color">Цвет тени.</param>
        /// <exception cref="ArgumentOutOfRangeException">Если <paramref name="blurRadius"/> отрицательный.</exception>
        public Shadow(int offsetX, int offsetY, int blurRadius, Color color)
        {
            if (blurRadius < 0)
                throw new ArgumentOutOfRangeException(nameof(blurRadius), "BlurRadius must be >= 0");

            OffsetX = offsetX;
            OffsetY = offsetY;
            BlurRadius = blurRadius;
            Color = color;
        }

        /// <summary>
        /// Создаёт тень со стандартным полупрозрачным чёрным цветом.
        /// </summary>
        public Shadow(int offsetX, int offsetY, int blurRadius)
            : this(offsetX, offsetY, blurRadius, new Color(0, 0, 0, 80))
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"Shadow ({OffsetX}:{OffsetY}), R={BlurRadius}, {Color})";
    }
}
