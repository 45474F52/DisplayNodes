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

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Фабрика компонентов рендерера. Создаёт компоненты без родителя — Parent устанавливается позже.
	/// </summary>
	/// <remarks>
	/// Реализации фабрики должны клонировать ресурсы при установке в компонент, 
	/// чтобы consumer-код сохранял владение оригиналами.
	/// </remarks>
	public interface IWidgetFactory
	{
		/// <summary>Создаёт компонент текстовой метки.</summary>
		ILabelComponent CreateLabel();

		/// <summary>Создаёт компонент изображения.</summary>
		IImageComponent CreateImage();

		/// <summary>Создаёт прямоугольную маску.</summary>
		IMaskComponent CreateRectMask();

		/// <summary>Создаёт круговую маску.</summary>
		IMaskComponent CreateCircleMask();

		/// <summary>Создаёт эллиптическую маску.</summary>
		IMaskComponent CreateEllipseMask();

		/// <summary>Создаёт маску со скруглёнными углами.</summary>
		IMaskComponent CreateRoundedRectMask(float cornerRadius);

		/// <summary>Создаёт маску с произвольной формой.</summary>
		IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder);
	}
}