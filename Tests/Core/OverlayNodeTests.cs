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
public class OverlayNodeTests
{
	[Test]
	public void Overlay_Measure_ReturnsMaxChildSize()
	{
		var overlay = new OverlayNode();
		overlay.Children.Add(new FixedNode(100, 50));
		overlay.Children.Add(new FixedNode(80, 120));
		overlay.Children.Add(new FixedNode(60, 40));

		var size = overlay.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(120));
		}
	}

	[Test]
	public void Overlay_Arrange_GivesSameSlotToAllChildren()
	{
		var overlay = new OverlayNode();
		var child1 = new FixedNode(50, 50) { HAlignment = Alignment.Stretch, VAlignment = Alignment.Stretch };
		var child2 = new FixedNode(60, 60) { HAlignment = Alignment.Stretch, VAlignment = Alignment.Stretch };
		overlay.Children.Add(child1);
		overlay.Children.Add(child2);

		_ = overlay.Measure(new Size(200, 200));
		overlay.Arrange(new Rect(10, 20, 200, 200));

		foreach (var child in overlay.Children)
		{
			using (Assert.EnterMultipleScope())
			{
				Assert.That(child.Bounds.X, Is.EqualTo(10));
				Assert.That(child.Bounds.Y, Is.EqualTo(20));
				Assert.That(child.Bounds.Width, Is.EqualTo(200));
				Assert.That(child.Bounds.Height, Is.EqualTo(200));
			}
		}
	}

	[Test]
	public void Overlay_WithPadding_DeflatesSlot()
	{
		var overlay = new OverlayNode { Padding = new Thickness(10, 20) };
		var child = new FixedNode(50, 50) { HAlignment = Alignment.Stretch, VAlignment = Alignment.Stretch };
		overlay.Children.Add(child);

		_ = overlay.Measure(new Size(200, 200));
		overlay.Arrange(new Rect(0, 0, 200, 200));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(overlay.Children[0].Bounds.Width, Is.EqualTo(180));
			Assert.That(overlay.Children[0].Bounds.Height, Is.EqualTo(160));
		}
	}
}