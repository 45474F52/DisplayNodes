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

using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using DisplayNodes.WinFormsAdapter;

namespace DisplayNodes.Tests.Adapters.WinFormsAdapter
{

	[TestFixture]
	public class ImageAdapterTests
	{
		private WidgetFactory _factory = null!;

		[SetUp]
		public void SetUp() => _factory = new WidgetFactory();

		[Test]
		public void Image_ClonesOnSet_OriginalNotDisposedByAdapter()
		{
			var img = _factory.CreateImage();
			using var original = new Bitmap(100, 100);
			img.Image = new GdiImage(original);
			(img as IDisposable)?.Dispose();
			Assert.That(original.Width, Is.EqualTo(100));
		}

		[Test]
		public void Image_SetAndGet()
		{
			var img = _factory.CreateImage();
			using var bmp = new Bitmap(50, 60);
			img.Image = new GdiImage(bmp);
			var retrieved = img.Image;
			Assert.That(retrieved, Is.Not.Null);
			using (Assert.EnterMultipleScope())
			{
				Assert.That(retrieved!.Width, Is.EqualTo(50));
				Assert.That(retrieved.Height, Is.EqualTo(60));
			}
			(img as IDisposable)?.Dispose();
		}

		[Test]
		public void Image_NullValue_Allowed()
		{
			var img = _factory.CreateImage();
			img.Image = null;
			Assert.That(img.Image, Is.Null);
			(img as IDisposable)?.Dispose();
		}

		[Test]
		public void SizeMode_SetAndGet()
		{
			var img = _factory.CreateImage();
			img.SizeMode = ImageSizeMode.Stretch;
			Assert.That(img.SizeMode, Is.EqualTo(ImageSizeMode.Stretch));
			(img as IDisposable)?.Dispose();
		}
	}
}
