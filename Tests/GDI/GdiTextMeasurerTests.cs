using System.Drawing;

using DisplayNodes.Gdi;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
[Platform("Win")]
public class GdiTextMeasurerTests
{
	private GdiTextMeasurer _measurer = null!;
	private Font _font = null!;

	[SetUp]
	public void SetUp()
	{
		_measurer = new GdiTextMeasurer();
		_font = new Font(FontFamily.GenericSansSerif, 12f);
	}

	[TearDown]
	public void TearDown() => _font.Dispose();

	[Test]
	public void MeasureArea_EmptyText_ReturnsEmpty()
	{
		var size = _measurer.MeasureArea("", new GdiFont(_font));
		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(0));
			Assert.That(size.Height, Is.EqualTo(0));
		}
	}

	[Test]
	public void MeasureArea_NullFont_ReturnsEmpty()
	{
		var size = _measurer.MeasureArea("Hello", null!);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(0));
			Assert.That(size.Height, Is.EqualTo(0));
		}
	}

	[Test]
	public void MeasureArea_ShortText_ReturnsNonEmpty()
	{
		var size = _measurer.MeasureArea("Hello", new GdiFont(_font));
		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.GreaterThan(0));
			Assert.That(size.Height, Is.GreaterThan(0));
		}
	}

	[Test]
	public void MeasureArea_LongerText_IsWider()
	{
		var shortSize = _measurer.MeasureArea("Hi", new GdiFont(_font));
		var longSize = _measurer.MeasureArea("Hello, world!", new GdiFont(_font));
		Assert.That(longSize.Width, Is.GreaterThan(shortSize.Width));
	}

	[Test]
	public void MeasureArea_WithMaxWidth_DoesNotExceedIt()
	{
		var size = _measurer.MeasureArea(
			"This is a very long text that should wrap",
			new GdiFont(_font),
			maxWidth: 50);
		// Width should be close to maxWidth (with some offset tolerance)
		Assert.That(size.Width, Is.LessThanOrEqualTo(60));
	}
}