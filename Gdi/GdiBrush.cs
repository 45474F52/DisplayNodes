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
using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IBrush"/>. Обёртка над <see cref="SolidBrush"/>.
	/// </summary>
	public sealed class GdiBrush : IBrush
	{
		/// <summary>Внутренний объект GDI+ <see cref="SolidBrush"/>.</summary>
		public SolidBrush Inner { get; }

		/// <summary>
		/// Создаёт обёртку над кистью GDI+.
		/// </summary>
		/// <param name="brush">Исходная кисть GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="brush"/> равен null.</exception>
		public GdiBrush(SolidBrush brush)
		{
			Inner = brush ?? throw new ArgumentNullException(nameof(brush));
		}
	}
}