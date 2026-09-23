# Руководство по `Observable<T>`

Глубокое руководство по реактивной системе DisplayNodes. Документ описывает внутреннее устройство, потокобезопасность, паттерны использования, типичные ошибки и интеграцию с виджетами.

## Содержание

1. [Обзор](#overview)
2. [Observable\<T\>](#observable)
3. [IObservableSource](#source)
4. [ComputedObservable\<T\>](#computed)
5. [ObservableList\<T\>](#list)
6. [ConditionalNode](#conditional)
7. [Потокобезопасность](#threading)
8. [Обработка исключений](#exceptions)
9. [Привязка к виджетам](#bindings)
10. [Автоматическая отписка](#auto-unsubscribe)
11. [Типичные паттерны](#patterns)
12. [Типичные ошибки](#mistakes)
13. [Производительность](#performance)
14. [Тестирование](#testing)

---

<a id="overview"></a>
## Обзор

Реактивная система DisplayNodes состоит из трёх уровней:

| Компонент | Назначение |
|---|---|
| `Observable<T>` | Реактивное свойство с уведомлением подписчиков |
| `IObservableSource` | Не-generic интерфейс для унификации `Observable<T>` с разными `T` |
| `ComputedObservable<T>` | Вычисляемое свойство на основе других источников |
| `ObservableList<T>` | Реактивная коллекция с событиями изменений |

```csharp
var counter = new Observable<int>(0);
var label = UI.Label("0", font, brush).BindText(
    new ComputedObservable<string>(() => counter.Value.ToString(), counter));

counter.Value = 10;  // UI обновится автоматически
```

---

<a id="observable"></a>
## Observable\<T\>

### Устройство класса

```csharp
public class Observable<T> : IObservableSource
{
    private readonly object _lock = new object();
    private readonly List<Action<T>> _subscribers = new List<Action<T>>();
    private T _value;

    public Observable(T initialValue)
    {
        _value = initialValue;
    }
}
```

- `_lock` — объект для синхронизации доступа к `_value` и `_subscribers`.
- `_subscribers` — список callback-функций, вызываемых при изменении значения.
- `_value` — текущее значение.

### Свойство Value

```csharp
public T Value
{
    get
    {
        lock (_lock)
            return _value;
    }
    set
    {
        Action<T>[] toNotify;
        lock (_lock)
        {
            // Не уведомляем, если значение не изменилось
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;
            _value = value;
            // Копируем список для безопасности — подписчик может отписаться
            toNotify = _subscribers.ToArray();
        }
        // Уведомляем вне lock, чтобы избежать deadlock
        foreach (var cb in toNotify)
        {
            try
            {
                cb(value);
            }
            catch
            {
                // Исключения в подписчиках не ломают цепочку
            }
        }
    }
}
```

**Ключевые особенности:**
- Getter защищён `lock` — потокобезопасное чтение.
- Setter использует `EqualityComparer<T>.Default` для сравнения значений. Если новое значение равно текущему, уведомление не происходит.
- Список подписчиков копируется в массив внутри `lock`, чтобы подписчик мог безопасно отписаться во время уведомления.
- Уведомления происходят **вне** `lock`, чтобы избежать deadlock (подписчик может изменить другой `Observable`, который тоже использует `lock`).
- Исключения в подписчиках перехватываются и игнорируются — один сломанный подписчик не прерывает цепочку.

### Подписка и отписка

```csharp
public IDisposable Subscribe(Action<T> callback)
{
    if (callback == null)
        throw new ArgumentNullException(nameof(callback));
    lock (_lock)
        _subscribers.Add(callback);
    return new Subscription(this, callback);
}

internal void Unsubscribe(Action<T> callback)
{
    lock (_lock)
        _ = _subscribers.Remove(callback);
}
```

**Subscription** — вложенный класс, реализующий `IDisposable`:

```csharp
private class Subscription : IDisposable
{
    private Observable<T> _observable;
    private Action<T> _callback;

    public Subscription(Observable<T> observable, Action<T> callback)
    {
        _observable = observable;
        _callback = callback;
    }

    public void Dispose()
    {
        if (_observable != null && _callback != null)
        {
            _observable.Unsubscribe(_callback);
            _observable = null;
            _callback = null;
        }
    }
}
```

**Особенности:**
- `Subscribe` возвращает `IDisposable`, который нужно вызвать для отписки.
- `Unsubscribe` — внутренний метод, вызывается только через `Subscription.Dispose`.
- После `Dispose` ссылки на `_observable` и `_callback` обнуляются, чтобы избежать утечек памяти.

---

<a id="source"></a>
## IObservableSource

`IObservableSource` — не-generic интерфейс для подписки на `Observable<T>` с разными `T`.

```csharp
public interface IObservableSource
{
    IDisposable Subscribe(Action<object> callback);
}
```

`Observable<T>` реализует `IObservableSource` явно:

```csharp
IDisposable IObservableSource.Subscribe(Action<object> callback)
{
    if (callback == null)
        throw new ArgumentNullException(nameof(callback));

    return Subscribe(v => callback(v));
}
```

### Зачем нужен

Из-за **инвариантности generic'ов** в C# нельзя передать `Observable<int>`, `Observable<string>` и `Observable<bool>` в один `params Observable<object>[]`:

```csharp
// Не компилируется: Observable<int> не является Observable<object>
var intObs = new Observable<int>(0);
Observable<object> objObs = intObs;  // ошибка компиляции
```

Через `IObservableSource` это возможно:

```csharp
var intObs = new Observable<int>(0);
var strObs = new Observable<string>("");
var boolObs = new Observable<bool>(true);

IObservableSource[] sources = { intObs, strObs, boolObs };  // OK
```

### Boxing

Для value-type это упаковка при уведомлении. Для ссылочных типов — без оверхеда.

**Пример:**

```csharp
var intObs = new Observable<int>(0);
IObservableSource source = intObs;

source.Subscribe(obj => Console.WriteLine((int)obj));  // boxing при уведомлении
```

---

<a id="computed"></a>
## ComputedObservable\<T\>

`ComputedObservable<T>` — реактивное свойство, значение которого вычисляется из других источников.

```csharp
public class ComputedObservable<T> : Observable<T>, IDisposable
{
    private readonly object _lock = new object();
    private readonly Func<T> _compute;
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

    private bool _disposed;

    public ComputedObservable(Func<T> compute, params IObservableSource[] dependencies)
        : base(ComputeInitial(compute))
    {
        _compute = compute;

        if (dependencies != null)
        {
            foreach (IObservableSource source in dependencies)
            {
                if (source == null)
                    continue;

                _subscriptions.Add(source.Subscribe(OnDependencyChanged));
            }
        }
    }
}
```

### Поведение

1. **При создании** значение вычисляется один раз (`ComputeInitial`).
2. **При изменении любой зависимости** вызывается `compute()`.
3. **Если новое значение отличается** от текущего — уведомляются подписчики (через `Value = newValue` в базовом `Observable<T>`).
4. **При `Dispose`** отписывается от всех зависимостей.

### Пример

```csharp
var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName,
    lastName);

// fullName.Value == "Ivan Petrov"
firstName.Value = "Petr";
// fullName.Value == "Petr Petrov" (автоматически)
```

### Chained computed

```csharp
var a = new Observable<int>(1);
var doubled = new ComputedObservable<int>(() => a.Value * 2, a);
var quadrupled = new ComputedObservable<int>(() => doubled.Value * 2, doubled);

// quadrupled.Value == 4
a.Value = 5;
// quadrupled.Value == 20
```

### С разнотипными зависимостями

```csharp
var num = new Observable<int>(10);
var text = new Observable<string>("Hello");

var result = new ComputedObservable<string>(
    () => text.Value + ": " + num.Value,
    num, text);  // IObservableSource[] — работает

// result.Value == "Hello: 10"
num.Value = 20;
// result.Value == "Hello: 20"
```

### Refresh

Метод `Refresh()` позволяет вручную пересчитать значение, если зависимости изменились в обход `Observable<T>.Value`:

```csharp
int external = 1;
var computed = new ComputedObservable<int>(() => external);

// computed.Value == 1
external = 42;
computed.Refresh();
// computed.Value == 42
```

### Обработка исключений в compute

Если `compute()` бросает исключение, оно перехватывается, старое значение остаётся, подписчики не уведомляются:

```csharp
var computed = new ComputedObservable<int>(() => 
{
    if (someCondition) throw new InvalidOperationException();
    return 42;
}, someObservable);
```

### Ограничения

- **Циклические зависимости** не отслеживаются — если `ComputedObservable` зависит от себя же, будет бесконечная рекурсия. Это ответственность пользователя.
- **Batch-обновления** не реализованы — при изменении нескольких зависимостей в одном тике `compute()` вызывается несколько раз (см. ROADMAP).

### Dispose

```csharp
public void Dispose()
{
    lock (_lock)
    {
        if (_disposed)
            return;
        _disposed = true;

        foreach (IDisposable sub in _subscriptions)
        {
            try { sub?.Dispose(); }
            catch { }
        }
        _subscriptions.Clear();
    }
}
```

После `Dispose` `OnDependencyChanged` ничего не делает.

---

<a id="list"></a>
## ObservableList\<T\>

`ObservableList<T>` — реактивная коллекция, реализующая `IList<T>`.

```csharp
public class ObservableList<T> : IList<T>, IDisposable
{
    private readonly object _lock = new object();
    private readonly List<T> _items;
    private bool _disposed;

    public event Action<ListChange<T>> Changed;

    // IList<T>
    public int Count { get; }
    public bool IsReadOnly => false;
    public T this[int index] { get; set; }

    public void Add(T item);
    public void Insert(int index, T item);
    public bool Remove(T item);
    public void RemoveAt(int index);
    public void Clear();

    public bool Contains(T item);
    public int IndexOf(T item);
    public void CopyTo(T[] array, int arrayIndex);
    public IEnumerator<T> GetEnumerator();

    // Дополнительно
    public void Move(int oldIndex, int newIndex);
    public List<T> ToList();
}
```

### ListChangeType

```csharp
public enum ListChangeType
{
    Add,      // элемент добавлен в конец
    Insert,   // элемент вставлен по индексу
    Remove,   // элемент удалён по индексу
    Replace,  // элемент заменён по индексу
    Move,     // элемент перемещён
    Reset     // коллекция очищена
}
```

### ListChange\<T\>

```csharp
public readonly struct ListChange<T>
{
    public readonly ListChangeType Type;
    public readonly int OldIndex;  // -1, если не применимо
    public readonly int NewIndex;  // -1, если не применимо
    public readonly T Item;
}
```

**Соглашения:**

| Type | OldIndex | NewIndex | Item |
|---|---|---|---|
| `Add` | -1 | индекс | добавленный |
| `Insert` | -1 | индекс | вставленный |
| `Remove` | индекс | -1 | удалённый |
| `Replace` | индекс | индекс | новый |
| `Move` | откуда | куда | перемещённый |
| `Reset` | -1 | -1 | default |

### Использование

```csharp
var list = new ObservableList<string>();

list.Changed += change =>
{
    Console.WriteLine($"{change.Type} at {change.NewIndex}: {change.Item}");
};

list.Add("Item 1");          // Add at 0: Item 1
list.Add("Item 2");          // Add at 1: Item 2
list.Insert(1, "Inserted");  // Insert at 1: Inserted
list.RemoveAt(0);            // Remove at 0: Item 1
list[0] = "Replaced";        // Replace at 0: Replaced
list.Move(0, 1);             // Move [0→1]: Replaced
list.Clear();                // Reset
```

### Потокобезопасность

- Все операции защищены `lock`.
- Событие `Changed` вызывается **вне lock** — подписчик может безопасно изменять коллекцию из обработчика.
- Все подписчики вызываются **отдельно** через `GetInvocationList()` — исключение в одном не ломает цепочку.

**Реализация RaiseChanged:**

```csharp
private void RaiseChanged(ListChange<T> change)
{
    Action<ListChange<T>> handler = Changed;
    if (handler == null)
        return;

    Delegate[] invocationList = handler.GetInvocationList();

    for (int i = 0; i < invocationList.Length; i++)
    {
        try
        {
            ((Action<ListChange<T>>)invocationList[i])(change);
        }
        catch { }
    }
}
```

**Почему через `GetInvocationList`:** multicast delegate вызывает подписчиков последовательно внутри одного вызова. Если один бросает исключение — остальные не вызываются. `GetInvocationList` разбивает delegate на отдельные делегаты, каждый в своём `try/catch`.

### Итерация

- `GetEnumerator` возвращает enumerator `List<T>` — `foreach` бросает `InvalidOperationException` при модификации, как в стандартном `List<T>`.
- `ToList()` возвращает безопасный снимок — можно модифицировать коллекцию во время перебора.

```csharp
// Бросает InvalidOperationException при модификации
foreach (var item in list) { list.Add(...); }  // ошибка

// Безопасно
foreach (var item in list.ToList()) { list.Add(...); }  // OK
```

### Dispose

После `Dispose` любая операция (кроме повторного `Dispose`) бросает `ObjectDisposedException`:

```csharp
var list = new ObservableList<int>();
list.Dispose();

list.Add(1);  // ObjectDisposedException
int c = list.Count;  // ObjectDisposedException
foreach (var x in list) { }  // ObjectDisposedException
```

### UI-интеграция

`RepeaterNode` — контейнер, подписанный на `ObservableList<T>`, автоматически создающий/удаляющий дочерние `LayoutNode` при изменениях — **отложен** до реализации hot-swap поддеревьев (см. ROADMAP).

---

<a id="conditional"></a>
## ConditionalNode

`ConditionalNode` — контейнер, отображающий одно из двух поддеревьев по `Observable<bool>`. Реализует `IDisposable` — при `Dispose` отписывается от `Observable<bool>`.

```csharp
var isLoggedIn = new Observable<bool>(false);

var conditional = UI.When(
    isLoggedIn,
    trueNode: UI.Label("Welcome!", font, greenBrush),
    falseNode: UI.Label("Please log in", font, grayBrush));

// Подписка на переключение
conditional.OnChanged(value =>
{
    Console.WriteLine($"Condition changed to: {value}");
});
```

**Ограничение:** `ConditionalNode` **не пересчитывает layout автоматически** при переключении. Пользователь должен вызвать `Measure`+`Arrange`+`Refresh` вручную или перестроить дерево целиком.

Внутренне `ConditionalNode` хранит `Observable<bool>` и подписывается на него через `Subscribe`. При изменении вызывается `OnConditionChanged`, который уведомляет `ConditionChanged` (внутреннее событие). Fluent-метод `.OnChanged(...)` подписывается на это событие.

---

<a id="threading"></a>
## Потокобезопасность

### Чтение и запись

`Observable<T>` потокобезопасен — можно читать и писать `Value` из любого потока:

```csharp
var counter = new Observable<int>(0);

// Чтение из фонового потока — безопасно
Task.Run(() => { int value = counter.Value; });

// Запись из фонового потока — безопасно
Task.Run(() => counter.Value = 10);
```

### Уведомления подписчиков

Подписчики вызываются в том потоке, который изменил `Value`:

```csharp
var counter = new Observable<int>(0);

counter.Subscribe(v =>
{
    // Этот код выполнится в потоке, который вызвал counter.Value = 10
    // Если это фоновый поток, нужно использовать Dispatcher.Invoke
    // для обновления UI
});

// Изменение из UI-потока — подписчик вызывается в UI-потоке
counter.Value = 10;

// Изменение из фонового потока — подписчик вызывается в фоновом потоке
Task.Run(() => counter.Value = 20);
```

**Важно:** если подписчик обновляет UI, он должен использовать `Dispatcher.Invoke`:

```csharp
counter.Subscribe(v =>
{
    Dispatcher.Invoke(() => label.Component.Text = v.ToString());
});
```

### Подписки и отписки

Операции `Subscribe` и `Unsubscribe` защищены `lock` — потокобезопасны:

```csharp
var subscription = counter.Subscribe(v => Console.WriteLine(v));
Task.Run(() => subscription.Dispose());  // безопасно
```

### Копирование списка подписчиков

Список подписчиков копируется в массив внутри `lock`, чтобы подписчик мог безопасно отписаться во время уведомления:

```csharp
counter.Subscribe(v =>
{
    if (v > 100)
    {
        // Отписка во время уведомления — безопасно
        // (мы уже в копии массива)
    }
});
```

---

<a id="exceptions"></a>
## Обработка исключений

Исключения в подписчиках перехватываются и игнорируются:

```csharp
var observable = new Observable<int>(0);

observable.Subscribe(_ => throw new InvalidOperationException("Test"));
observable.Subscribe(v => Console.WriteLine($"Value: {v}"));

observable.Value = 10;  // выведет "Value: 10", исключение проигнорировано
```

**Почему это важно:**
- Один сломанный подписчик не прерывает цепочку.
- Другие подписчики получают уведомление.
- Исключение не распространяется на вызывающий код.

**Недостаток:**
- Ошибки в подписчиках скрываются — сложно отлаживать.
- Рекомендуется логировать исключения в подписчиках:

```csharp
observable.Subscribe(v =>
{
    try
    {
        // Логика
    }
    catch (Exception ex)
    {
        AppLog.Error("Observable subscriber", ex);
    }
});
```

---

<a id="bindings"></a>
## Привязка к виджетам

Виджеты предоставляют методы `Bind*` для привязки свойств к `Observable<T>`.

### Базовая привязка

```csharp
var textObservable = new Observable<string>("Initial");

var label = UI.Label("Text", font, brush)
    .BindText(textObservable);

// UI обновится автоматически
textObservable.Value = "Updated";
```

**Реализация `BindText`:**

```csharp
public LabelNode BindText(Observable<string> observable)
{
    if (observable == null)
        throw new ArgumentNullException(nameof(observable));

    // Устанавливаем начальное значение
    Component.Text = observable.Value ?? string.Empty;

    // Подписываемся на изменения
    _ = AddSubscription(observable.Subscribe(v => Component.Text = v ?? string.Empty));

    return this;
}
```

### Привязка нескольких свойств

```csharp
var textObservable = new Observable<string>("Text");
var fontObservable = new Observable<IFont>(font);
var brushObservable = new Observable<IBrush>(brush);

var label = UI.Label("Text", font, brush)
    .BindText(textObservable)
    .BindFont(fontObservable)
    .BindForegroundBrush(brushObservable);

textObservable.Value = "New text";
fontObservable.Value = newFont;
brushObservable.Value = newBrush;
```

### Привязка видимости

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Text", font, brush)
    .BindVisible(isVisible);

isVisible.Value = false;
isVisible.Value = true;
```

### Привязка эффектов

```csharp
var opacity = new Observable<double>(100.0);
var brightness = new Observable<double>(0.0);
var contrast = new Observable<double>(0.0);

var image = UI.Image(UI.ImageFromFile("photo.jpg"))
    .BindOpacity(opacity)
    .BindBrightness(brightness)
    .BindContrast(contrast);
```

### Привязка изображения

```csharp
var imageObservable = new Observable<IImage>(UI.ImageFromFile("default.png"));

var imageNode = UI.Image()
    .BindBitmap(imageObservable);

imageObservable.Value = UI.ImageFromFile("new.png");
```

### Привязка фона

```csharp
var bgObservable = new Observable<IBrush>(UI.SolidBrush(Color.Black));

var label = UI.Label("Text", font, brush)
    .BindBackgroundBrush(bgObservable);
```

### Таблица методов привязки

| Свойство | Метод привязки |
|---|---|
| `ILabelComponent.Text` | `BindText` |
| `ILabelComponent.Font` | `BindFont` |
| `ILabelComponent.ForegroundBrush` | `BindForegroundBrush` |
| `ILabelComponent.BackgroundBrush` | `BindBackgroundBrush` |
| `ILabelComponent.Format` | `BindFormat` |
| `ITextLayoutComponent.DrawMethod` | `BindDrawMethod` |
| `ITextLayoutComponent.Stretch` | `BindStretch` |
| `IImageComponent.Image` | `BindBitmap` |
| `IImageComponent.SizeMode` | `BindSizeMode` |
| `IRenderComponent.Visible` | `BindVisible` |
| `IEffectComponent.Opacity` | `BindOpacity` |
| `IEffectComponent.Brightness` | `BindBrightness` |
| `IEffectComponent.Contrast` | `BindContrast` |

**История переименований:**
- `BindBrush` → `BindForegroundBrush`.
- `BindFullBrush` → `BindBackgroundBrush`.

---

<a id="auto-unsubscribe"></a>
## Автоматическая отписка

Виджеты автоматически отписываются от всех `Observable` при `Dispose`.

### Механизм

```csharp
public abstract class WidgetNode : LayoutNode, IDisposable
{
    private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

    public IDisposable AddSubscription(IDisposable subscription)
    {
        if (subscription != null)
        {
            lock (_subscriptions)
                _subscriptions.Add(subscription);
        }
        return subscription;
    }

    public void Dispose()
    {
        lock (_subscriptions)
        {
            foreach (var s in _subscriptions)
                s?.Dispose();
            _subscriptions.Clear();
        }
    }
}
```

**Особенности:**
- `AddSubscription` регистрирует подписку в списке.
- `Dispose` отписывает все подписки.
- Список защищён `lock` — потокобезопасен.

### Пример

```csharp
var observable = new Observable<string>("Initial");

var label = UI.Label("Text", font, brush)
    .BindText(observable);

// Подписка зарегистрирована в label

label.Dispose();  // автоматически отписывается от observable

observable.Value = "Updated";  // label не обновится — подписка отписана
```

---

<a id="patterns"></a>
## Типичные паттерны

### Паттерн 1: Простая привязка

```csharp
var counter = new Observable<int>(0);

var label = UI.Label("0", font, brush)
    .BindText(new ComputedObservable<string>(
        () => counter.Value.ToString(),
        counter));
```

### Паттерн 2: Множественные привязки

```csharp
var text = new Observable<string>("Text");
var font = new Observable<IFont>(defaultFont);
var brush = new Observable<IBrush>(defaultBrush);

var label = UI.Label("Text", font.Value, brush.Value)
    .BindText(text)
    .BindFont(font)
    .BindForegroundBrush(brush);
```

### Паттерн 3: Условная видимость

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Text", font, brush)
    .BindVisible(isVisible);

isVisible.Value = !isVisible.Value;
```

### Паттерн 4: Цепочка преобразований

```csharp
var counter = new Observable<int>(0);
var counterText = new ComputedObservable<string>(
    () => counter.Value.ToString(),
    counter);

var label = UI.Label("0", font, brush)
    .BindText(counterText);

counter.Value = 10;  // counterText и label обновятся
```

### Паттерн 5: Ручная подписка

```csharp
var observable = new Observable<int>(0);

var subscription = observable.Subscribe(v =>
{
    Console.WriteLine($"Value: {v}");
});

// ... использование ...

subscription.Dispose();  // явная отписка
```

### Паттерн 6: Реактивная коллекция

```csharp
var items = new ObservableList<string>();

items.Changed += change =>
{
    if (change.Type == ListChangeType.Add)
        Console.WriteLine($"Added: {change.Item}");
    else if (change.Type == ListChangeType.Remove)
        Console.WriteLine($"Removed: {change.Item}");
};

items.Add("Item 1");   // Added: Item 1
items.Add("Item 2");   // Added: Item 2
items.RemoveAt(0);     // Removed: Item 1
```

### Паттерн 7: Вычисляемое свойство с множественными зависимостями

```csharp
var a = new Observable<int>(1);
var b = new Observable<int>(2);
var c = new Observable<int>(3);

var sum = new ComputedObservable<int>(
    () => a.Value + b.Value + c.Value,
    a, b, c);

// sum.Value == 6
a.Value = 10;
// sum.Value == 15
```

---

<a id="mistakes"></a>
## Типичные ошибки

### Ошибка 1: Забытая отписка

```csharp
// Неправильно: ручная подписка без отписки
var observable = new Observable<string>("value");
observable.Subscribe(v => label.Component.Text = v);
// утечка памяти: observable держит ссылку на label
```

**Решение:** использовать `Bind*` методы или сохранять `IDisposable` и вызывать `Dispose`.

### Ошибка 2: Изменение UI из фонового потока

```csharp
// Неправильно: изменение UI из фонового потока
Task.Run(() =>
{
    observable.Value = 10;  // подписчик вызовется в фоновом потоке
    // если подписчик обновляет UI — исключение
});
```

**Решение:** использовать `Dispatcher.Invoke` в подписчике:

```csharp
observable.Subscribe(v =>
{
    Dispatcher.Invoke(() => label.Component.Text = v.ToString());
});
```

### Ошибка 3: Изменение Observable в конструкторе виджета

```csharp
// Неправильно: изменение Observable до привязки
var observable = new Observable<string>("Initial");
observable.Value = "Updated";

var label = UI.Label("Text", font, brush).BindText(observable);
// label.Text = "Updated" — корректно, но логика запутанная
```

**Решение:** устанавливать начальное значение в конструкторе `Observable`, изменять после привязки.

### Ошибка 4: Множественные привязки к одному Observable

```csharp
var observable = new Observable<string>("Text");

var label1 = UI.Label("Text", font, brush).BindText(observable);
var label2 = UI.Label("Text", font, brush).BindText(observable);

// Оба label обновятся — корректно, но может быть неочевидно
```

**Решение:** использовать отдельные `Observable` для каждого виджета или документировать поведение.

### Ошибка 5: Изменение Observable в подписчике

```csharp
// Опасно: изменение Observable в подписчике
var observable1 = new Observable<int>(0);
var observable2 = new Observable<int>(0);

observable1.Subscribe(v =>
{
    observable2.Value = v * 2;  // изменение observable2 в подписчике observable1
});
```

**Решение:** избегать циклических зависимостей между `Observable`.

### Ошибка 6: Использование неправильного типа Observable

```csharp
// Неправильно: Observable<object> вместо Observable<IBrush>
var brush = new Observable<object>(UI.SolidBrush(Color.Red));
label.BindForegroundBrush(brush);  // ошибка компиляции
```

**Решение:** использовать конкретный тип `Observable<IBrush>`.

---

<a id="performance"></a>
## Производительность

### Избегание лишних уведомлений

`Observable<T>` не уведомляет подписчиков, если значение не изменилось:

```csharp
var observable = new Observable<int>(10);
int callCount = 0;

observable.Subscribe(_ => callCount++);

observable.Value = 10;  // не уведомит — значение не изменилось
observable.Value = 20;  // уведомит — значение изменилось

// callCount = 1
```

### Кэширование значений

Если вычисление значения дорогое, кэшируйте его:

```csharp
var observable = new Observable<string>("");

// Неправильно: дорогое вычисление на каждое изменение
counter.Subscribe(v =>
{
    observable.Value = ExpensiveComputation(v);
});

// Правильно: кэширование
var cache = new Dictionary<int, string>();
counter.Subscribe(v =>
{
    if (!cache.TryGetValue(v, out string result))
    {
        result = ExpensiveComputation(v);
        cache[v] = result;
    }
    observable.Value = result;
});
```

### Избегание аллокаций в подписчиках

```csharp
// Неправильно: аллокация строки на каждое уведомление
observable.Subscribe(v =>
{
    label.Component.Text = $"Value: {v}";
});

// Правильно: переиспользование буфера
var buffer = new StringBuilder();
observable.Subscribe(v =>
{
    buffer.Clear();
    buffer.Append("Value: ").Append(v);
    label.Component.Text = buffer.ToString();
});
```

### Batch-обновления

При изменении нескольких зависимостей в одном тике `ComputedObservable<T>` пересчитывается несколько раз. См. ROADMAP → «Batch-обновления ComputedObservable».

---

<a id="testing"></a>
## Тестирование

### Тест базовой функциональности

```csharp
[Test]
public void Observable_ValueChange_NotifiesSubscribers()
{
    var observable = new Observable<int>(0);
    int lastValue = -1;

    observable.Subscribe(v => lastValue = v);
    observable.Value = 10;

    Assert.That(lastValue, Is.EqualTo(10));
}
```

### Тест отсутствия уведомления при неизменном значении

```csharp
[Test]
public void Observable_SameValue_DoesNotNotify()
{
    var observable = new Observable<int>(10);
    int callCount = 0;

    observable.Subscribe(_ => callCount++);
    observable.Value = 10;

    Assert.That(callCount, Is.EqualTo(0));
}
```

### Тест отписки

```csharp
[Test]
public void Observable_Unsubscribe_StopsNotifications()
{
    var observable = new Observable<int>(0);
    int lastValue = -1;

    var subscription = observable.Subscribe(v => lastValue = v);
    observable.Value = 1;

    Assert.That(lastValue, Is.EqualTo(1));

    subscription.Dispose();
    observable.Value = 2;

    Assert.That(lastValue, Is.EqualTo(1));  // не изменилось
}
```

### Тест множественных подписчиков

```csharp
[Test]
public void Observable_MultipleSubscribers_AllNotified()
{
    var observable = new Observable<int>(0);
    int subscriber1 = 0, subscriber2 = 0;

    observable.Subscribe(v => subscriber1 = v);
    observable.Subscribe(v => subscriber2 = v);

    observable.Value = 42;

    Assert.That(subscriber1, Is.EqualTo(42));
    Assert.That(subscriber2, Is.EqualTo(42));
}
```

### Тест обработки исключений

```csharp
[Test]
public void Observable_ExceptionInSubscriber_DoesNotBreakChain()
{
    var observable = new Observable<int>(0);
    int lastValue = -1;

    observable.Subscribe(_ => throw new InvalidOperationException("Test"));
    observable.Subscribe(v => lastValue = v);

    Assert.DoesNotThrow(() => observable.Value = 10);
    Assert.That(lastValue, Is.EqualTo(10));
}
```

### Тест ComputedObservable

```csharp
[Test]
public void ComputedObservable_RecomputesOnDependencyChange()
{
    var a = new Observable<int>(2);
    var b = new Observable<int>(3);

    var sum = new ComputedObservable<int>(() => a.Value + b.Value, a, b);

    Assert.That(sum.Value, Is.EqualTo(5));

    a.Value = 10;
    Assert.That(sum.Value, Is.EqualTo(13));
}
```

### Тест ObservableList

```csharp
[Test]
public void ObservableList_AddRaisesChangedEvent()
{
    var list = new ObservableList<int>();
    ListChange<int> last = default;
    list.Changed += c => last = c;

    list.Add(42);

    using (Assert.EnterMultipleScope())
    {
        Assert.That(last.Type, Is.EqualTo(ListChangeType.Add));
        Assert.That(last.NewIndex, Is.EqualTo(0));
        Assert.That(last.Item, Is.EqualTo(42));
    }
}
```

### Тест потокобезопасности

```csharp
[Test]
public void ObservableList_ChangedHandler_CanModifyList()
{
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
```