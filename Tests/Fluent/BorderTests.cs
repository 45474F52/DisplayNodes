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
using DisplayNodes.Fluent;
using DisplayNodes.Widgets;
using Color = DisplayNodes.Core.Color;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Fluent;

[TestFixture]
public class BorderTests
{
    private IBrushFactory _oldBrushFactory;
    private IWidgetFactory _oldWidgetFactory;

    [SetUp]
    public void SetUp()
    {
        _oldBrushFactory = UI.BrushFactory;
        _oldWidgetFactory = UI.Factory;

        UI.BrushFactory = new StubBrushFactory();
        UI.Factory = new StubWidgetFactory();
    }

    [TearDown]
    public void TearDown()
    {
        UI.BrushFactory = _oldBrushFactory;
        UI.Factory = _oldWidgetFactory;
    }

    // ---------------------------------------------------------------
    // Структура дерева
    // ---------------------------------------------------------------

    [Test]
    public void Border_ReturnsOverlayNode()
    {
        var border = UI.Border(new StubBrush());
        Assert.That(border, Is.InstanceOf<OverlayNode>());
    }

    [Test]
    public void Border_FirstChildIsClipWithBackground()
    {
        var border = UI.Border(new StubBrush());

        Assert.That(border.Children.Count, Is.EqualTo(1));
        Assert.That(border.Children[0], Is.InstanceOf<ClipNode>());

        var clip = (ClipNode)border.Children[0];
        Assert.That(clip.Children.Count, Is.EqualTo(1));
        Assert.That(clip.Children[0], Is.InstanceOf<BackgroundNode>());
    }

    [Test]
    public void Border_ZeroCornerRadius_MaskIsSet()
    {
        var border = UI.Border(new StubBrush(), cornerRadius: 0);
        var clip = (ClipNode)border.Children[0];

        Assert.That(clip.Mask, Is.Not.Null);
        Assert.That(clip.Mask, Is.InstanceOf<IMaskComponent>());
    }

    [Test]
    public void Border_WithCornerRadius_MaskIsSet()
    {
        var border = UI.Border(new StubBrush(), cornerRadius: 8);
        var clip = (ClipNode)border.Children[0];

        Assert.That(clip.Mask, Is.Not.Null);
        Assert.That(clip.Mask, Is.InstanceOf<IMaskComponent>());
    }

    [Test]
    public void Border_AddContent_ContentIsSiblingOfClip()
    {
        var border = UI.Border(new StubBrush())
            .Add(new FixedNode(50, 50));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(border.Children.Count, Is.EqualTo(2));
            Assert.That(border.Children[0], Is.InstanceOf<ClipNode>());
            Assert.That(border.Children[1], Is.InstanceOf<FixedNode>());
        }
    }

    // ---------------------------------------------------------------
    // Measure / Arrange
    // ---------------------------------------------------------------

    [Test]
    public void Border_Measure_ReturnsContentSize()
    {
        var border = UI.Border(new StubBrush())
            .Add(new FixedNode(100, 50));

        var size = border.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(100));
            Assert.That(size.Height, Is.EqualTo(50));
        }
    }

    [Test]
    public void Border_WithPadding_AddsPadding()
    {
        var border = UI.Border(new StubBrush())
            .Padding(12)
            .Add(new FixedNode(100, 50));

        var size = border.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(124));   // 100 + 12*2
            Assert.That(size.Height, Is.EqualTo(74));   // 50 + 12*2
        }
    }

    [Test]
    public void Border_BackgroundDoesNotAffectMeasure()
    {
        // BackgroundNode.MeasureOverride возвращает Size.Empty — фон не влияет на размер.
        var border = UI.Border(new StubBrush())
            .Add(new FixedNode(60, 40));

        var size = border.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(60));
            Assert.That(size.Height, Is.EqualTo(40));
        }
    }

    // ---------------------------------------------------------------
    // Защита от null
    // ---------------------------------------------------------------

    [Test]
    public void Border_NullBrush_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => UI.Border((IBrush)null!));
    }

    [Test]
    public void Background_NullBrush_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => UI.Background((IBrush)null!));
    }

    // ---------------------------------------------------------------
    // Перегрузка Color
    // ---------------------------------------------------------------

    [Test]
    public void Border_ColorOverload_CreatesBrush()
    {
        var border = UI.Border(Color.Red);

        Assert.That(border, Is.InstanceOf<OverlayNode>());
        Assert.That(border.Children.Count, Is.EqualTo(1));
    }

    [Test]
    public void Background_ColorOverload_CreatesBrush()
    {
        var bg = UI.Background(Color.Blue);

        Assert.That(bg, Is.InstanceOf<BackgroundNode>());
    }

    // ===============================================================
    // Стабы
    // ===============================================================

#pragma warning disable CS8618
    private sealed class StubBrush : IBrush { }

    private sealed class StubBrushFactory : IBrushFactory
    {
        public IBrush CreateSolidBrush(Color color) => new StubBrush();

        public IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops)
            => new StubBrush();

        public IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops)
            => new StubBrush();
    }

    private sealed class StubMaskComponent : IMaskComponent
    {
        public IRenderComponent Parent { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; } = true;
    }

    private sealed class StubLabelComponent : ILabelComponent, ITextLayoutComponent
    {
        public string Text { get; set; }
        public IFont Font { get; set; }
        public IBrush ForegroundBrush { get; set; }
        public IBrush BackgroundBrush { get; set; }
        public ITextFormat Format { get; set; }
        public bool UseMnemonic { get; set; }

        public IRenderComponent Parent { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; } = true;

        public double Opacity { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public Shadow? Shadow { get; set; }

        public TextDrawMethod DrawMethod { get; set; }
        public LabelStretch Stretch { get; set; }
    }

    private sealed class StubWidgetFactory : IWidgetFactory
    {
        public ILabelComponent CreateLabel() => new StubLabelComponent();

        public IImageComponent CreateImage()
            => throw new NotSupportedException("Image not needed in Border tests.");

        public IMaskComponent CreateRectMask() => new StubMaskComponent();

        public IMaskComponent CreateCircleMask() => new StubMaskComponent();

        public IMaskComponent CreateEllipseMask() => new StubMaskComponent();

        public IMaskComponent CreateRoundedRectMask(float cornerRadius) => new StubMaskComponent();

        public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
            => new StubMaskComponent();
    }
#pragma warning restore CS8618
}