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