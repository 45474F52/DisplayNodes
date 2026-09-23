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
using DisplayNodes.Gdi;

using Color = DisplayNodes.Core.Color;
using Point = DisplayNodes.Core.Point;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
public class GdiBrushFactoryGradientTests
{
    private GdiBrushFactory _factory = null!;

    [SetUp]
    public void SetUp() => _factory = new GdiBrushFactory();

    [Test]
    public void CreateLinearGradient_ReturnsDescription()
    {
        var brush = _factory.CreateLinearGradient(
            new Point(0, 0),
            new Point(100, 0),
            new GradientStop(Color.Red, 0),
            new GradientStop(Color.Blue, 100));

        Assert.That(brush, Is.InstanceOf<GdiLinearGradientBrush>());
    }

    [Test]
    public void CreateLinearGradient_NullStops_Throws()
        => _ = Assert.Throws<ArgumentNullException>(
            () => _factory.CreateLinearGradient(new Point(0, 0), new Point(100, 0), null!));

    [Test]
    public void CreateLinearGradient_OneStop_Throws()
        => _ = Assert.Throws<ArgumentException>(
            () => _factory.CreateLinearGradient(new Point(0, 0), new Point(100, 0), new GradientStop(Color.Red, 0)));

    [Test]
    public void CreateLinearGradient_StoresStops()
    {
        var stops = new[]
        {
            new GradientStop(Color.Red, 0),
            new GradientStop(Color.Green, 50),
            new GradientStop(Color.Blue, 100)
        };

        var brush = (GdiLinearGradientBrush)_factory.CreateLinearGradient(
            new Point(0, 0), new Point(100, 0), stops);

        Assert.That(brush.Stops.Length, Is.EqualTo(3));
    }

    [Test]
    public void CreateRadialGradient_ReturnsDescription()
    {
        var brush = _factory.CreateRadialGradient(
            new Point(50, 50),
            50,
            new GradientStop(Color.White, 0),
            new GradientStop(Color.Black, 100));

        Assert.That(brush, Is.InstanceOf<GdiRadialGradientBrush>());
    }

    [Test]
    public void CreateRadialGradient_NullStops_Throws()
        => _ = Assert.Throws<ArgumentNullException>(
            () => _factory.CreateRadialGradient(new Point(50, 50), 50, null!));

    [Test]
    public void LinearGradientBrush_CreateGdiBrush_ProducesBrush()
    {
        var description = (GdiLinearGradientBrush)_factory.CreateLinearGradient(
            new Point(0, 0),
            new Point(100, 0),
            new GradientStop(Color.Red, 0),
            new GradientStop(Color.Blue, 100));

        using (var gdi = description.CreateGdiBrush(new RectangleF(0, 0, 200, 100)))
        {
            Assert.That(gdi, Is.Not.Null);
        }
    }

    [Test]
    public void RadialGradientBrush_CreateGdiBrush_ProducesBrush()
    {
        var description = (GdiRadialGradientBrush)_factory.CreateRadialGradient(
            new Point(50, 50),
            50,
            new GradientStop(Color.White, 0),
            new GradientStop(Color.Black, 100));

        using (var gdi = description.CreateGdiBrush(new RectangleF(0, 0, 200, 200)))
        {
            Assert.That(gdi, Is.Not.Null);
        }
    }
}