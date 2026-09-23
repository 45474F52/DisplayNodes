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
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using DisplayNodes.LibDisplayDrawingAdapter.Components;

using LibDisplayDrawing;

namespace DisplayNodes.LibDisplayDrawingAdapter
{
	/// <summary>
	/// Реализация <see cref="IRenderRoot"/>. Инкапсулирует workaround для бага <c>LibDisplayDrawing</c>.
	/// </summary>
	/// <remarks>
	/// <para><see cref="Graphic2D.Dispose"/> в легаси вызывает <c>managerTimers.Remove(timer)</c>
	/// без проверки на null. Этот класс инициализирует <c>managerTimers</c> через
	/// <see cref="Layout.AttachManagerTimers"/> перед любым вызовом Dispose.</para>
	/// </remarks>
	internal sealed class RenderRoot : IRenderRoot
	{
		private Layout _rootAdapter;
		private readonly IComponent _parent;
		private readonly ManagerTimers _managerTimers;

		/// <summary>Создаёт корневой контейнер.</summary>
		/// <param name="parent">Родительский компонент (из легаси).</param>
		/// <param name="managerTimers">Менеджер таймеров для безопасного Dispose.</param>
		public RenderRoot(IComponent parent, ManagerTimers managerTimers)
		{
			_parent = parent ?? throw new ArgumentNullException(nameof(parent));
			_managerTimers = managerTimers ?? throw new ArgumentNullException(nameof(managerTimers));
		}

		/// <inheritdoc/>
		public ILayoutComponent Root => _rootAdapter;

		/// <inheritdoc/>
		public void Build(LayoutNode node, Point location, Size size)
		{
			Clear();

			var layout = new Layout2D(
				_parent,
				new Point2D(location.X, location.Y),
				new Size2D(size.Width, size.Height)
			);

			_rootAdapter = new Layout(layout);
			_rootAdapter.AttachManagerTimers(_managerTimers);

			node.Apply(_rootAdapter, location, size);
			_parent.Add(layout);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			if (_rootAdapter == null)
				return;

			_parent.Remove((Layout2D)_rootAdapter.Inner);
			_rootAdapter.Dispose();
			_rootAdapter = null;
		}

		/// <inheritdoc/>
		public void Dispose() => Clear();
	}
}