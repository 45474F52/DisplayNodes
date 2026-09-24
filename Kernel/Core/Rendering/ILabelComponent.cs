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

using DisplayNodes.Core;

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт компонента текстовой метки.
	/// </summary>
	public interface ILabelComponent : IRenderComponent, IEffectComponent
	{
		/// <summary>Отображаемый текст.</summary>
		string Text { get; set; }

		/// <summary>
		/// Шрифт текста.
		/// </summary>
		IFont Font { get; set; }

		/// <summary>
		/// Кисть для цвета текста.
		/// </summary>
		IBrush ForegroundBrush { get; set; }

		/// <summary>
		/// Кисть для цвета фона метки.
		/// </summary>
		IBrush BackgroundBrush { get; set; }

		/// <summary>
		/// Форматирование текста (выравнивание, направление и т.д.).
		/// </summary>
		ITextFormat Format { get; set; }

		/// <summary>
		/// Режим mnemonics ('&amp;' как префикс мнемоники). В адаптерах также может
		/// выбирать способ позиционирования текста (например, в WinForms true
		/// эмулирует системный рендер с верхним рядом, false включает точное
		/// двумерное выравнивание). Поддержка опциональна: компоненты без такого
		/// режима могут бросать <see cref="NotSupportedException"/>.
		/// </summary>
		bool UseMnemonic { get; set; }

		/// <summary>
		/// Внутренние отступы текстовой области метки. Фон и границы компонента
		/// от padding НЕ уменьшаются — текст просто рисуется внутри прямоугольника,
		/// сдвинутого на величину отступов (аналог Padding у Label в WPF).
		/// <para>Значение по умолчанию — нулевые отступы. Компоненты, не умеющие
		/// смещать текст, могут бросать <see cref="NotSupportedException"/> в сеттере.</para>
		/// </summary>
		Thickness Padding { get; set; }
	}
}