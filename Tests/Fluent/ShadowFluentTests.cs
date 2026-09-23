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
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Fluent;

[TestFixture]
public class ShadowFluentTests
{
    private IWidgetFactory _oldFactory;

    [SetUp]
    public void SetUp()
    {
        _oldFactory = UI.Factory;
        UI.Factory = new StubWidgetFactory();
        UI.Measurer = new StubTextMeasurer();
    }

    [TearDown]
    public void TearDown()
    {
        UI.Factory = _oldFactory;
    }

    [Test]
    public void Shadow_OnWidgetNode_Throws_NotSupported()
    {
        // Стаб-компонент бросает NotSupportedException на установку Shadow.
        var node = UI.Label("text", new StubFont(), new StubBrush());

        _ = Assert.Throws<NotSupportedException>(() => node.Shadow(2, 4, 8));
    }

    [Test]
    public void Shadow_OnNonWidget_Ignored()
    {
        // Stack — не виджет, Shadow молча игнорируется.
        var stack = UI.Stack();

        Assert.DoesNotThrow(() => stack.Shadow(2, 4, 8));
    }

    // ---------------------------------------------------------------
    // Стабы
    // ---------------------------------------------------------------

#pragma warning disable CS8618
    private sealed class StubBrush : IBrush { }
    private sealed class StubFont : IFont { }

    private sealed class StubTextMeasurer : ITextMeasurer
    {
        public Size MeasureArea(string text, IFont font, int maxWidth = 0)
        {
            return new Size(text.Length, 14);
        }
    }

    private sealed class StubLabelComponent : ILabelComponent, ITextLayoutComponent, IEffectComponent
    {
        public string Text { get; set; }
        public IFont Font { get; set; }
        public IBrush ForegroundBrush { get; set; }
        public IBrush BackgroundBrush { get; set; }
        public ITextFormat Format { get; set; }

        public IRenderComponent Parent { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; } = true;

        public double Opacity { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }

        public TextDrawMethod DrawMethod { get; set; }
        public LabelStretch Stretch { get; set; }

        private Shadow? _shadow;
        public Shadow? Shadow
        {
            get => _shadow;
            set
            {
                if (value.HasValue)
                    throw new NotSupportedException("Shadow is not supported by StubLabelComponent.");
                _shadow = null;
            }
        }
    }

    private sealed class StubWidgetFactory : IWidgetFactory
    {
        public ILabelComponent CreateLabel() => new StubLabelComponent();

        public IImageComponent CreateImage()
            => throw new NotSupportedException();

        public IMaskComponent CreateRectMask()
            => throw new NotSupportedException();

        public IMaskComponent CreateCircleMask()
            => throw new NotSupportedException();

        public IMaskComponent CreateEllipseMask()
            => throw new NotSupportedException();

        public IMaskComponent CreateRoundedRectMask(float cornerRadius)
            => throw new NotSupportedException();

        public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
            => throw new NotSupportedException();
    }
#pragma warning restore CS8618
}