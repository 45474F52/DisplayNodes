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
public class ObservableListTests
{
    // ---------------------------------------------------------------
    // Базовые операции IList<T>
    // ---------------------------------------------------------------

    [Test]
    public void EmptyList_CountIsZero()
    {
        var list = new ObservableList<int>();
        Assert.That(list.Count, Is.EqualTo(0));
        Assert.That(list.IsReadOnly, Is.False);
    }

    [Test]
    public void Constructor_FromEnumerable_CopiesItems()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0], Is.EqualTo(1));
            Assert.That(list[2], Is.EqualTo(3));
        }
    }

    [Test]
    public void Constructor_NullEnumerable_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ObservableList<int>(null!));
    }

    [Test]
    public void Add_AppendsAndRaisesEvent()
    {
        var list = new ObservableList<int>();
        ListChange<int> last = default;
        int count = 0;
        list.Changed += c => { last = c; count++; };

        list.Add(42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(42));
            Assert.That(count, Is.EqualTo(1));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Add));
            Assert.That(last.NewIndex, Is.EqualTo(0));
            Assert.That(last.OldIndex, Is.EqualTo(-1));
            Assert.That(last.Item, Is.EqualTo(42));
        }
    }

    [Test]
    public void Insert_AtMiddle_ShiftsItems()
    {
        var list = new ObservableList<int>(new[] { 1, 3 });
        ListChange<int> last = default;
        list.Changed += c => last = c;

        list.Insert(1, 2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0], Is.EqualTo(1));
            Assert.That(list[1], Is.EqualTo(2));
            Assert.That(list[2], Is.EqualTo(3));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Insert));
            Assert.That(last.NewIndex, Is.EqualTo(1));
            Assert.That(last.Item, Is.EqualTo(2));
        }
    }

    [Test]
    public void Indexer_Set_ReplacesAndRaisesReplace()
    {
        var list = new ObservableList<string>(new[] { "a", "b", "c" });
        ListChange<string> last = default;
        list.Changed += c => last = c;

        list[1] = "B";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[1], Is.EqualTo("B"));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Replace));
            Assert.That(last.OldIndex, Is.EqualTo(1));
            Assert.That(last.NewIndex, Is.EqualTo(1));
            Assert.That(last.Item, Is.EqualTo("B"));
        }
    }

    [Test]
    public void Indexer_Set_SameValue_NoEvent()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        int count = 0;
        list.Changed += _ => count++;

        list[1] = 2;   // то же значение

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_FirstOccurrence()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3, 2 });
        ListChange<int> last = default;
        list.Changed += c => last = c;

        bool removed = list.Remove(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(removed, Is.True);
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[1], Is.EqualTo(3));    // первый 2 удалён, второй остался
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Remove));
            Assert.That(last.OldIndex, Is.EqualTo(1));
            Assert.That(last.NewIndex, Is.EqualTo(-1));
            Assert.That(last.Item, Is.EqualTo(2));
        }
    }

    [Test]
    public void Remove_NotPresent_ReturnsFalse_NoEvent()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        int count = 0;
        list.Changed += _ => count++;

        bool removed = list.Remove(99);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(removed, Is.False);
            Assert.That(count, Is.EqualTo(0));
        }
    }

    [Test]
    public void RemoveAt_RemovesByIndex()
    {
        var list = new ObservableList<int>(new[] { 10, 20, 30 });
        ListChange<int> last = default;
        list.Changed += c => last = c;

        list.RemoveAt(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(10));
            Assert.That(list[1], Is.EqualTo(30));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Remove));
            Assert.That(last.OldIndex, Is.EqualTo(1));
            Assert.That(last.Item, Is.EqualTo(20));
        }
    }

    [Test]
    public void Clear_EmptiesAndRaisesReset()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        ListChange<int> last = default;
        int count = 0;
        list.Changed += c => { last = c; count++; };

        list.Clear();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(count, Is.EqualTo(1));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Reset));
        }
    }

    [Test]
    public void Clear_EmptyList_NoEvent()
    {
        var list = new ObservableList<int>();
        int count = 0;
        list.Changed += _ => count++;

        list.Clear();

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Move_ShiftsItem()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3, 4 });
        ListChange<int> last = default;
        list.Changed += c => last = c;

        list.Move(0, 2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0], Is.EqualTo(2));
            Assert.That(list[1], Is.EqualTo(3));
            Assert.That(list[2], Is.EqualTo(1));
            Assert.That(list[3], Is.EqualTo(4));
            Assert.That(last.Type, Is.EqualTo(ListChangeType.Move));
            Assert.That(last.OldIndex, Is.EqualTo(0));
            Assert.That(last.NewIndex, Is.EqualTo(2));
            Assert.That(last.Item, Is.EqualTo(1));
        }
    }

    [Test]
    public void Move_SameIndex_NoEvent()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        int count = 0;
        list.Changed += _ => count++;

        list.Move(1, 1);

        Assert.That(count, Is.EqualTo(0));
    }

    // ---------------------------------------------------------------
    // Contains / IndexOf / CopyTo / Enumeration
    // ---------------------------------------------------------------

    [Test]
    public void Contains_And_IndexOf_Work()
    {
        var list = new ObservableList<string>(new[] { "a", "b", "c" });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(list.Contains("b"), Is.True);
            Assert.That(list.Contains("x"), Is.False);
            Assert.That(list.IndexOf("c"), Is.EqualTo(2));
            Assert.That(list.IndexOf("x"), Is.EqualTo(-1));
        }
    }

    [Test]
    public void CopyTo_CopiesElements()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        var array = new int[3];

        list.CopyTo(array, 0);

        Assert.That(array, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void ToList_ReturnsIndependentCopy()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        var snapshot = list.ToList();

        list.Add(4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshot.Count, Is.EqualTo(3));
            Assert.That(list.Count, Is.EqualTo(4));
        }
    }

    [Test]
    public void Linq_Works()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3, 4, 5 });
        int sum = list.Where(x => x % 2 == 1).Sum();

        Assert.That(sum, Is.EqualTo(9));  // 1 + 3 + 5
    }

    [Test]
    public void ToList_AllowsSafeIterationWithModification()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });
        var seen = new List<int>();

        // Через ToList() — безопасно.
        foreach (int item in list.ToList())
        {
            seen.Add(item);
            if (item == 1)
                list.Add(99);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(seen, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(list.Count, Is.EqualTo(4));
        }
    }

    [Test]
    public void Enumeration_ThrowsOnModification()
    {
        var list = new ObservableList<int>(new[] { 1, 2, 3 });

        // Модификация во время foreach бросает InvalidOperationException,
        // как в стандартном List<T>.
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (int item in list)
            {
                if (item == 1)
                    list.Add(99);
            }
        });
    }

    // ---------------------------------------------------------------
    // Событие вне lock'а
    // ---------------------------------------------------------------

    [Test]
    public void ChangedHandler_CanModifyList()
    {
        // Если бы событие вызывалось внутри lock'а, это привело бы к deadlock.
        var list = new ObservableList<int>();
        bool reentered = false;

        list.Changed += c =>
        {
            if (!reentered && c.Type == ListChangeType.Add)
            {
                reentered = true;
                list.Add(100);
            }
        };

        Assert.DoesNotThrow(() => list.Add(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(reentered, Is.True);
            Assert.That(list.Count, Is.EqualTo(2));
        }
    }

    [Test]
    public void ChangedHandler_Throws_DoesNotBreakChain()
    {
        var list = new ObservableList<int>();
        int count = 0;

        list.Changed += _ => throw new InvalidOperationException("test");
        list.Changed += _ => count++;

        Assert.DoesNotThrow(() => list.Add(1));
        Assert.That(count, Is.EqualTo(1));
    }

    // ---------------------------------------------------------------
    // Dispose
    // ---------------------------------------------------------------

    [Test]
    public void DisposeTwice_IsIdempotent()
    {
        var list = new ObservableList<int>();
        list.Dispose();
        Assert.DoesNotThrow(list.Dispose);
    }

    [Test]
    public void Dispose_MakesListUnusable()
    {
        var list = new ObservableList<int>();
        list.Dispose();

        _ = Assert.Throws<ObjectDisposedException>(() => { int _ = list.Count; });
        _ = Assert.Throws<ObjectDisposedException>(() => list.Add(1));
        _ = Assert.Throws<ObjectDisposedException>(() => { int _ = list[0]; });
        _ = Assert.Throws<ObjectDisposedException>(() => list.Clear());
        _ = Assert.Throws<ObjectDisposedException>(() => { foreach (int _ in list) { } });
    }

    [Test]
    public void Dispose_RemovesAllHandlers()
    {
        var list = new ObservableList<int>();
        int count = 0;
        list.Changed += _ => count++;

        list.Add(1);   // count = 1
        list.Dispose();

        // После Dispose нельзя вызывать Add, но событие точно не будет вызвано.
        Assert.That(count, Is.EqualTo(1));
    }
}