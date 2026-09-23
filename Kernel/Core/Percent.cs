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
    /// Процентное значение в диапазоне [0..100].
    /// </summary>
    /// <remarks>
    /// Используется для задания координат градиента, позиций стопов и радиусов.<br/>
    /// Значение проверяется при создании: <see cref="ArgumentOutOfRangeException"/>
    /// при выходе за пределы диапазона.
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("{Value}%")]
    public readonly struct Percent : IEquatable<Percent>
    {
        /// <summary>
        /// Числовое значение в процентах (0..100).
        /// </summary>
        public readonly int Value;

        /// <summary>
		/// Создаёт процентное значение.
		/// </summary>
		/// <param name="value">Значение от 0 до 100.</param>
		/// <exception cref="ArgumentOutOfRangeException">Если <paramref name="value"/> вне диапазона [0..100].</exception>
        public Percent(int value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Percent must be in range [0..100].");

            Value = value;
        }

        /// <summary>0%</summary>
        public static Percent Zero => new Percent(0);

        /// <summary>25%</summary>
        public static Percent Quarter => new Percent(25);

        /// <summary>50%</summary>`
        public static Percent Half => new Percent(50);

        /// <summary>100%</summary>
        public static Percent Hundred => new Percent(100);

        /*
         Неявные операторы удобны для fluent-стиля:
            new GradientStop(Color.Red, 0)     // 0 → Percent(0)
            new GradientStop(Color.Blue, 100)  // 100 → Percent(100)
         */

        /// <summary>
        /// Неявное преобразование из <see cref="int"/> в <see cref="Percent"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Если значение вне диапазона.</exception>
        public static implicit operator Percent(int value) => new Percent(value);

        /// <summary>
        /// Неявное преобразование из <see cref="Percent"/> в <see cref="int"/>.
        /// </summary>
        public static implicit operator int(Percent p) => p.Value;

        /// <inheritdoc/>
        public bool Equals(Percent other) => Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Percent p && Equals(p);

        /// <inheritdoc/>
        public override int GetHashCode() => Value;

        /// <summary>Проверяет равенство значений.</summary>
        public static bool operator ==(Percent a, Percent b) => a.Value == b.Value;

        /// <summary>Проверяет неравенство значений.</summary>
        public static bool operator !=(Percent a, Percent b) => a.Value != b.Value;

        /// <inheritdoc/>
        public override string ToString() => Value + "%";
    }
}
