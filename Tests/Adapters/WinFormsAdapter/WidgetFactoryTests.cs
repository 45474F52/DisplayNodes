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
