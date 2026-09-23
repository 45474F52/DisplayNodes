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

using DisplayNodes.Core;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class UniformGridNodeTests
{
	[Test]
	public void UniformGrid_CalculatesCellSizeByMaxChild()
	{
		var grid = new UniformGridNode(rows: 2, columns: 2, spacing: 5);
		grid.Children.Add(new FixedNode(30, 40));
		grid.Children.Add(new FixedNode(50, 20));
		grid.Children.Add(new FixedNode(20, 60));
		grid.Children.Add(new FixedNode(40, 30));

		var size = grid.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			// Max child: 50x60. Cell = 50x60.
			// Width: 2*50 + 5 = 105
			// Height: 2*60 + 5 = 125
			Assert.That(size.Width, Is.EqualTo(105));
			Assert.That(size.Height, Is.EqualTo(125));
		}
	}

	[Test]
	public void UniformGrid_Arrange_PlacesChildrenCorrectly()
	{
		var grid = new UniformGridNode(rows: 2, columns: 2, spacing: 10);
		grid.Children.Add(new FixedNode(30, 30));
		grid.Children.Add(new FixedNode(30, 30));
		grid.Children.Add(new FixedNode(30, 30));
		grid.Children.Add(new FixedNode(30, 30));

		_ = grid.Measure(new Size(200, 200));
		grid.Arrange(new Rect(0, 0, 200, 200));

		using (Assert.EnterMultipleScope())
		{
			// Cell: (200-10)/2 = 95
			Assert.That(grid.Children[0].Bounds.X, Is.EqualTo(0));
			Assert.That(grid.Children[0].Bounds.Y, Is.EqualTo(0));

			Assert.That(grid.Children[1].Bounds.X, Is.EqualTo(105)); // 95 + 10
			Assert.That(grid.Children[1].Bounds.Y, Is.EqualTo(0));

			Assert.That(grid.Children[2].Bounds.X, Is.EqualTo(0));
			Assert.That(grid.Children[2].Bounds.Y, Is.EqualTo(105));
		}
	}

	[Test]
	public void UniformGrid_MinRowsColumns_IsOne()
	{
		var grid = new UniformGridNode(rows: 0, columns: 0);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(grid.Rows, Is.EqualTo(1));
			Assert.That(grid.Columns, Is.EqualTo(1));
		}
	}

	[Test]
	public void UniformGrid_WithPadding_IncludesPadding()
	{
		var grid = new UniformGridNode(rows: 1, columns: 1)
		{
			Padding = new Thickness(10, 20)
		};
		grid.Children.Add(new FixedNode(50, 50));

		var size = grid.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(70));   // 50 + 20
			Assert.That(size.Height, Is.EqualTo(90));  // 50 + 40
		}
	}
}