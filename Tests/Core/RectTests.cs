using DisplayNodes.Core;
using Point = DisplayNodes.Core.Point;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class RectTests
{
	[Test]
	public void Constructor_SetsValuesCorrectly()
	{
		var rect = new Rect(10, 20, 100, 200);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(rect.X, Is.EqualTo(10));
			Assert.That(rect.Y, Is.EqualTo(20));
			Assert.That(rect.Width, Is.EqualTo(100));
			Assert.That(rect.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void Right_Bottom_CalculatedCorrectly()
	{
		var rect = new Rect(10, 20, 100, 200);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(rect.Right, Is.EqualTo(110));
			Assert.That(rect.Bottom, Is.EqualTo(220));
		}
	}

	[Test]
	public void Deflate_ReducesRectByThickness()
	{
		var rect = new Rect(0, 0, 100, 100);
		var thickness = new Thickness(10, 20);
		var deflated = rect.Deflate(thickness);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(deflated.X, Is.EqualTo(10));
			Assert.That(deflated.Y, Is.EqualTo(20));
			Assert.That(deflated.Width, Is.EqualTo(80));
			Assert.That(deflated.Height, Is.EqualTo(60));
		}
	}

	[Test]
	public void Contains_Point_ReturnsTrueForInsidePoint()
	{
		var rect = new Rect(0, 0, 100, 100);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(rect.Contains(new Point(50, 50)), Is.True);
			Assert.That(rect.Contains(new Point(0, 0)), Is.True); // Left/Top edge inclusive
			Assert.That(rect.Contains(new Point(99, 99)), Is.True);
			Assert.That(rect.Contains(new Point(100, 50)), Is.False); // Right edge exclusive
			Assert.That(rect.Contains(new Point(50, 100)), Is.False); // Bottom edge exclusive
		}
	}

	[Test]
	public void IntersectsWith_ReturnsTrueForOverlappingRects()
	{
		var rect1 = new Rect(0, 0, 100, 100);
		var rect2 = new Rect(50, 50, 100, 100);
		Assert.That(rect1.IntersectsWith(rect2), Is.True);
	}

	[Test]
	public void IntersectsWith_ReturnsFalseForNonOverlappingRects()
	{
		var rect1 = new Rect(0, 0, 100, 100);
		var rect2 = new Rect(200, 200, 100, 100);
		Assert.That(rect1.IntersectsWith(rect2), Is.False);
	}

	[Test]
	public void Intersect_ReturnsIntersectionRect()
	{
		var rect1 = new Rect(0, 0, 100, 100);
		var rect2 = new Rect(50, 50, 100, 100);
		var intersection = rect1.Intersect(rect2);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(intersection.X, Is.EqualTo(50));
			Assert.That(intersection.Y, Is.EqualTo(50));
			Assert.That(intersection.Width, Is.EqualTo(50));
			Assert.That(intersection.Height, Is.EqualTo(50));
		}
	}

	[Test]
	public void Intersect_ReturnsEmptyForNonOverlapping()
	{
		var rect1 = new Rect(0, 0, 100, 100);
		var rect2 = new Rect(200, 200, 100, 100);
		var intersection = rect1.Intersect(rect2);

		Assert.That(intersection.IsEmpty, Is.True);
	}
}
