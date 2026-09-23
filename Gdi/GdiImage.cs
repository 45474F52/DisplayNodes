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

using System.Drawing;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
	/// <summary>
	/// GDI-реализация <see cref="IImage"/>. Обёртка над <see cref="Bitmap"/>.
	/// </summary>
	public sealed class GdiImage : IImage
	{
		/// <summary>Внутренний объект GDI+ <see cref="Bitmap"/>.</summary>
		public Bitmap Inner { get; }

		/// <summary>Ширина изображения в пикселях. Возвращает 0, если изображение null.</summary>
		public int Width => Inner?.Width ?? 0;

		/// <summary>Высота изображения в пикселях. Возвращает 0, если изображение null.</summary>
		public int Height => Inner?.Height ?? 0;

		/// <summary>
		/// Создаёт обёртку над растровым изображением GDI+.
		/// </summary>
		/// <param name="bitmap">Исходное изображение GDI+. Может быть null.</param>
		public GdiImage(Bitmap bitmap)
		{
			Inner = bitmap;
		}
	}
}