using DisplayNodes.Core;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class GridDefinitionTests
{
	[Test]
	public void GridLength_Pixel_CreatedCorrectly()
	{
		var length = GridLength.Pixels(100);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(100));
			Assert.That(length.IsAbsolute, Is.True);
			Assert.That(length.IsAuto, Is.False);
			Assert.That(length.IsStar, Is.False);
		}
	}

	[Test]
	public void GridLength_Auto_CreatedCorrectly()
	{
		var length = GridLength.Auto;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(0));
			Assert.That(length.IsAuto, Is.True);
			Assert.That(length.IsAbsolute, Is.False);
		}
	}

	[Test]
	public void GridLength_Star_CreatedCorrectly()
	{
		var length = GridLength.Star(2);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(2));
			Assert.That(length.IsStar, Is.True);
			Assert.That(length.IsAuto, Is.False);
		}
	}

	[Test]
	public void GridLength_Star_DefaultValueIsOne()
	{
		var length = GridLength.Star();
		Assert.That(length.Value, Is.EqualTo(1));
	}

	[Test]
	public void GridLength_NegativeValue_ThrowsException()
		=> _ = Assert.Throws<ArgumentOutOfRangeException>(() => GridLength.Pixels(-1));

	[Test]
	public void RowDefinition_DefaultIsStar()
	{
		var row = new RowDefinition(GridLength.Auto);
		Assert.That(row.Height.IsAuto, Is.True);
	}

	[Test]
	public void ColumnDefinition_DefaultIsStar()
	{
		var col = new ColumnDefinition(GridLength.Pixels(100));
		using (Assert.EnterMultipleScope())
		{
			Assert.That(col.Width.IsAbsolute, Is.True);
			Assert.That(col.Width.Value, Is.EqualTo(100));
		}
	}
}