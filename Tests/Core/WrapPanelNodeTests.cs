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
public class WrapPanelNodeTests
{
	// ---------------------------------------------------------------
	// Horizontal
	// ---------------------------------------------------------------

	[Test]
	public void Wrap_Horizontal_AllFitInOneRow()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(30, 20));
		panel.Children.Add(new FixedNode(30, 20));
		panel.Children.Add(new FixedNode(30, 20));

		var size = panel.Measure(new Size(200, 100));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(90));
			Assert.That(size.Height, Is.EqualTo(20));
		}
	}

	[Test]
	public void Wrap_Horizontal_WrapsToSecondRow()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(60, 20));
		panel.Children.Add(new FixedNode(60, 20));
		panel.Children.Add(new FixedNode(60, 20));

		// available 150: 60 + 60 + 60 = 180 не влезает → 2 в первой, 1 во второй
		var size = panel.Measure(new Size(150, 500));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(120));   // max(120, 60)
			Assert.That(size.Height, Is.EqualTo(40));   // 20 + 20
		}
	}

	[Test]
	public void Wrap_Horizontal_Arrange_PositionsCorrectly()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal, Spacing = 10 };
		panel.Children.Add(new FixedNode(50, 20));
		panel.Children.Add(new FixedNode(50, 20));
		panel.Children.Add(new FixedNode(50, 20));

		// available 150: 50 + 10 + 50 = 110 ≤ 150, третья не влезает (110+10+50=170)
		_ = panel.Measure(new Size(150, 500));
		panel.Arrange(new Rect(0, 0, 150, 500));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(panel.Children[0].Bounds.X, Is.EqualTo(0));
			Assert.That(panel.Children[0].Bounds.Y, Is.EqualTo(0));

			Assert.That(panel.Children[1].Bounds.X, Is.EqualTo(60));  // 50 + 10
			Assert.That(panel.Children[1].Bounds.Y, Is.EqualTo(0));

			Assert.That(panel.Children[2].Bounds.X, Is.EqualTo(0));
			Assert.That(panel.Children[2].Bounds.Y, Is.EqualTo(20));
		}
	}

	[Test]
	public void Wrap_Horizontal_LineSpacing_Applied()
	{
		var panel = new WrapPanelNode
		{
			Direction = WrapDirection.Horizontal,
			LineSpacing = 5
		};
		panel.Children.Add(new FixedNode(100, 20));
		panel.Children.Add(new FixedNode(100, 20));

		_ = panel.Measure(new Size(150, 500));
		panel.Arrange(new Rect(0, 0, 150, 500));

		// Второй ребёнок на новой строке с LineSpacing=5.
		Assert.That(panel.Children[1].Bounds.Y, Is.EqualTo(25));
	}

	[Test]
	public void Wrap_Horizontal_ChildWiderThanAvailable_Overflows()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(500, 20));

		var size = panel.Measure(new Size(100, 100));

		using (Assert.EnterMultipleScope())
		{
			// Ребёнок шире available — не переносится, переполняет.
			Assert.That(size.Width, Is.EqualTo(500));
			Assert.That(size.Height, Is.EqualTo(20));
		}
	}

	[Test]
	public void Wrap_Horizontal_RowHeightIsMaxChildHeight()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(30, 10));
		panel.Children.Add(new FixedNode(30, 40));   // самый высокий
		panel.Children.Add(new FixedNode(30, 20));

		var size = panel.Measure(new Size(200, 500));

		Assert.That(size.Height, Is.EqualTo(40));
	}

	[Test]
	public void Wrap_Horizontal_WithPadding()
	{
		var panel = new WrapPanelNode
		{
			Direction = WrapDirection.Horizontal,
			Padding = new Thickness(10, 20)
		};
		panel.Children.Add(new FixedNode(30, 30));

		var size = panel.Measure(new Size(200, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(50));   // 30 + 20
			Assert.That(size.Height, Is.EqualTo(70));  // 30 + 40
		}
	}

	// ---------------------------------------------------------------
	// Vertical
	// ---------------------------------------------------------------

	[Test]
	public void Wrap_Vertical_AllFitInOneColumn()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Vertical };
		panel.Children.Add(new FixedNode(20, 30));
		panel.Children.Add(new FixedNode(20, 30));
		panel.Children.Add(new FixedNode(20, 30));

		var size = panel.Measure(new Size(200, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(20));
			Assert.That(size.Height, Is.EqualTo(90));
		}
	}

	[Test]
	public void Wrap_Vertical_WrapsToSecondColumn()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Vertical };
		panel.Children.Add(new FixedNode(20, 60));
		panel.Children.Add(new FixedNode(20, 60));
		panel.Children.Add(new FixedNode(20, 60));

		// available height 150: 60 + 60 = 120, третий (180) не влезает → 2 в первой колонке
		var size = panel.Measure(new Size(500, 150));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(40));   // 20 + 20
			Assert.That(size.Height, Is.EqualTo(120)); // max(120, 60)
		}
	}

	[Test]
	public void Wrap_Vertical_Arrange_PositionsCorrectly()
	{
		var panel = new WrapPanelNode
		{
			Direction = WrapDirection.Vertical,
			Spacing = 10,
			LineSpacing = 5
		};
		panel.Children.Add(new FixedNode(20, 50));
		panel.Children.Add(new FixedNode(20, 50));
		panel.Children.Add(new FixedNode(20, 50));

		// available height 150: 50 + 10 + 50 = 110 ≤ 150, третий (110+10+50=170) не влезает
		_ = panel.Measure(new Size(500, 150));
		panel.Arrange(new Rect(0, 0, 500, 150));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(panel.Children[0].Bounds.X, Is.EqualTo(0));
			Assert.That(panel.Children[0].Bounds.Y, Is.EqualTo(0));

			Assert.That(panel.Children[1].Bounds.X, Is.EqualTo(0));
			Assert.That(panel.Children[1].Bounds.Y, Is.EqualTo(60));  // 50 + 10

			// Третий — новая колонка: X = 20 (ширина колонки) + 5 (LineSpacing) = 25
			Assert.That(panel.Children[2].Bounds.X, Is.EqualTo(25));
			Assert.That(panel.Children[2].Bounds.Y, Is.EqualTo(0));
		}
	}

	[Test]
	public void Wrap_Vertical_ChildTallerThanAvailable_Overflows()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Vertical };
		panel.Children.Add(new FixedNode(20, 500));

		var size = panel.Measure(new Size(100, 100));

		using (Assert.EnterMultipleScope())
		{
			// Ребёнок выше available — не переносится.
			Assert.That(size.Width, Is.EqualTo(20));
			Assert.That(size.Height, Is.EqualTo(500));
		}
	}

	// ---------------------------------------------------------------
	// Граничные случаи
	// ---------------------------------------------------------------

	[Test]
	public void Wrap_EmptyPanel_ReturnsPaddingOnly()
	{
		var panel = new WrapPanelNode
		{
			Direction = WrapDirection.Horizontal,
			Padding = new Thickness(10, 20)
		};

		var size = panel.Measure(new Size(200, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(20));
			Assert.That(size.Height, Is.EqualTo(40));
		}
	}

	[Test]
	public void Wrap_InfiniteWidth_NoWrap()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(100, 20));
		panel.Children.Add(new FixedNode(100, 20));
		panel.Children.Add(new FixedNode(100, 20));

		var size = panel.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(300));
			Assert.That(size.Height, Is.EqualTo(20));
		}
	}

	[Test]
	public void Wrap_Margin_AddsToSize()
	{
		var panel = new WrapPanelNode
		{
			Direction = WrapDirection.Horizontal,
			Margin = new Thickness(10, 20)
		};
		panel.Children.Add(new FixedNode(30, 30));

		var size = panel.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(50));   // 30 + 20
			Assert.That(size.Height, Is.EqualTo(70));  // 30 + 40
		}
	}

	[Test]
	public void Wrap_SingleChild_NoWrap()
	{
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(30, 30));

		var size = panel.Measure(new Size(1000, 1000));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(30));
			Assert.That(size.Height, Is.EqualTo(30));
		}
	}

	[Test]
	public void Wrap_MeasureAndArrange_SameLayout()
	{
		// Проверяем, что Arrange даёт ту же разбивку на строки, что Measure.
		var panel = new WrapPanelNode { Direction = WrapDirection.Horizontal };
		panel.Children.Add(new FixedNode(60, 20));
		panel.Children.Add(new FixedNode(60, 20));
		panel.Children.Add(new FixedNode(60, 20));

		_ = panel.Measure(new Size(150, 500));
		panel.Arrange(new Rect(0, 0, 150, 500));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(panel.Children[0].Bounds.Y, Is.EqualTo(0));
			Assert.That(panel.Children[1].Bounds.Y, Is.EqualTo(0));
			Assert.That(panel.Children[2].Bounds.Y, Is.EqualTo(20));
		}
	}
}