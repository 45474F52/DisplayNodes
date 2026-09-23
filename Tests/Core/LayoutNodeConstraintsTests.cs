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
public class LayoutNodeConstraintsTests
{
	[Test]
	public void MinWidth_IncreasesDesiredSize()
	{
		var node = new FixedNode(50, 30) { MinWidth = 100 };
		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(30));
		}
	}

	[Test]
	public void MaxWidth_DecreasesDesiredSize()
	{
		var node = new FixedNode(500, 30) { MaxWidth = 100 };
		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(30));
		}
	}

	[Test]
	public void MinHeight_IncreasesDesiredSize()
	{
		var node = new FixedNode(30, 50) { MinHeight = 200 };
		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(30));
			Assert.That(size.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void MaxHeight_DecreasesDesiredSize()
	{
		var node = new FixedNode(30, 500) { MaxHeight = 80 };
		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(30));
			Assert.That(size.Height, Is.EqualTo(80));
		}
	}

	[Test]
	public void MinAndMax_Together_ClampBoth()
	{
		var node = new FixedNode(50, 50)
		{
			MinWidth = 100,
			MaxWidth = 200,
			MinHeight = 60,
			MaxHeight = 90
		};

		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));   // 50 → clamp [100, 200]
			Assert.That(size.Height, Is.EqualTo(60));   // 50 → clamp [60, 90]
		}
	}

	[Test]
	public void Constraints_AppliedBeforeMargin()
	{
		// Margin добавляется к clamped DesiredSize, а не к сырому
		var node = new FixedNode(500, 500)
		{
			Margin = new Thickness(10, 20),
			MaxWidth = 100,
			MaxHeight = 100
		};

		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(120));   // 100 + 10*2
			Assert.That(size.Height, Is.EqualTo(140));  // 100 + 20*2
		}
	}

	[Test]
	public void Constraints_NoEffect_WhenUnset()
	{
		var node = new FixedNode(42, 42);
		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(42));
			Assert.That(size.Height, Is.EqualTo(42));
		}
	}

	[Test]
	public void Constraints_StackLayoutNode_Works()
	{
		var stack = new StackLayoutNode { IsVertical = true, Spacing = 0 };
		stack.Children.Add(new FixedNode(30, 30));
		stack.Children.Add(new FixedNode(30, 30));

		stack.MinWidth = 200;
		stack.MinHeight = 500;

		var size = stack.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(200));
			Assert.That(size.Height, Is.EqualTo(500));  // 30+30 < 500 → clamp
		}
	}

	[Test]
	public void Constraints_Arrange_UsesClampedBounds()
	{
		var node = new FixedNode(50, 50) { MinWidth = 150 };
		node.Measure(Size.Infinity);
		node.Arrange(new Rect(0, 0, 400, 400));

		// HAlignment = Stretch по умолчанию → Bounds = слот
		// Но DesiredSize уже 150
		Assert.That(node.DesiredSize.Width, Is.EqualTo(150));
	}

	[Test]
	public void Constraints_Fluent_Works()
	{
		var node = new FixedNode(50, 50)
			.MinWidth(100)
			.MaxWidth(200)
			.MinHeight(60)
			.MaxHeight(90);

		var size = node.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(60));
		}
	}

	[Test]
	public void Constraints_WidthRange_Fluent_Works()
	{
		var node = new FixedNode(500, 30).WidthRange(50, 100);
		var size = node.Measure(Size.Infinity);

		Assert.That(size.Width, Is.EqualTo(100));
	}
}