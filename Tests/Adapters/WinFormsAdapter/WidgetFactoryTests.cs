using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter;

namespace DisplayNodes.Tests.Adapters.WinFormsAdapter
{
    [TestFixture]
    public class WidgetFactoryTests
    {
        private WidgetFactory _factory = null!;

        [SetUp]
        public void SetUp() => _factory = new WidgetFactory();

        [Test]
        public void CreateLabel_ReturnsILabelComponent()
        {
            var label = _factory.CreateLabel();
            Assert.That(label, Is.InstanceOf<ILabelComponent>());
            (label as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateImage_ReturnsIImageComponent()
        {
            var image = _factory.CreateImage();
            Assert.That(image, Is.InstanceOf<IImageComponent>());
            (image as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateRectMask_ReturnsIMaskComponent()
        {
            var mask = _factory.CreateRectMask();
            Assert.That(mask, Is.InstanceOf<IMaskComponent>());
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateCircleMask_ReturnsIMaskComponent()
        {
            var mask = _factory.CreateCircleMask();
            Assert.That(mask, Is.InstanceOf<IMaskComponent>());
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateEllipseMask_ReturnsIMaskComponent()
        {
            var mask = _factory.CreateEllipseMask();
            Assert.That(mask, Is.InstanceOf<IMaskComponent>());
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateRoundedRectMask_ReturnsIMaskComponent()
        {
            var mask = _factory.CreateRoundedRectMask(10f);
            Assert.That(mask, Is.InstanceOf<IMaskComponent>());
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CreatePathMask_WithPathBuilder_ReturnsIMaskComponent()
        {
            var mask = _factory.CreatePathMask(rect =>
                new DisplayNodes.Gdi.GdiGraphicsPath(new System.Drawing.Drawing2D.GraphicsPath()));
            Assert.That(mask, Is.InstanceOf<IMaskComponent>());
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CreateLabel_EachCall_ReturnsNewInstance()
        {
            var l1 = _factory.CreateLabel();
            var l2 = _factory.CreateLabel();
            Assert.That(l1, Is.Not.SameAs(l2));
            (l1 as IDisposable)?.Dispose();
            (l2 as IDisposable)?.Dispose();
        }
    }
}
