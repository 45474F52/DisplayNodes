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
    /// Одна точка градиента: цвет и позиция (в процентах от 0 до 100).
    /// </summary>
    /// <remarks>
    /// Используется в <see cref="Rendering.IBrushFactory.CreateLinearGradient"/>
    /// и <see cref="Rendering.IBrushFactory.CreateRadialGradient"/>.
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("({Offset} {Color})")]
    public readonly struct GradientStop
    {
        /// <summary>
        /// Цвет стопа
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// Позиция стопа в процентах
        /// </summary>
        public readonly Percent Offset;

        /// <summary>
        /// Создаёт стоп градиента.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <param name="offset">Позиция в процентах (0..100).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Если <paramref name="offset"/> вне диапазона.</exception>
        public GradientStop(Color color, Percent offset)
        {
            Color = color;
            Offset = offset;
        }

        /// <inheritdoc/>
        public override string ToString() => Offset + " " + Color;
    }
}
