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

using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
	/// <summary>Fluent-расширения для настройки <see cref="ImageNode"/>.</summary>
	public static class ImageNodeFluent
	{
		/// <summary>Устанавливает режим отображения изображения.</summary>
		public static ImageNode SizeMode(this ImageNode node, ImageSizeMode mode)
		{
			node.Component.SizeMode = mode;
			return node;
		}

		/// <summary>Заменяет исходное изображение.</summary>
		public static ImageNode Image(this ImageNode node, IImage image)
		{
			node.Component.Image = image;
			return node;
		}
	}
}