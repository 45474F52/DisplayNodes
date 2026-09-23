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
public class StackLayoutNodeTests
{
	[Test]
	public void VerticalStack_Measure_SumsHeights()
	{
		var stack = new StackLayoutNode { IsVertical = true, Spacing = 10 };
		stack.Children.Add(new FixedNode(100, 50));
		stack.Children.Add(new FixedNode(100, 30));

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(90)); // 50 + 30 + 10
		}
	}

	[Test]
	public void HorizontalStack_Measure_SumsWidths()
	{
		var stack = new StackLayoutNode { IsVertical = false, Spacing = 5 };
		stack.Children.Add(new FixedNode(40, 20));
		stack.Children.Add(new FixedNode(60, 20));

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(105)); // 40 + 60 + 5
			Assert.That(size.Height, Is.EqualTo(20));
		}
	}

	[Test]
	public void Stack_WithPadding_IncludesPaddingInMeasure()
	{
		var stack = new StackLayoutNode
		{
			IsVertical = true,
			Padding = new Thickness(10, 20)
		};
		stack.Children.Add(new FixedNode(50, 30));

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(70));  // 50 + 20
			Assert.That(size.Height, Is.EqualTo(70)); // 30 + 40
		}
	}

	[Test]
	public void Stack_MainAxisAlignment_Center_CentersChildren()
	{
		var stack = new StackLayoutNode
		{
			IsVertical = true,
			MainAxisAlignment = MainAxisAlignment.Center
		};
		stack.Children.Add(new FixedNode(100, 20));

		_ = stack.Measure(new Size(100, 100));
		stack.Arrange(new Rect(0, 0, 100, 100));

		// Free space: 100 - 20 = 80, offset = 40
		Assert.That(stack.Children[0].Bounds.Y, Is.EqualTo(40));
	}

	[Test]
	public void Stack_MainAxisAlignment_SpaceBetween_DistributesGap()
	{
		var stack = new StackLayoutNode
		{
			IsVertical = true,
			MainAxisAlignment = MainAxisAlignment.SpaceBetween
		};
		stack.Children.Add(new FixedNode(100, 20));
		stack.Children.Add(new FixedNode(100, 20));
		stack.Children.Add(new FixedNode(100, 20));

		_ = stack.Measure(new Size(100, 100));
		stack.Arrange(new Rect(0, 0, 100, 100));

		using (Assert.EnterMultipleScope())
		{
			// Free space: 100 - 60 = 40, gap = 40 / 2 = 20
			Assert.That(stack.Children[0].Bounds.Y, Is.EqualTo(0));
			Assert.That(stack.Children[1].Bounds.Y, Is.EqualTo(40));  // 20 + 20
			Assert.That(stack.Children[2].Bounds.Y, Is.EqualTo(80));  // 40 + 20 + 20
		}
	}

	[Test]
	public void Stack_Margin_AddsToTotalSize()
	{
		var stack = new StackLayoutNode { IsVertical = true };
		stack.Margin = new Thickness(10, 20);
		stack.Children.Add(new FixedNode(100, 50));

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(120));  // 100 + 20
			Assert.That(size.Height, Is.EqualTo(90));  // 50 + 40
		}
	}
}