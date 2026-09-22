using DisplayNodes.Gdi;
using DisplayNodes.WinFormsAdapter;
using System.Drawing;

namespace DisplayNodes.Tests.Adapters.WinFormsAdapter
{
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
            (label as IDisposable)?.Dispose();
            Assert.That(original.SizeInPoints, Is.EqualTo(14f));
        }

        [Test]
        public void Brush_ClonesOnSet_OriginalNotDisposedByAdapter()
        {
            var label = _factory.CreateLabel();
            using var original = new SolidBrush(Color.Red);
            label.ForegroundBrush = new GdiBrush(original);
            (label as IDisposable)?.Dispose();
            Assert.That(original.Color, Is.EqualTo(Color.Red));
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
    }
}
