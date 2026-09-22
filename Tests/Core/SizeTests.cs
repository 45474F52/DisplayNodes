using DisplayNodes.Core;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class SizeTests
{
	[Test]
	public void Constructor_SetsValuesCorrectly()
	{
		var size = new Size(100, 200);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(100));
			Assert.That(size.Height, Is.EqualTo(200));
		}
	}

	[Test]
	public void Empty_ReturnsZeroSize()
	{
		using (Assert.EnterMultipleScope())
		{
			Assert.That(Size.Empty.Width, Is.EqualTo(0));
			Assert.That(Size.Empty.Height, Is.EqualTo(0));
		}
	}

	[Test]
	public void Infinity_ReturnsMaxValues()
	{
		using (Assert.EnterMultipleScope())
		{
			Assert.That(Size.Infinity.Width, Is.EqualTo(int.MaxValue));
			Assert.That(Size.Infinity.Height, Is.EqualTo(int.MaxValue));
		}
	}

	[Test]
	public void WithWidth_ReturnsNewSizeWithChangedWidth()
	{
		var size = new Size(100, 200);
		var newSize = size.WithWidth(300);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(newSize.Width, Is.EqualTo(300));
			Assert.That(newSize.Height, Is.EqualTo(200));
			Assert.That(size.Width, Is.EqualTo(100)); // Original unchanged
		}
	}

	[Test]
	public void Inflate_AddsThickness()
	{
		var size = new Size(100, 200);
		var thickness = new Thickness(10, 20);
		var inflated = size.Inflate(thickness);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(inflated.Width, Is.EqualTo(120)); // 100 + 10*2
			Assert.That(inflated.Height, Is.EqualTo(240)); // 200 + 20*2
		}
	}

	[Test]
	public void Deflate_SubtractsThickness()
	{
		var size = new Size(100, 200);
		var thickness = new Thickness(10, 20);
		var deflated = size.Deflate(thickness);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(deflated.Width, Is.EqualTo(80)); // 100 - 10*2
			Assert.That(deflated.Height, Is.EqualTo(160)); // 200 - 20*2
		}
	}

	[Test]
	public void Deflate_NeverReturnsNegative()
	{
		var size = new Size(10, 10);
		var thickness = new Thickness(20, 20);
		var deflated = size.Deflate(thickness);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(deflated.Width, Is.EqualTo(0));
			Assert.That(deflated.Height, Is.EqualTo(0));
		}
	}

	[Test]
	public void Equality_OperatorWorks()
	{
		var size1 = new Size(100, 200);
		var size2 = new Size(100, 200);
		var size3 = new Size(100, 300);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size1, Is.EqualTo(size2));
			Assert.That(size1, Is.Not.EqualTo(size3));
			Assert.That(size1, Is.Not.EqualTo(size3));
		}
	}

	[Test]
	public void Addition_OperatorWorks()
	{
		var size1 = new Size(100, 200);
		var size2 = new Size(50, 30);
		var result = size1 + size2;

		using (Assert.EnterMultipleScope())
		{
			Assert.That(result.Width, Is.EqualTo(150));
			Assert.That(result.Height, Is.EqualTo(230));
		}
	}

	[Test]
	public void Subtraction_OperatorWorks()
	{
		var size1 = new Size(100, 200);
		var size2 = new Size(50, 30);
		var result = size1 - size2;

		using (Assert.EnterMultipleScope())
		{
			Assert.That(result.Width, Is.EqualTo(50));
			Assert.That(result.Height, Is.EqualTo(170));
		}
	}
}