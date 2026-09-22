# Руководство по `Observable<T>`

Глубокое руководство по реактивной системе `Observable<T>` в DisplayNodes. Документ описывает внутреннее устройство, потокобезопасность, паттерны использования, типичные ошибки и интеграцию с виджетами.

## Обзор

`Observable<T>` — реактивное свойство, уведомляющее подписчиков об изменении значения. Используется для привязки данных к виджетам: при изменении `Value` все подписанные виджеты автоматически обновляются.

```csharp
var counter = new Observable<int>(0);
var label = UI.Label("0", font, brush)
    .BindText(new Observable<string>("0"));

counter.Subscribe(v => Console.WriteLine($"Counter: {v}"));
counter.Value = 10;  // выведет "Counter: 10"
```

## Устройство класса

### Поля и состояние

```csharp
public class Observable<T>
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

## Потокобезопасность

### Чтение и запись

`Observable<T>` потокобезопасен — можно читать и писать `Value` из любого потока:

```csharp
var counter = new Observable<int>(0);

// Чтение из фонового потока — безопасно
Task.Run(() =>
{
    int value = counter.Value;  // защищено lock
});

// Запись из фонового потока — безопасно
Task.Run(() =>
{
    counter.Value = 10;  // защищено lock
});
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

**Важно:** если подписчик обновляет UI, он должен использовать `Dispatcher.Invoke` или аналогичный механизм для переключения в UI-поток.

### Подписки и отписки

Операции `Subscribe` и `Unsubscribe` защищены `lock` — потокобезопасны:

```csharp
// Подписка из фонового потока — безопасно
var subscription = counter.Subscribe(v => Console.WriteLine(v));

// Отписка из другого потока — безопасно
Task.Run(() => subscription.Dispose());
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

**Особенности:**
- Начальное значение устанавливается сразу.
- Подписка регистрируется через `AddSubscription` — автоматически отписывается при `Dispose` виджета.
- Подписчик обновляет свойство компонента.

### Привязка нескольких свойств

Виджет может иметь несколько привязок:

```csharp
var textObservable = new Observable<string>("Text");
var fontObservable = new Observable<IFont>(font);
var brushObservable = new Observable<IBrush>(brush);

var label = UI.Label("Text", font, brush)
    .BindText(textObservable)
    .BindFont(fontObservable)
    .BindBrush(brushObservable);

// Все свойства обновятся автоматически
textObservable.Value = "New text";
fontObservable.Value = newFont;
brushObservable.Value = newBrush;
```

### Привязка видимости

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Text", font, brush)
    .BindVisible(isVisible);

isVisible.Value = false;  // метка скроется
isVisible.Value = true;   // метка появится
```

**Реализация:**

```csharp
public static T BindVisible<T>(this T node, Observable<bool> source)
    where T : WidgetNode
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));
    
    node.Component.Visible = source.Value;
    _ = node.AddSubscription(source.Subscribe(v => node.Component.Visible = v));
    
    return node;
}
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

opacity.Value = 50.0;      // прозрачность 50%
brightness.Value = 20.0;   // яркость +20
contrast.Value = -10.0;    // контраст -10
```

### Привязка изображения

```csharp
var imageObservable = new Observable<IImage>(UI.ImageFromFile("default.png"));

var imageNode = UI.Image()
    .BindBitmap(imageObservable);

imageObservable.Value = UI.ImageFromFile("new.png");  // изображение обновится
```

### Привязка режима отображения

```csharp
var sizeModeObservable = new Observable<ImageSizeMode>(ImageSizeMode.Normal);

var imageNode = UI.Image(UI.ImageFromFile("icon.png"))
    .BindSizeMode(sizeModeObservable);

sizeModeObservable.Value = ImageSizeMode.Stretch;  // режим изменится
```

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

## Типичные паттерны

### Паттерн 1: Простая привязка

```csharp
var counter = new Observable<int>(0);

var label = UI.Label("0", font, brush)
    .BindText(new Observable<string>("0"));

counter.Subscribe(v =>
{
    // Обновить Observable<string> для label
});
```

### Паттерн 2: Множественные привязки

```csharp
var text = new Observable<string>("Text");
var font = new Observable<IFont>(defaultFont);
var brush = new Observable<IBrush>(defaultBrush);

var label = UI.Label("Text", font.Value, brush.Value)
    .BindText(text)
    .BindFont(font)
    .BindBrush(brush);
```

### Паттерн 3: Условная видимость

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Text", font, brush)
    .BindVisible(isVisible);

// Переключение видимости
isVisible.Value = !isVisible.Value;
```

### Паттерн 4: Цепочка преобразований

```csharp
var counter = new Observable<int>(0);
var counterText = new Observable<string>("0");

counter.Subscribe(v => counterText.Value = v.ToString());

var label = UI.Label("0", font, brush)
    .BindText(counterText);

counter.Value = 10;  // counterText обновится, label обновится
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
observable.Value = "Updated";  // изменение до привязки

var label = UI.Label("Text", font, brush)
    .BindText(observable);
// label.Text = "Updated" — корректно, но логика запутанная
```

**Решение:** устанавливать начальное значение в конструкторе `Observable`, изменять после привязки.

### Ошибка 4: Множественные привязки к одному Observable

```csharp
// Неправильно: две привязки к одному Observable
var observable = new Observable<string>("Text");

var label1 = UI.Label("Text", font, brush)
    .BindText(observable);

var label2 = UI.Label("Text", font, brush)
    .BindText(observable);

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

observable2.Subscribe(v =>
{
    Console.WriteLine($"observable2: {v}");
});

observable1.Value = 10;
// выведет "observable2: 20" — корректно, но логика сложная
```

**Решение:** избегать циклических зависимостей между `Observable`.

## Расширенные сценарии

### ComputedObservable (планируется)

Реактивное свойство, вычисляемое на основе других `Observable`:

```csharp
// Планируемая функциональность
var a = new Observable<int>(10);
var b = new Observable<int>(20);
var sum = ComputedObservable.Create(() => a.Value + b.Value);

// sum.Value = 30
// при изменении a или b — sum автоматически пересчитается
```

### ObservableList<T> (планируется)

Реактивный список, уведомляющий об изменениях:

```csharp
// Планируемая функциональность
var list = new ObservableList<string>();

list.Subscribe(change =>
{
    Console.WriteLine($"Changed: {change.Type}");
});

list.Add("Item 1");  // уведомит подписчиков
list.Remove("Item 1");  // уведомит подписчиков
```

### ConditionalNode (планируется)

Условный контейнер, отображающий одного из детей в зависимости от `Observable<bool>`:

```csharp
// Планируемая функциональность
var isVisible = new Observable<bool>(true);

var conditional = new ConditionalNode(
    isVisible,
    UI.Label("Visible", font, brush),
    UI.Label("Hidden", font, grayBrush)
);

isVisible.Value = false;  // отобразится второй label
```

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
    label.Component.Text = $"Value: {v}";  // аллокация строки
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
    observable.Value = 10;  // то же значение
    
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

## Заключение

`Observable<T>` — потокобезопасное реактивное свойство, уведомляющее подписчиков об изменении значения. Виджеты автоматически отписываются при `Dispose`, что предотвращает утечки памяти. Подписчики должны быть потокобезопасными и не блокировать поток надолго. Исключения в подписчиках перехватываются и игнорируются, что требует осторожности при отладке.