using System.Drawing;

using DisplayNodes.Gdi;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
public class GdiWrappersTests
{
	[Test]
	public void GdiFont_NullFont_Throws()
		=> _ = Assert.Throws<ArgumentNullException>(() => new GdiFont(null!));

	[Test]
	public void GdiBrush_NullBrush_Throws()
		=> _ = Assert.Throws<ArgumentNullException>(() => new GdiBrush(null!));

	[Test]
	public void GdiTextFormat_NullFormat_Throws()
		=> _ = Assert.Throws<ArgumentNullException>(() => new GdiTextFormat(null!));

	[Test]
	public void GdiGraphicsPath_NullPath_Throws()
		=> _ = Assert.Throws<ArgumentNullException>(() => new GdiGraphicsPath(null!));

	[Test]
	public void GdiImage_NullBitmap_Allowed()
	{
		var img = new GdiImage(null);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(img.Inner, Is.Null);
			Assert.That(img.Width, Is.EqualTo(0));
			Assert.That(img.Height, Is.EqualTo(0));
		}
	}

	[Test]
	public void GdiImage_WithBitmap_ReturnsCorrectDimensions()
	{
		using var bmp = new Bitmap(123, 456);
		var img = new GdiImage(bmp);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(img.Width, Is.EqualTo(123));
			Assert.That(img.Height, Is.EqualTo(456));
		}
	}
}