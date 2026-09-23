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
	/// Контракт для измерения размеров текста.
	/// Абстрагирует механизм измерения от конкретного бэкенда рендеринга.
	/// </summary>
	public interface ITextMeasurer
	{
		/// <summary>
		/// Измеряет площадь, занимаемую текстом при заданных параметрах.
		/// </summary>
		/// <param name="text">Измеряемый текст.</param>
		/// <param name="font">Шрифт, используемый для отрисовки.</param>
		/// <param name="maxWidth">
		/// Максимальная ширина для переноса текста.
		/// Если <c>0</c>, текст измеряется в одну строку без ограничений по ширине.
		/// </param>
		/// <returns>Размер (<see cref="Size"/>), занимаемый текстом.</returns>
		Size MeasureArea(string text, IFont font, int maxWidth = 0);
	}
}