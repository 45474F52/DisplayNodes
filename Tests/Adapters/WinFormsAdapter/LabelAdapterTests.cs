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
        public void Format_VerticalCenter_PreservedInGetAndUseMnemonicSwitchedOff()
        {
            var label = _factory.CreateLabel();

            using var original = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            label.Format = new GdiTextFormat(original);

            // Вертикальная составляющая не должна теряться при round-trip через Format.
            var readBack = label.Format.ToGdi();
            Assert.That(readBack.Alignment, Is.EqualTo(StringAlignment.Center));
            Assert.That(readBack.LineAlignment, Is.EqualTo(StringAlignment.Center),
                "LineAlignment (vertical) must be preserved");

            // Двумерное выравнивание возможно только в OwnerDraw-режиме (UseMnemonic=false):
            // нативный рендер системного Label поддерживает только верхний ряд.
            Assert.That(label.UseMnemonic, Is.False);

            (label as IDisposable)?.Dispose();
        }

        [Test]
        public void Format_TopVertical_KeepsSystemRenderMode()
        {
            var label = _factory.CreateLabel();

            using var original = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            };
            label.Format = new GdiTextFormat(original);

            var readBack = label.Format.ToGdi();
            Assert.That(readBack.Alignment, Is.EqualTo(StringAlignment.Far));
            Assert.That(readBack.LineAlignment, Is.EqualTo(StringAlignment.Near));
            Assert.That(label.UseMnemonic, Is.True);

            (label as IDisposable)?.Dispose();
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
