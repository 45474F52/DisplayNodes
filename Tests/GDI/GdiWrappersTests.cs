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