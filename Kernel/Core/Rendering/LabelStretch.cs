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

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Режим растягивания текста в <see cref="ILabelComponent"/>.
	/// Определяет, как текст масштабируется относительно границ метки.
	/// </summary>
	public enum LabelStretch
	{
		/// <summary>Без растягивания. Текст отображается в исходном размере.</summary>
		None,

		/// <summary>Растягивание только по горизонтали.</summary>
		Horizontal,

		/// <summary>Растягивание только по вертикали.</summary>
		Vertical,

		/// <summary>Растягивание по обеим осям.</summary>
		Full
	}
}