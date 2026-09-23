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
using DisplayNodes.Fluent;

using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class ConditionalNodeTests
{
    // ---------------------------------------------------------------
    // Конструктор
    // ---------------------------------------------------------------

    [Test]
    public void Constructor_NullCondition_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(
            () => new ConditionalNode(null!, new FixedNode(10, 10), new FixedNode(20, 20)));
    }

    [Test]
    public void Constructor_AddsBothNodesToChildren()
    {
        var trueNode = new FixedNode(10, 10);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, falseNode);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cond.Children.Count, Is.EqualTo(2));
            Assert.That(cond.Children[0], Is.SameAs(trueNode));
            Assert.That(cond.Children[1], Is.SameAs(falseNode));
        }
    }

    [Test]
    public void Constructor_NullTrueNode_OnlyFalseInChildren()
    {
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(false), null, falseNode);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cond.Children.Count, Is.EqualTo(1));
            Assert.That(cond.Children[0], Is.SameAs(falseNode));
        }
    }

    [Test]
    public void Constructor_NullFalseNode_OnlyTrueInChildren()
    {
        var trueNode = new FixedNode(10, 10);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cond.Children.Count, Is.EqualTo(1));
            Assert.That(cond.Children[0], Is.SameAs(trueNode));
        }
    }

    // ---------------------------------------------------------------
    // ActiveNode
    // ---------------------------------------------------------------

    [Test]
    public void ActiveNode_TrueCondition_ReturnsTrueNode()
    {
        var trueNode = new FixedNode(10, 10);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, falseNode);

        Assert.That(cond.ActiveNode, Is.SameAs(trueNode));
    }

    [Test]
    public void ActiveNode_FalseCondition_ReturnsFalseNode()
    {
        var trueNode = new FixedNode(10, 10);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(false), trueNode, falseNode);

        Assert.That(cond.ActiveNode, Is.SameAs(falseNode));
    }

    [Test]
    public void ActiveNode_ChangesWithCondition()
    {
        var condition = new Observable<bool>(true);
        var trueNode = new FixedNode(10, 10);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(condition, trueNode, falseNode);

        Assert.That(cond.ActiveNode, Is.SameAs(trueNode));

        condition.Value = false;
        Assert.That(cond.ActiveNode, Is.SameAs(falseNode));

        condition.Value = true;
        Assert.That(cond.ActiveNode, Is.SameAs(trueNode));
    }

    // ---------------------------------------------------------------
    // Measure
    // ---------------------------------------------------------------

    [Test]
    public void Measure_TrueCondition_ReturnsTrueNodeSize()
    {
        var cond = new ConditionalNode(
            new Observable<bool>(true),
            new FixedNode(100, 50),
            new FixedNode(20, 20));

        var size = cond.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(100));
            Assert.That(size.Height, Is.EqualTo(50));
        }
    }

    [Test]
    public void Measure_FalseCondition_ReturnsFalseNodeSize()
    {
        var cond = new ConditionalNode(
            new Observable<bool>(false),
            new FixedNode(100, 50),
            new FixedNode(20, 20));

        var size = cond.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(20));
            Assert.That(size.Height, Is.EqualTo(20));
        }
    }

    [Test]
    public void Measure_NullActiveNode_ReturnsEmpty()
    {
        var cond = new ConditionalNode(new Observable<bool>(true), null, new FixedNode(20, 20));

        var size = cond.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(0));
            Assert.That(size.Height, Is.EqualTo(0));
        }
    }

    [Test]
    public void Measure_WithPadding_AddsPadding()
    {
        var cond = new ConditionalNode(
            new Observable<bool>(true),
            new FixedNode(100, 50),
            null)
        {
            Padding = new Thickness(10, 20)
        };

        var size = cond.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(120));   // 100 + 20
            Assert.That(size.Height, Is.EqualTo(90));   // 50 + 40
        }
    }

    [Test]
    public void Measure_InactiveNode_NotMeasured()
    {
        // Если бы неактивный узел измерялся, он бы попал в Children
        // и его DesiredSize изменился бы с 0 на своё значение.
        var trueNode = new FixedNode(100, 50);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, falseNode);
        _ = cond.Measure(Size.Infinity);

        // TrueNode измерен, FalseNode — нет.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(trueNode.DesiredSize.Width, Is.EqualTo(100));
            Assert.That(falseNode.DesiredSize.Width, Is.EqualTo(0));   // не измерен
        }
    }

    // ---------------------------------------------------------------
    // Arrange
    // ---------------------------------------------------------------

    [Test]
    public void Arrange_TrueCondition_ArrangesTrueNode()
    {
        var trueNode = new FixedNode(100, 50);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, falseNode);
        _ = cond.Measure(new Size(200, 200));
        cond.Arrange(new Rect(0, 0, 200, 200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trueNode.Bounds.Width, Is.EqualTo(100));
            Assert.That(trueNode.Bounds.Height, Is.EqualTo(50));
            Assert.That(falseNode.Bounds.Width, Is.EqualTo(0));
        }
    }

    [Test]
    public void Arrange_FalseCondition_ArrangesFalseNode()
    {
        var trueNode = new FixedNode(100, 50);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(false), trueNode, falseNode);
        _ = cond.Measure(new Size(200, 200));
        cond.Arrange(new Rect(0, 0, 200, 200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trueNode.Bounds.Width, Is.EqualTo(0));
            Assert.That(falseNode.Bounds.Width, Is.EqualTo(20));
        }
    }

    // ---------------------------------------------------------------
    // Событие ConditionChanged
    // ---------------------------------------------------------------

    [Test]
    public void ConditionChanged_FiresOnToggle()
    {
        var condition = new Observable<bool>(true);
        var cond = new ConditionalNode(condition, new FixedNode(10, 10), new FixedNode(20, 20));

        bool? lastValue = null;
        int count = 0;
        cond.OnChanged(v => { lastValue = v; count++; });

        condition.Value = false;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(count, Is.EqualTo(1));
            Assert.That(lastValue, Is.False);
        }
    }

    [Test]
    public void ConditionChanged_NotFiredWhenValueUnchanged()
    {
        var condition = new Observable<bool>(true);
        var cond = new ConditionalNode(condition, new FixedNode(10, 10), null);

        int count = 0;
        cond.OnChanged(_ => count++);

        condition.Value = true;   // то же значение

        Assert.That(count, Is.EqualTo(0));
    }

    // ---------------------------------------------------------------
    // Dispose
    // ---------------------------------------------------------------

    [Test]
    public void Dispose_UnsubscribesFromCondition()
    {
        var condition = new Observable<bool>(true);
        var cond = new ConditionalNode(condition, new FixedNode(10, 10), new FixedNode(20, 20));

        int count = 0;
        cond.OnChanged(_ => count++);

        cond.Dispose();

        condition.Value = false;   // не должно уведомить

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void DisposeTwice_IsIdempotent()
    {
        var cond = new ConditionalNode(new Observable<bool>(true), new FixedNode(10, 10), null);
        cond.Dispose();
        Assert.DoesNotThrow(() => cond.Dispose());
    }

    // ---------------------------------------------------------------
    // Интеграция: switch + повторный Measure
    // ---------------------------------------------------------------

    [Test]
    public void SwitchCondition_AndRemeasure_UpdatesSize()
    {
        var condition = new Observable<bool>(true);
        var cond = new ConditionalNode(
            condition,
            new FixedNode(100, 50),
            new FixedNode(20, 20));

        var firstSize = cond.Measure(Size.Infinity);
        Assert.That(firstSize.Width, Is.EqualTo(100));

        // Переключаем и пересчитываем.
        condition.Value = false;
        var secondSize = cond.Measure(Size.Infinity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondSize.Width, Is.EqualTo(20));
            Assert.That(secondSize.Height, Is.EqualTo(20));
        }
    }

    [Test]
    public void Arrange_ActiveNodeStretch_FillsSlot()
    {
        var trueNode = new StretchNode(100, 50);
        var falseNode = new FixedNode(20, 20);

        var cond = new ConditionalNode(new Observable<bool>(true), trueNode, falseNode);
        _ = cond.Measure(new Size(200, 200));
        cond.Arrange(new Rect(0, 0, 200, 200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trueNode.Bounds.Width, Is.EqualTo(200));
            Assert.That(trueNode.Bounds.Height, Is.EqualTo(200));
        }
    }
}