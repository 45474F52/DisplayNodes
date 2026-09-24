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

using DisplayNodes.WinFormsAdapter;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Adapters.WinFormsAdapter
{
    [TestFixture]
    public class MaskAdapterTests
    {
        private WidgetFactory _factory = null!;

        [SetUp]
        public void SetUp() => _factory = new WidgetFactory();

        [Test]
        public void RectMask_SetLocationSize()
        {
            var mask = _factory.CreateRectMask();
            mask.Location = new Point(10, 20);
            mask.Size = new Size(100, 200);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mask.Location, Is.EqualTo(new Point(10, 20)));
                Assert.That(mask.Size, Is.EqualTo(new Size(100, 200)));
            }
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void CircleMask_SetLocationSize()
        {
            var mask = _factory.CreateCircleMask();
            mask.Location = new Point(5, 15);
            mask.Size = new Size(50, 50);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mask.Location, Is.EqualTo(new Point(5, 15)));
                Assert.That(mask.Size, Is.EqualTo(new Size(50, 50)));
            }
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void EllipseMask_SetLocationSize()
        {
            var mask = _factory.CreateEllipseMask();
            mask.Location = new Point(0, 0);
            mask.Size = new Size(80, 40);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mask.Location, Is.EqualTo(new Point(0, 0)));
                Assert.That(mask.Size, Is.EqualTo(new Size(80, 40)));
            }
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void RoundedRectMask_SetLocationSize()
        {
            var mask = _factory.CreateRoundedRectMask(12f);
            mask.Location = new Point(1, 2);
            mask.Size = new Size(200, 100);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mask.Location, Is.EqualTo(new Point(1, 2)));
                Assert.That(mask.Size, Is.EqualTo(new Size(200, 100)));
            }
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void RoundedRectMask_ZeroHeight_DoesNotThrow()
        {
            // Регрессия на ArgumentException ("Недопустимый параметр") в GraphicsPath.AddArc:
            // маска может получить нулевую высоту до/во время первого прохода лэйаута.
            var mask = _factory.CreateRoundedRectMask(8f);
            Assert.DoesNotThrow(() => mask.Size = new Size(69, 0));
            Assert.DoesNotThrow(() => mask.Size = new Size(0, 0));
            Assert.DoesNotThrow(() => mask.Size = new Size(69, 16)); // вырожденный случай: r >= H/2
            Assert.DoesNotThrow(() => mask.Size = new Size(69, 40)); // нормальный скруглённый прямоугольник
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void ClipNode_Measure_WithOnlyBackground_IsAtLeastOnePixel()
        {
            // UI.Border(...) кладёт BackgroundNode внутрь ClipNode; фон даёт DesiredSize 0,
            // но клип не должен возвращать нулевой размер — иначе GDI+ падает на построении Region.
            var clip = new DisplayNodes.Widgets.ClipNode(_factory.CreateRoundedRectMask(8f));
            clip.Children.Add(new DisplayNodes.Widgets.BackgroundNode(
                new DisplayNodes.Gdi.GdiBrush(new System.Drawing.SolidBrush(System.Drawing.Color.Gray)),
                (DisplayNodes.Core.Rendering.ILabelComponent)new WinFormsAdapter.Components.Label()));

            var size = clip.Measure(new Size(int.MaxValue, int.MaxValue));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(size.Width, Is.GreaterThanOrEqualTo(1));
                Assert.That(size.Height, Is.GreaterThanOrEqualTo(1));
            }
            (clip as IDisposable)?.Dispose();
        }

        [Test]
        public void PathMask_WithBuilder_Works()
        {
            var mask = _factory.CreatePathMask(rect =>
                new DisplayNodes.Gdi.GdiGraphicsPath(new System.Drawing.Drawing2D.GraphicsPath()));
            mask.Location = new Point(10, 10);
            mask.Size = new Size(100, 100);
            Assert.That(mask.Size, Is.EqualTo(new Size(100, 100)));
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void Mask_Visible_DefaultsToTrue()
        {
            var mask = _factory.CreateRectMask();
            Assert.That(mask.Visible, Is.True);
            (mask as IDisposable)?.Dispose();
        }

        [Test]
        public void Mask_Visible_CanBeToggled()
        {
            var mask = _factory.CreateRectMask();
            mask.Visible = false;
            Assert.That(mask.Visible, Is.False);
            (mask as IDisposable)?.Dispose();
        }
    }
}
