using DisplayNodes.Core;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class FixedNodeTests
{
	[Test]
	public void FixedNode_AlwaysReturnsFixedSize()
	{
		var node = new FixedNode(100, 200);

		var size1 = node.Measure(new Size(50, 50));      // Less than fixed
		var size2 = node.Measure(new Size(500, 500));    // More than fixed

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size1.Width, Is.EqualTo(100));
			Assert.That(size1.Height, Is.EqualTo(200));
			Assert.That(size2.Width, Is.EqualTo(100));
			Assert.That(size2.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void FixedNode_Arrange_DoesNothing()
	{
		var node = new FixedNode(100, 200);
		_ = node.Measure(Size.Infinity);

		Assert.DoesNotThrow(() => node.Arrange(new Rect(0, 0, 100, 200)));
	}
}