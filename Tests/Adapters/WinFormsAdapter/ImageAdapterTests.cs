using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;
using DisplayNodes.WinFormsAdapter;
using System.Drawing;

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
