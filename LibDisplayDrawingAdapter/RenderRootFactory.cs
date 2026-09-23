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

using DisplayNodes.Core.Rendering;

using LibDisplayDrawing;

namespace DisplayNodes.LibDisplayDrawingAdapter
{
	/// <summary>
	/// Реализация <see cref="IRenderRootFactory"/>. Инкапсулирует знание о parent-компоненте
	/// и <see cref="ManagerTimers"/>.
	/// </summary>
	public sealed class RenderRootFactory : IRenderRootFactory
	{
		private readonly IComponent _parent;
		private readonly ManagerTimers _managerTimers;

		/// <summary>Создаёт фабрику корневых контейнеров.</summary>
		/// <param name="parent">Родительский компонент (из легаси).</param>
		/// <param name="managerTimers">Менеджер таймеров для безопасного Dispose.</param>
		public RenderRootFactory(IComponent parent, ManagerTimers managerTimers)
		{
			_parent = parent ?? throw new ArgumentNullException(nameof(parent));
			_managerTimers = managerTimers ?? throw new ArgumentNullException(nameof(managerTimers));
		}

		/// <inheritdoc/>
		public IRenderRoot Create() => new RenderRoot(_parent, _managerTimers);
	}
}