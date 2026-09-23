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
	/// GDI-реализация <see cref="ITextFormat"/>. Обёртка над <see cref="StringFormat"/>.
	/// </summary>
	public sealed class GdiTextFormat : ITextFormat
	{
		/// <summary>Внутренний объект GDI+ <see cref="StringFormat"/>.</summary>
		public StringFormat Inner { get; }

		/// <summary>
		/// Создаёт обёртку над форматом строки GDI+.
		/// </summary>
		/// <param name="format">Исходный формат строки GDI+. Не может быть null.</param>
		/// <exception cref="ArgumentNullException">Если <paramref name="format"/> равен null.</exception>
		public GdiTextFormat(StringFormat format)
		{
			Inner = format ?? throw new ArgumentNullException(nameof(format));
		}
	}
}