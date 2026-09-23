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

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Фабрика кистей. Абстрагирует создание кистей от конкретного бэкенда.
	/// </summary>
	public interface IBrushFactory
	{
		/// <summary>Создаёт сплошную кисть заданного цвета.</summary>
		IBrush CreateSolidBrush(Color color);

		/// <summary>
		/// Создаёт линейный градиент.
		/// </summary>
		/// <param name="start">
		/// Начальная точка градиента в нормализованных координатах (0..100).
		/// <c>Point(0, 0)</c> — левый верхний угол, <c>Point(100, 100)</c> — правый нижний.
		/// </param>
		/// <param name="end">Конечная точка градиента в нормализованных координатах (0..100).</param>
		/// <param name="stops">
		/// Стопы градиента. Должно быть минимум два стопа.
		/// </param>
		/// <exception cref="System.ArgumentNullException">Если <paramref name="stops"/> равен <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Если стопов меньше двух.</exception>
		IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops);

        /// <summary>
        /// Создаёт радиальный градиент.
        /// </summary>
        /// <param name="center">Центр градиента в нормализованных координатах (0..100).</param>
        /// <param name="radius">Радиус в процентах от <c>min(width, height)</c> (0..100).</param>
        /// <param name="stops">Стопы градиента. Должно быть минимум два стопа.</param>
        /// <exception cref="System.ArgumentNullException">Если <paramref name="stops"/> равен <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Если стопов меньше двух.</exception>
        IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops);
    }
}