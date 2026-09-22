using DisplayNodes.Gdi;

using Color = DisplayNodes.Core.Color;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
public class GdiBrushFactoryTests
{
	[Test]
	public void CreateSolidBrush_CreatesBrushWithCorrectColor()
	{
		var factory = new GdiBrushFactory();
		var color = new Color(100, 150, 200, 128);

		var brush = factory.CreateSolidBrush(color);

		Assert.That(brush, Is.InstanceOf<GdiBrush>());
		var gdiBrush = ((GdiBrush)brush).Inner;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gdiBrush.Color.R, Is.EqualTo(100));
			Assert.That(gdiBrush.Color.G, Is.EqualTo(150));
			Assert.That(gdiBrush.Color.B, Is.EqualTo(200));
			Assert.That(gdiBrush.Color.A, Is.EqualTo(128));
		}
	}

	[Test]
	public void CreateSolidBrush_CreatesNewInstanceEachCall()
	{
		var factory = new GdiBrushFactory();
		var color = Color.Red;

		var brush1 = factory.CreateSolidBrush(color);
		var brush2 = factory.CreateSolidBrush(color);

		Assert.That(brush1, Is.Not.SameAs(brush2));
	}
}