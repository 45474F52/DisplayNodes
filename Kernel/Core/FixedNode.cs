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

namespace DisplayNodes.Core
{
	/// <summary>
	/// Узел с фиксированным размером. Не зависит от доступного пространства и всегда возвращает заданный размер.
	/// Полезен для создания распорок (Spacer) или разделителей.
	/// </summary>
	public class FixedNode : LayoutNode
	{
		/// <summary>Фиксированный размер узла.</summary>
		public Size FixedSize { get; }

		/// <summary>Создает узел с заданной шириной и высотой.</summary>
		public FixedNode(int width, int height)
		{
			FixedSize = new Size(width, height);
            HAlignment = Alignment.Start;
            VAlignment = Alignment.Start;
        }

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect) { }

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available) => FixedSize;
	}
}