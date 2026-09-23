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

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class ComputedObservableTests
{
    [Test]
    public void Constructor_ComputesInitialValue()
    {
        var a = new Observable<int>(2);
        var b = new Observable<int>(3);

        var sum = new ComputedObservable<int>(() => a.Value + b.Value, a, b);

        Assert.That(sum.Value, Is.EqualTo(5));
    }

    [Test]
    public void ChangeDependency_RecomputesValue()
    {
        var a = new Observable<int>(2);
        var b = new Observable<int>(3);

        var sum = new ComputedObservable<int>(() => a.Value + b.Value, a, b);

        a.Value = 10;
        Assert.That(sum.Value, Is.EqualTo(13));
    }

    [Test]
    public void MultipleDependencies_AllTriggerRecompute()
    {
        var a = new Observable<int>(1);
        var b = new Observable<int>(1);
        var c = new Observable<int>(1);

        var product = new ComputedObservable<int>(() => a.Value * b.Value * c.Value, a, b, c);

        Assert.That(product.Value, Is.EqualTo(1));

        a.Value = 2;
        Assert.That(product.Value, Is.EqualTo(2));

        b.Value = 3;
        Assert.That(product.Value, Is.EqualTo(6));

        c.Value = 4;
        Assert.That(product.Value, Is.EqualTo(24));
    }

    [Test]
    public void SameValue_DoesNotNotifySubscribers()
    {
        var a = new Observable<int>(5);
        var computed = new ComputedObservable<int>(() => a.Value % 2, a);

        int notifyCount = 0;
        _ = computed.Subscribe(_ => notifyCount++);

        // 5 → 6, чётность меняется 1 → 0. Уведомление.
        a.Value = 6;
        Assert.That(notifyCount, Is.EqualTo(1));

        // 6 → 8, чётность та же 0 → 0. Без уведомления.
        a.Value = 8;
        Assert.That(notifyCount, Is.EqualTo(1));
    }

    [Test]
    public void Subscriber_NotifiedOnDependencyChange()
    {
        var a = new Observable<int>(1);
        var b = new Observable<int>(2);

        var sum = new ComputedObservable<int>(() => a.Value + b.Value, a, b);

        int lastValue = 0;
        _ = sum.Subscribe(v => lastValue = v);

        a.Value = 10;
        Assert.That(lastValue, Is.EqualTo(12));

        b.Value = 20;
        Assert.That(lastValue, Is.EqualTo(30));
    }

    [Test]
    public void Dispose_UnsubscribesFromDependencies()
    {
        var a = new Observable<int>(1);
        var computed = new ComputedObservable<int>(() => a.Value * 2, a);

        int notifyCount = 0;
        _ = computed.Subscribe(_ => notifyCount++);

        computed.Dispose();

        a.Value = 100;

        using (Assert.EnterMultipleScope())
        {
            // После Dispose computed не пересчитывается и не уведомляет.
            Assert.That(computed.Value, Is.EqualTo(2));      // старое значение
            Assert.That(notifyCount, Is.EqualTo(0));         // не уведомлён
        }
    }

    [Test]
    public void DisposeTwice_IsIdempotent()
    {
        var a = new Observable<int>(1);
        var computed = new ComputedObservable<int>(() => a.Value, a);

        computed.Dispose();
        Assert.DoesNotThrow(computed.Dispose);
    }

    [Test]
    public void NoDependencies_ValueIsStable()
    {
        var counter = 0;
        var computed = new ComputedObservable<int>(() => ++counter);

        Assert.That(computed.Value, Is.EqualTo(1));

        // Без зависимостей значение не пересчитывается само.
        Assert.That(computed.Value, Is.EqualTo(1));
    }

    [Test]
    public void Refresh_RecomputesManually()
    {
        int external = 1;
        var computed = new ComputedObservable<int>(() => external);

        Assert.That(computed.Value, Is.EqualTo(1));

        external = 42;
        computed.Refresh();

        Assert.That(computed.Value, Is.EqualTo(42));
    }

    [Test]
    public void ComputeThrows_SubscribersNotNotified()
    {
        var a = new Observable<int>(1);
        bool shouldThrow = false;

        var computed = new ComputedObservable<int>(
            () => shouldThrow ? throw new InvalidOperationException("test") : a.Value * 2,
            a);

        int notifyCount = 0;
        _ = computed.Subscribe(_ => notifyCount++);

        shouldThrow = true;
        Assert.DoesNotThrow(() => a.Value = 5);   // compute кинет, но Observable проглотит
        Assert.That(notifyCount, Is.EqualTo(0));
    }

    [Test]
    public void NullCompute_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(
            () => new ComputedObservable<int>(null!));
    }

    [Test]
    public void NullDependency_Ignored()
    {
        var a = new Observable<int>(1);
        var computed = new ComputedObservable<int>(() => a.Value, a, null!, a);

        // null пропущен, работает с a.
        a.Value = 5;
        Assert.That(computed.Value, Is.EqualTo(5));
    }

    [Test]
    public void ChainedComputed_Works()
    {
        var a = new Observable<int>(1);
        var doubled = new ComputedObservable<int>(() => a.Value * 2, a);
        var quadrupled = new ComputedObservable<int>(() => doubled.Value * 2, doubled);

        Assert.That(quadrupled.Value, Is.EqualTo(4));

        a.Value = 5;
        Assert.That(quadrupled.Value, Is.EqualTo(20));
    }

    [Test]
    public void DifferentTypes_DependenciesWork()
    {
        var num = new Observable<int>(10);
        var text = new Observable<string>("Hello");

        var result = new ComputedObservable<string>(
            () => text.Value + ": " + num.Value,
            num, text);

        Assert.That(result.Value, Is.EqualTo("Hello: 10"));

        num.Value = 20;
        Assert.That(result.Value, Is.EqualTo("Hello: 20"));

        text.Value = "World";
        Assert.That(result.Value, Is.EqualTo("World: 20"));
    }
}