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
	/// Узел, который растягивается вдоль главной оси.
	/// </summary>
	public sealed class StretchNode : LayoutNode
	{
		/// <summary>
		/// Натуральная ширина узла
		/// </summary>
		public int NaturalWidth { get; }

		/// <summary>
		/// Натуральная высота узла
		/// </summary>
		public int NaturalHeight { get; }

		/// <summary>
		/// Создаёт растягиваемый узел
		/// </summary>
		/// <param name="naturalWidth">Натуральная ширина</param>
		/// <param name="naturalHeight">Натуральная высота</param>
		public StretchNode(int naturalWidth = 0, int naturalHeight = 0)
		{
			NaturalWidth = naturalWidth;
			NaturalHeight = naturalHeight;
			HAlignment = Alignment.Stretch;
			VAlignment = Alignment.Stretch;
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available) => new Size(NaturalWidth, NaturalHeight);

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect) { }
	}
}
