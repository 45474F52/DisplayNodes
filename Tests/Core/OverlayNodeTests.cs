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