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

using DisplayNodes.Core;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class TransformNodeTests
{
    // ---------------------------------------------------------------
    // Bounding box через Measure
    // ---------------------------------------------------------------

    [Test]
    public void Measure_Identity_ReturnsChildSize()
    {
        var node = new TransformNode(Transform.Identity)
        {
            Child = new FixedNode(100, 50)
        };

        var size = node.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(100));
            Assert.That(size.Height, Is.EqualTo(50));
        }
    }

    [Test]
    public void Measure_Scale2x_DoublesBoundingBox()
    {
        var node = new TransformNode(new Transform(2f, 2f, 0f, new Point(50, 50)))
        {
            Child = new FixedNode(100, 50)
        };

        var size = node.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(200));
            Assert.That(size.Height, Is.EqualTo(100));
        }
    }

    [Test]
    public void Measure_Rotation90_SwapsDimensions()
    {
        var node = new TransformNode(new Transform(1f, 1f, 90f, new Point(50, 50)))
        {
            Child = new FixedNode(100, 50)
        };

        var size = node.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(50));
            Assert.That(size.Height, Is.EqualTo(100));
        }
    }

    [Test]
    public void Measure_Rotation45_ExpandsBoundingBox()
    {
        // Квадрат 100x100, повёрнутый на 45°, имеет bbox ≈ 141x141.
        var node = new TransformNode(new Transform(1f, 1f, 45f, new Point(50, 50)))
        {
            Child = new FixedNode(100, 100)
        };

        var size = node.Measure(Size.Infinity);

        // 100 * sqrt(2) ≈ 141.42 → ceil = 142.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(142));
            Assert.That(size.Height, Is.EqualTo(142));
        }
    }

    [Test]
    public void Measure_Skew_ExpandsBoundingBox()
    {
        // Прямоугольник 100x50 со skewX=45° даёт сдвиг верхнего края на 50 пикселей,
        // bbox становится шире.
        var node = new TransformNode(new Transform(1f, 1f, 0f, 45f, 0f, new Point(0, 0)))
        {
            Child = new FixedNode(100, 50)
        };

        var size = node.Measure(Size.Infinity);

        // SkewX=45° → tan(45)=1. Верхняя грань сдвигается вправо на 50*1 = 50.
        // Нижняя грань остаётся в x=0..100. bbox: x от 0 до 150 → 150.
        Assert.That(size.Width, Is.EqualTo(150));
    }

    [Test]
    public void Measure_NullChild_ReturnsPaddingOnly()
    {
        var node = new TransformNode(Transform.Identity)
        {
            Padding = new Thickness(10, 20)
        };

        var size = node.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(20));
            Assert.That(size.Height, Is.EqualTo(40));
        }
    }

    // ---------------------------------------------------------------
    // Arrange
    // ---------------------------------------------------------------

    [Test]
    public void Arrange_Identity_ChildAtSlotOrigin()
    {
        var child = new FixedNode(100, 50) { HAlignment = Alignment.Start, VAlignment = Alignment.Start };
        var node = new TransformNode(Transform.Identity) { Child = child };

        _ = node.Measure(new Size(200, 200));
        node.Arrange(new Rect(0, 0, 200, 200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.Bounds.X, Is.EqualTo(0));
            Assert.That(child.Bounds.Y, Is.EqualTo(0));
            Assert.That(child.Bounds.Width, Is.EqualTo(100));
            Assert.That(child.Bounds.Height, Is.EqualTo(50));
        }
    }

    [Test]
    public void Arrange_Scale2x_ChildCentered()
    {
        // При scale 2x от центра bounding box = 200x100.
        // Untransformed child 100x50 должен лежать так, чтобы после scale от центра
        // он заполнил весь bbox. Значит, child сдвинут на (100-100)/2 = 0 по X? Нет.
        // ox=50, minX = -50*2 = -100. dx = -50 - (-100) = 50.
        // То есть child сдвинут на 50 вправо от левого верхнего угла slot.
        var child = new FixedNode(100, 50) { HAlignment = Alignment.Start, VAlignment = Alignment.Start };
        var node = new TransformNode(new Transform(2f, 2f, 0f, new Point(50, 50))) { Child = child };

        _ = node.Measure(new Size(300, 300));
        node.Arrange(new Rect(0, 0, 300, 300));

        // ox=50, oy=25. minX = -50*2 = -100, minY = -25*2 = -50.
        // dx = -50 - (-100) = 50. dy = -25 - (-50) = 25.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.Bounds.X, Is.EqualTo(50));
            Assert.That(child.Bounds.Y, Is.EqualTo(25));
            Assert.That(child.Bounds.Width, Is.EqualTo(100));
            Assert.That(child.Bounds.Height, Is.EqualTo(50));
        }
    }

    [Test]
    public void Arrange_WithPadding_OffsetsByPadding()
    {
        var child = new FixedNode(100, 50) { HAlignment = Alignment.Start, VAlignment = Alignment.Start };
        var node = new TransformNode(Transform.Identity)
        {
            Child = child,
            Padding = new Thickness(10, 20)
        };

        _ = node.Measure(new Size(200, 200));
        node.Arrange(new Rect(0, 0, 200, 200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.Bounds.X, Is.EqualTo(10));
            Assert.That(child.Bounds.Y, Is.EqualTo(20));
        }
    }

    // ---------------------------------------------------------------
    // Child property
    // ---------------------------------------------------------------

    [Test]
    public void Child_SetAddsToChildren()
    {
        var child = new FixedNode(10, 10);
        var node = new TransformNode(Transform.Identity) { Child = child };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(node.Children.Count, Is.EqualTo(1));
            Assert.That(node.Children[0], Is.SameAs(child));
            Assert.That(node.Child, Is.SameAs(child));
        }
    }

    [Test]
    public void Child_SetTwice_ReplacesChild()
    {
        var first = new FixedNode(10, 10);
        var second = new FixedNode(20, 20);

        var node = new TransformNode(Transform.Identity) { Child = first };
        node.Child = second;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(node.Children.Count, Is.EqualTo(1));
            Assert.That(node.Children[0], Is.SameAs(second));
        }
    }

    [Test]
    public void Child_Null_RemovesAll()
    {
        var node = new TransformNode(Transform.Identity) { Child = new FixedNode(10, 10) };
        node.Child = null;

        Assert.That(node.Children.Count, Is.EqualTo(0));
    }
}