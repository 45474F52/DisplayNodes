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

using DisplayNodes.Fluent;

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт компонента-контейнера, поддерживающего принудительное обновление отрисовки.
	/// </summary>
	/// <remarks>
	/// <para>Реализуется адаптерами рендерера для корневых контейнеров.
	/// Используется классом <see cref="DisplayRoot"/> для управления жизненным циклом UI-дерева.</para>
	/// <para>В отличие от базового <see cref="IRenderComponent"/>, добавляет возможность
	/// сигнализировать рендереру о необходимости перерисовки содержимого.</para>
	/// </remarks>
	public interface ILayoutComponent : IRenderComponent
	{
		/// <summary>
		/// Запрашивает принудительное обновление отрисовки контейнера и его содержимого.
		/// </summary>
		/// <remarks>
		/// Вызов метода не гарантирует мгновенную перерисовку.
		/// </remarks>
		void Refresh();
	}
}