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

using System.Drawing;

using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using DisplayNodes.LibDisplayDrawingAdapter;

namespace DisplayNodes.Tests.Adapters.LibDisplayDrawingAdapter;

[TestFixture]
public class LabelAdapterTests
{
	private WidgetFactory _factory = null!;

	[SetUp]
	public void SetUp() => _factory = new WidgetFactory();

	[Test]
	public void Text_SetAndGet()
	{
		var label = _factory.CreateLabel();
		label.Text = "Hello";
		Assert.That(label.Text, Is.EqualTo("Hello"));
		(label as IDisposable)?.Dispose();
	}

	[Test]
	public void Font_ClonesOnSet_OriginalNotDisposedByAdapter()
	{
		var label = _factory.CreateLabel();
		using var original = new Font("Arial", 14f);

		label.Font = new GdiFont(original);

		// Dispose adapter
		(label as IDisposable)?.Dispose();

		// Original font should still be usable (adapter cloned it)
		Assert.That(original.SizeInPoints, Is.EqualTo(14f));
	}

	[Test]
	public void Brush_ClonesOnSet_OriginalNotDisposedByAdapter()
	{
		var label = _factory.CreateLabel();
		using var original = new SolidBrush(System.Drawing.Color.Red);

		label.ForegroundBrush = new GdiBrush(original);
		(label as IDisposable)?.Dispose();

		Assert.That(original.Color, Is.EqualTo(System.Drawing.Color.Red));
	}

	[Test]
	public void Format_ClonesOnSet_OriginalNotDisposedByAdapter()
	{
		var label = _factory.CreateLabel();
		using var original = new StringFormat { Alignment = StringAlignment.Center };

		label.Format = new GdiTextFormat(original);
		(label as IDisposable)?.Dispose();

		Assert.That(original.Alignment, Is.EqualTo(StringAlignment.Center));
	}

	[Test]
	public void DrawMethod_SetAndGet()
	{
		var label = _factory.CreateLabel() as ITextLayoutComponent ?? throw new ArgumentException();
		label.DrawMethod = TextDrawMethod.AutoSizeAccordingToText;
		Assert.That(label.DrawMethod, Is.EqualTo(TextDrawMethod.AutoSizeAccordingToText));
		(label as IDisposable)?.Dispose();
	}

	[Test]
	public void Stretch_SetAndGet()
	{
		var label = _factory.CreateLabel() as ITextLayoutComponent ?? throw new ArgumentException();
        label.Stretch = LabelStretch.Full;
		Assert.That(label.Stretch, Is.EqualTo(LabelStretch.Full));
		(label as IDisposable)?.Dispose();
	}

	[Test]
	public void Opacity_SetAndGet()
	{
		var label = _factory.CreateLabel();
		label.Opacity = 50.0;
		Assert.That(label.Opacity, Is.EqualTo(50.0));
		(label as IDisposable)?.Dispose();
	}
}