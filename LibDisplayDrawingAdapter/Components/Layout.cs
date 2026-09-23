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

namespace DisplayNodes.LibDisplayDrawingAdapter.Components
{
	/// <summary>
	/// Адаптер для корневого layout-контейнера. Обёртка над <see cref="Layout2D"/>.
	/// </summary>
	/// <remarks>
	/// Используется только для корневого контейнера. <see cref="ManagerTimers"/>
	/// устанавливается через <see cref="AttachManagerTimers"/> для безопасного Dispose.
	/// </remarks>
	internal sealed class Layout : ComponentBase, ILayoutComponent, IDisposable
	{
		private readonly Layout2D _layout;
		private ManagerTimers _managerTimers;

		/// <summary>Создаёт адаптер layout-контейнера.</summary>
		public Layout(Layout2D layout) : base(layout)
		{
			_layout = layout ?? throw new ArgumentNullException(nameof(layout));
		}

		/// <summary>
		/// Устанавливает <see cref="ManagerTimers"/> для безопасного Dispose.
		/// </summary>
		public void AttachManagerTimers(ManagerTimers managerTimers)
		{
			_managerTimers = managerTimers;
			((IGraphic)_layout).SET_ManagerTimers(managerTimers);
		}

		/// <inheritdoc/>
		public void Refresh() => _layout.Refresh();

		/// <summary>
		/// Освобождает ресурсы.
		/// </summary>
		/// <remarks>
		/// Перед Dispose повторно устанавливает ManagerTimers,
		/// чтобы обойти баг Graphic2D.Dispose (NullReferenceException).
		/// </remarks>
		public void Dispose()
		{
			if (_managerTimers != null)
				AttachManagerTimers(_managerTimers);

			((IDisposable)_layout).Dispose();
		}
	}
}