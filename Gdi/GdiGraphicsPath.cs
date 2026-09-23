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
using System.Drawing.Drawing2D;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IGraphicsPath"/>. Обёртка над <see cref="GraphicsPath"/>.
	/// </summary>
	public sealed class GdiGraphicsPath : IGraphicsPath
	{
		/// <summary>Внутренний объект GDI+ <see cref="GraphicsPath"/>.</summary>
		public GraphicsPath Inner { get; }

		/// <summary>
		/// Создаёт обёртку над графическим путём GDI+.
		/// </summary>
		/// <param name="path">Исходный путь GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="path"/> равен null.</exception>
		public GdiGraphicsPath(GraphicsPath path)
		{
			Inner = path ?? throw new ArgumentNullException(nameof(path));
		}
	}
}