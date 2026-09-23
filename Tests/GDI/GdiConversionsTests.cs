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
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using System.Drawing.Drawing2D;
using Color = DisplayNodes.Core.Color;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
public class GdiConversionsTests
{
	[Test]
	public void Color_ToGdi_ConvertsChannels()
	{
		var core = new Color(10, 20, 30, 40);
		var gdi = core.ToGdi();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(gdi.R, Is.EqualTo(10));
			Assert.That(gdi.G, Is.EqualTo(20));
			Assert.That(gdi.B, Is.EqualTo(30));
			Assert.That(gdi.A, Is.EqualTo(40));
		}
	}

	[Test]
	public void Color_FromGdi_ConvertsChannels()
	{
		var gdi = System.Drawing.Color.FromArgb(50, 60, 70, 80);
		var core = gdi.FromGdi();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(core.R, Is.EqualTo(60));
			Assert.That(core.G, Is.EqualTo(70));
			Assert.That(core.B, Is.EqualTo(80));
			Assert.That(core.A, Is.EqualTo(50));
		}
	}

	[Test]
	public void Rect_ToGdi_PreservesValues()
	{
		var core = new Rect(10, 20, 30, 40);
		var gdi = core.ToGdi();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(gdi.X, Is.EqualTo(10f));
			Assert.That(gdi.Y, Is.EqualTo(20f));
			Assert.That(gdi.Width, Is.EqualTo(30f));
			Assert.That(gdi.Height, Is.EqualTo(40f));
		}
	}

	[Test]
	public void Rect_FromGdi_PreservesValues()
	{
		var gdi = new RectangleF(1.5f, 2.5f, 3.5f, 4.5f);
		var core = gdi.FromGdi();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(core.X, Is.EqualTo(2));  // округляется
			Assert.That(core.Y, Is.EqualTo(2));
			Assert.That(core.Width, Is.EqualTo(4));
			Assert.That(core.Height, Is.EqualTo(4));
		}
	}

	[Test]
	public void IFont_ToGdi_ReturnsInner_WhenGdiFont()
	{
		using var font = new Font("Arial", 12f);
		var wrapper = new GdiFont(font);
		var extracted = ((IFont)wrapper).ToGdi();
		Assert.That(ReferenceEquals(extracted, font), Is.True);
	}

	[Test]
	public void IFont_ToGdi_ReturnsNull_WhenWrongType()
	{
		var stub = new StubFont();
		var extracted = stub.ToGdi();
		Assert.That(extracted, Is.Null);
	}

	[Test]
	public void IImage_ToGdi_ReturnsInner_WhenGdiImage()
	{
		using var bmp = new Bitmap(10, 10);
		var wrapper = new GdiImage(bmp);
		var extracted = ((IImage)wrapper).ToGdi();
		Assert.That(ReferenceEquals(extracted, bmp), Is.True);
	}

	[Test]
	public void IGraphicsPath_ToGdi_ReturnsInner()
	{
		using var path = new GraphicsPath();
		path.AddLine(0, 0, 10, 10);
		var wrapper = new GdiGraphicsPath(path);
		var extracted = ((IGraphicsPath)wrapper).ToGdi();
		Assert.That(ReferenceEquals(extracted, path), Is.True);
	}

    [Test]
    public void IBrush_ToGdi_ReturnsInner_WhenGdiBrush()
    {
        using var solid = new SolidBrush(System.Drawing.Color.Red);
        var wrapper = new GdiBrush(solid);

        var extracted = ((IBrush)wrapper).ToGdi();

        Assert.That(ReferenceEquals(extracted, solid), Is.True);
    }

    [Test]
    public void IBrush_ToGdi_ReturnsNull_WhenNull()
    {
        IBrush? brush = default;
        SolidBrush extracted = GdiConversions.ToGdi(brush);
        Assert.That(extracted, Is.Null);
    }

    [Test]
    public void IBrush_ToGdi_Throws_WhenGradient()
    {
        var factory = new GdiBrushFactory();
        var gradient = factory.CreateLinearGradient(
            new DisplayNodes.Core.Point(0, 0),
            new DisplayNodes.Core.Point(100, 0),
            new GradientStop(Color.Red, 0),
            new GradientStop(Color.Blue, 100));

        var ex = Assert.Throws<NotSupportedException>(() => gradient.ToGdi());
        Assert.That(ex.Message, Does.Contain("GdiLinearGradientBrush"));
    }

    private sealed class StubFont : IFont { }
}