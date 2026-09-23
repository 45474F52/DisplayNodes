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
using DisplayNodes.Fluent;

using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class StackLayoutFlexTests
{
	[Test]
	public void Flex_SingleChild_StretchesToFill()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(100, 200));
		stack.Arrange(new Rect(0, 0, 100, 200));

		Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(200));
	}

	[Test]
	public void Flex_TwoChildren_DistributesProportionally()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 0).Flex(1));
		stack.Children.Add(new StretchNode(0, 0).Flex(2));

		_ = stack.Measure(new Size(100, 300));
		stack.Arrange(new Rect(0, 0, 100, 300));

		// 1:2 → 100 и 200
		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(100));
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void Flex_FixedAndFlexible_FixedKeepsSize()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new FixedNode(100, 50));                 // fixed
		stack.Children.Add(new StretchNode(0, 0).Flex(1));          // забирает остаток

		_ = stack.Measure(new Size(100, 300));
		stack.Arrange(new Rect(0, 0, 100, 300));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(50));
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(250));
		}
	}

	[Test]
	public void Flex_WithSpacing_SpacingConsidered()
	{
		var stack = new StackLayoutNode { IsVertical = true, Spacing = 20 };
		stack.Children.Add(new FixedNode(100, 50));
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(100, 300));
		stack.Arrange(new Rect(0, 0, 100, 300));

		// 50 fixed + 20 spacing + 230 flex = 300
		Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(230));
	}

	[Test]
	public void Flex_Horizontal_Direction_Works()
	{
		var stack = new StackLayoutNode { IsVertical = false };
		stack.Children.Add(new FixedNode(50, 100));
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(300, 100));
		stack.Arrange(new Rect(0, 0, 300, 100));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Width, Is.EqualTo(50));
			Assert.That(stack.Children[1].Bounds.Width, Is.EqualTo(250));
		}
	}

	[Test]
	public void Flex_WeightZero_BehavesLikeFixed()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 50).Flex(0));
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(100, 200));
		stack.Arrange(new Rect(0, 0, 100, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(50));
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(150));
		}
	}

	[Test]
	public void Flex_WithMainAxisAlignment_Center_FlexTakesFreeSpaceFirst()
	{
		// При наличии flex-ребёнка free = 0 после распределения, поэтому Center не влияет
		var stack = new StackLayoutNode
		{
			IsVertical = true,
			MainAxisAlignment = MainAxisAlignment.Center
		};
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(100, 200));
		stack.Arrange(new Rect(0, 0, 100, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Y, Is.EqualTo(0));
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void Flex_Fluent_Works()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new FixedNode(100, 50));
		stack.Children.Add(new StretchNode(0, 0).Flex(2));

		_ = stack.Measure(new Size(100, 250));
		stack.Arrange(new Rect(0, 0, 100, 250));

		Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(200));
	}

	// ---------------------------------------------------------------
	// Сжатие (free < 0)
	// ---------------------------------------------------------------

	[Test]
	public void Flex_FreeNegative_ShrinksFlexOnly()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new FixedNode(100, 100));                 // fixed — не сжимается
		stack.Children.Add(new StretchNode(0, 100).Flex(1));         // flex — сжимается

		// natural: 100 + 100 = 200. available: 150. deficit: 50.
		_ = stack.Measure(new Size(100, 150));
		stack.Arrange(new Rect(0, 0, 100, 150));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(100));  // не тронут
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(50));   // 100 - 50
		}
	}

	[Test]
	public void Flex_FreeNegative_TwoFlexChildren_ShrinkProportionally()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 100).Flex(1));
		stack.Children.Add(new StretchNode(0, 200).Flex(2));

		// natural: 100 + 200 = 300. available: 150. deficit: 150.
		// Веса 1:2 → сжатие: 50 и 100. Итого: 50 и 100.
		_ = stack.Measure(new Size(100, 150));
		stack.Arrange(new Rect(0, 0, 100, 150));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(50));
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(100));
		}
	}

	[Test]
	public void Flex_FreeNegative_ShrinkStopsAtZero()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new FixedNode(100, 200));
		stack.Children.Add(new StretchNode(0, 20).Flex(1));

		// natural: 200 + 20 = 220. available: 100. deficit: 120.
		// flex-ребёнок natural=20, но deficit=120 → clamp(20 - 120, 0) = 0.
		_ = stack.Measure(new Size(100, 100));
		stack.Arrange(new Rect(0, 0, 100, 100));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stack.Children[0].Bounds.Height, Is.EqualTo(200));  // fixed не сжимается
			Assert.That(stack.Children[1].Bounds.Height, Is.EqualTo(0));    // clamp(20-120, 0)
		}
	}

	// ---------------------------------------------------------------
	// Отсутствие flex и граничные случаи
	// ---------------------------------------------------------------

	[Test]
	public void Flex_NoFlexChildren_BehavesAsBefore()
	{
		var stack = new StackLayoutNode { IsVertical = true, Spacing = 10 };
		stack.Children.Add(new FixedNode(100, 30));
		stack.Children.Add(new FixedNode(100, 40));

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Height, Is.EqualTo(80));  // 30 + 10 + 40
			Assert.That(size.Width, Is.EqualTo(100));
		}
	}

	[Test]
	public void Flex_InfiniteSpace_NoStretch()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 50).Flex(1));

		var size = stack.Measure(Size.Infinity);

		// При бесконечном available flex не растягивается
		Assert.That(size.Height, Is.EqualTo(50));
	}

	[Test]
	public void Flex_MeasureTwice_StableResult()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new FixedNode(100, 50));
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(100, 300));
		int firstHeight = stack.Children[1].DesiredSize.Height;

		_ = stack.Measure(new Size(100, 300));
		int secondHeight = stack.Children[1].DesiredSize.Height;

		// DesiredSize flex-ребёнка не меняется — растяжение применяется в Arrange.
		using (Assert.EnterMultipleScope())
		{
			Assert.That(firstHeight, Is.EqualTo(0));
			Assert.That(secondHeight, Is.EqualTo(0));
		}
	}

	[Test]
	public void Flex_BoundsWidth_AlwaysFillsSlot()
	{
		// В вертикальном стеке ширина ребёнка растягивается на всю ширину слота,
		// вне зависимости от flex (по HAlignment=Stretch у StretchNode).
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Children.Add(new StretchNode(0, 0).Flex(1));

		_ = stack.Measure(new Size(150, 100));
		stack.Arrange(new Rect(0, 0, 150, 100));

		Assert.That(stack.Children[0].Bounds.Width, Is.EqualTo(150));
	}
}