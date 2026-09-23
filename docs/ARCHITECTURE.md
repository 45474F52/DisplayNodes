# Архитектура DisplayNodes

Документ описывает архитектуру DisplayNodes: модули, алгоритмы, жизненный цикл компонентов, потоки выполнения и взаимодействия между слоями.

## Обзор модулей

```
┌──────────────────────────────────────────────────────────┐
│                       DisplayNodes                       │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Kernel (ядро)                                     │  │
│  │  ├─ Core/            типы, LayoutNode, контейнеры  │  │
│  │  ├─ Core/Rendering/  интерфейсы бэкенда            │  │
│  │  ├─ Widgets/         листовые узлы (Label, Image…) │  │
│  │  ├─ Fluent/          UI-фабрика, DisplayRoot       │  │
│  │  └─ Helpers/         методы расширения             │  │
│  └────────────────────────────────────────────────────┘  │
│                              │                           │
│                              ▼                           │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Gdi (GDI+-реализации)                             │  │
│  │  GdiFont, GdiBrush, GdiImage, GdiTextMeasurer, …   │  │
│  └────────────────────────────────────────────────────┘  │
│                              │                           │
│                              ▼                           │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Adapters (бэкенды рендеринга)                     │  │
│  │  ├─ WinFormsAdapter/                               │  │
│  │  └─ … (другие бэкенды)                             │  │
│  └────────────────────────────────────────────────────┘  │
│                              │                           │
│                              ▼                           │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Playground (IDE для скриптов)                     │  │
│  └────────────────────────────────────────────────────┘  │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### Зависимости

- **Kernel** не зависит ни от GDI, ни от WinForms, ни от какого-либо бэкенда.
- **Gdi** зависит от `Kernel` и `System.Drawing`.
- **Адаптеры** зависят от `Kernel` и `Gdi`.
- **Playground** зависит от всех остальных модулей.

## Ядро (Kernel)

### Базовые типы

Все базовые типы — `readonly struct`, неизменяемые, реализуют `IEquatable<T>`, операторы `==`/`!=`, переопределяют `Equals`/`GetHashCode`/`ToString`.

| Тип | Назначение |
|---|---|
| `Point` | Точка в 2D-пространстве (X, Y) |
| `Size` | Размер (Width, Height) |
| `Rect` | Прямоугольник (X, Y, Width, Height) |
| `Thickness` | Отступы с четырёх сторон (Left, Top, Right, Bottom) |
| `Color` | Цвет в формате RGBA |
| `GridLength` | Размер строки/колонки в Grid (Pixel, Auto, Star) |
| `Percent` | Процентное значение [0..100] с валидацией |
| `GradientStop` | Точка градиента (Color + Percent Offset) |
| `Shadow` | Описание тени (OffsetX, OffsetY, BlurRadius, Color) |
| `Transform` | Описание аффинного преобразования (ScaleX, ScaleY, Rotation, SkewX, SkewY, Origin) |

### LayoutNode

Абстрактный базовый класс для всех узлов layout-системы. Реализует двухпроходный алгоритм.

```csharp
public abstract class LayoutNode
{
    // Свойства
    public Thickness Margin { get; set; }
    public Thickness Padding { get; set; }
    public Alignment HAlignment { get; set; }
    public Alignment VAlignment { get; set; }
    public int? MinWidth { get; set; }
    public int? MaxWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxHeight { get; set; }
    public double FlexWeight { get; set; }
    public Size DesiredSize { get; protected set; }
    public Rect Bounds { get; protected set; }
    public List<LayoutNode> Children { get; }

    // Публичный API
    public Size Measure(Size available);
    public virtual void Arrange(Rect finalRect);

    // Переопределяется в наследниках
    protected abstract Size MeasureOverride(Size available);
    protected abstract void ArrangeOverride(Rect finalRect);
}
```

### Контейнеры

Контейнеры — наследники `LayoutNode`, имеющие дочерние узлы.

| Контейнер | Поведение |
|---|---|
| `StackLayoutNode` | Строка или столбец. Дети располагаются последовательно вдоль главной оси. Поддерживает `FlexWeight` |
| `GridNode` | Сетка с произвольными размерами строк/колонок (Pixel, Auto, Star) |
| `UniformGridNode` | Равномерная сетка с фиксированным числом строк/колонок |
| `OverlayNode` | Все дети в одном слоте (друг поверх друга) |
| `WrapPanelNode` | Перенос детей на следующую строку/столбец при нехватке места |
| `ConditionalNode` | Отображает одно из двух поддеревьев по `Observable<bool>` |
| `TransformNode` | Применяет аффинное преобразование к единственному ребёнку |
| `FixedNode` | Узел с фиксированным размером (распорка) |
| `StretchNode` | Узел, растягивающийся вдоль обеих осей |

**Общее правило для контейнеров:** контейнер всегда занимает весь предоставленный слот (с учётом `Margin`). Переопределение `Arrange` помечается как `sealed`:

```csharp
public sealed override void Arrange(Rect finalRect)
{
    Rect inner = finalRect.Deflate(Margin);
    Bounds = inner;
    ArrangeOverride(inner);
}
```

Исключение — `LayoutNode` (базовый класс использует `virtual`) и виджеты (`FixedNode`/`StretchNode` — листовые, у них нет детей, но `Arrange` тоже может быть переопределён особым образом).

### Виджеты

Виджеты — листовые узлы, оборачивающие компонент рендерера (`IRenderComponent`).

```csharp
public abstract class WidgetNode : LayoutNode, IDisposable
{
    public IRenderComponent Component { get; }

    protected override void ArrangeOverride(Rect finalRect) => ApplyBounds();
    protected abstract void ApplyBounds();

    public IDisposable AddSubscription(IDisposable subscription);
    public void Dispose();
}
```

| Виджет | Назначение |
|---|---|
| `LabelNode` | Текстовая метка |
| `ImageNode` | Изображение |
| `BackgroundNode` | Фон (растягивается на весь слот, `DesiredSize = 0`) |
| `ClipNode` | Контейнер-маска (обрезает содержимое по форме) |

## Алгоритм Measure/Arrange

Двухпроходный алгоритм — основа layout-системы.

### Первый проход: Measure

Цель — вычислить `DesiredSize` каждого узла.

```
Measure(available)
    │
    ├── 1. Вычесть Margin из available
    │      inner = available - Margin.Horizontal/Vertical
    │
    ├── 2. Вызвать MeasureOverride(inner)
    │      (наследник вычисляет желаемый размер)
    │
    ├── 3. Применить ограничения MinWidth/MaxWidth/MinHeight/MaxHeight
    │      (см. раздел «Ограничения размеров»)
    │
    └── 4. Прибавить Margin к результату
           DesiredSize = clamped + Margin
```

**Контракт MeasureOverride:**
- Принимает доступное пространство (уже уменьшенное на `Margin`).
- Возвращает желаемый размер контента (без `Margin`).
- Может рекурсивно вызвать `child.Measure(...)` для детей.

**Пример: StackLayoutNode (вертикальный)**

```csharp
protected override Size MeasureOverride(Size available)
{
    var inner = available.Deflate(Padding);
    int main = 0, cross = 0;

    for (int i = 0; i < Children.Count; i++)
    {
        var s = Children[i].Measure(inner);
        main += s.Height;
        cross = Math.Max(cross, s.Width);
        if (i > 0) main += Spacing;
    }

    return new Size(cross + Padding.Horizontal, main + Padding.Vertical);
}
```

### Второй проход: Arrange

Цель — разместить узел и его детей в выделенном слоте.

```
Arrange(finalRect)
    │
    ├── 1. Вычесть Margin из finalRect
    │      inner = finalRect - Margin
    │
    ├── 2. Вычислить выровненный прямоугольник
    │      Bounds = AlignRect(DesiredSize, inner, HAlignment, VAlignment)
    │
    └── 3. Вызвать ArrangeOverride(Bounds)
           (наследник размещает детей)
```

**Контракт ArrangeOverride:**
- Принимает слот (уже уменьшенный на `Margin`, с учётом выравнивания).
- Должен разместить детей через `child.Arrange(rect)`.
- Не должен возвращать значение.

**Пример: OverlayNode**

```csharp
protected override void ArrangeOverride(Rect finalRect)
{
    Rect slot = finalRect.Deflate(Padding);
    foreach (LayoutNode child in Children)
        child.Arrange(slot);
}
```

### Выравнивание

Метод `AlignRect` вычисляет прямоугольник внутри слота на основе `DesiredSize` и выравнивания:

```csharp
private static Rect AlignRect(Size desired, Rect slot,
                              Alignment horizontal, Alignment vertical)
{
    int w = horizontal == Alignment.Stretch
        ? slot.Size.Width
        : Math.Min(desired.Width, slot.Size.Width);
    int h = vertical == Alignment.Stretch
        ? slot.Size.Height
        : Math.Min(desired.Height, slot.Size.Height);

    int x = horizontal switch
    {
        Alignment.Center => slot.Point.X + (slot.Size.Width - w) / 2,
        Alignment.End    => slot.Point.X + slot.Size.Width - w,
        _                => slot.Point.X
    };

    int y = vertical switch
    {
        Alignment.Center => slot.Point.Y + (slot.Size.Height - h) / 2,
        Alignment.End    => slot.Point.Y + slot.Size.Height - h,
        _                => slot.Point.Y
    };

    return new Rect(x, y, w, h);
}
```

### Полный цикл

```
┌──────────────────────────────────────────────────────────┐
│  root.Measure(Size(800, 600))                            │
│    │                                                     │
│    ├── child1.Measure(Size(800, 600))                    │
│    │     └── возвращает DesiredSize = Size(200, 50)      │
│    │                                                     │
│    ├── child2.Measure(Size(800, 600))                    │
│    │     └── возвращает DesiredSize = Size(300, 80)      │
│    │                                                     │
│    └── возвращает DesiredSize = Size(500, 130)           │
│                                                          │
│  root.Arrange(Rect(0, 0, 800, 600))                      │
│    │                                                     │
│    ├── child1.Arrange(Rect(0, 0, 200, 50))               │
│    │                                                     │
│    └── child2.Arrange(Rect(200, 0, 300, 80))             │
└──────────────────────────────────────────────────────────┘
```

## Ограничения размеров (Min/Max)

`LayoutNode` поддерживает четыре ограничения:

- `MinWidth` (`int?`)
- `MaxWidth` (`int?`)
- `MinHeight` (`int?`)
- `MaxHeight` (`int?`)

Применяются в базовом методе `Measure` **после** вызова `MeasureOverride`, но **до** добавления `Margin`.

```csharp
public Size Measure(Size available)
{
    Size inner = new Size(
        Math.Max(0, available.Width - Margin.Horizontal),
        Math.Max(0, available.Height - Margin.Vertical)
    );

    Size measured = MeasureOverride(inner);

    int w = measured.Width;
    int h = measured.Height;

    if (MinWidth.HasValue)  w = Math.Max(w, MinWidth.Value);
    if (MaxWidth.HasValue)  w = Math.Min(w, MaxWidth.Value);
    if (MinHeight.HasValue) h = Math.Max(h, MinHeight.Value);
    if (MaxHeight.HasValue) h = Math.Min(h, MaxHeight.Value);

    DesiredSize = new Size(w, h);
    return new Size(DesiredSize.Width + Margin.Horizontal, DesiredSize.Height + Margin.Vertical);
}
```

**Важно:**
- Ограничения применяются к `DesiredSize` контента, не к слоту.
- Если `MinWidth > MaxWidth` — ограничения не согласованы, побеждает `MaxWidth`.
- `Margin` прибавляется **после** ограничений — то есть `MaxWidth` ограничивает контент, а не полный размер с margin.
- Наследники (`MeasureOverride`) не знают об ограничениях — это ответственность базового класса.

**Пример:**

```csharp
UI.Label("Text", font, brush)
    .MinWidth(100)
    .MaxWidth(400)
    .MinHeight(30)
    .MaxHeight(200);
```

## Flex-механика в StackLayoutNode

`StackLayoutNode` поддерживает пропорциональное распределение свободного пространства через `LayoutNode.FlexWeight` (`double`, default `0`).

- `FlexWeight == 0` — узел фиксированного размера (не участвует в распределении).
- `FlexWeight > 0` — узел получает долю свободного места пропорционально весу.

### Алгоритм Measure

1. **Первый проход** — измеряются все дети с `available`. Flex-дети получают свой «естественный» размер (`DesiredSize`).
2. **Вычисление свободного места:**
   ```
   totalNatural = sum(DesiredSize всех детей по главной оси) + Spacing
   free = availableMain - totalNatural
   ```
3. **Распределение:**
   - Если `free > 0` — flex-дети растягиваются: `cm = natural + free * (weight / totalWeight)`.
   - Если `free < 0` — flex-дети сжимаются: `cm = max(0, natural - |free| * (weight / totalWeight))`. Fixed-дети не сжимаются.
   - Если `free == 0` — размеры не меняются.

### Алгоритм Arrange

Та же логика распределения, что в Measure. Размеры детей вдоль главной оси берутся из `DesiredSize` (для fixed) и пересчитываются (для flex). `MainAxisAlignment` применяется к `actualFree` **после** распределения flex.

### Пример

```csharp
UI.Column(spacing: 8)
    .Add(UI.Label("Fixed", font, brush))            // FlexWeight = 0
    .Add(UI.Label("Stretched", font, brush).Flex(1)) // забирает всё свободное место
    .Add(UI.Label("Double", font, brush).Flex(2));   // забирает 2 доли из 3
```

### StretchNode

`StretchNode` — публичный узел, растягивающийся вдоль обеих осей. Полезен как flex-ребёнок, когда нужно, чтобы ребёнок растянулся:

```csharp
public sealed class StretchNode : LayoutNode
{
    public int NaturalWidth { get; }
    public int NaturalHeight { get; }

    public StretchNode(int naturalWidth = 0, int naturalHeight = 0)
    {
        NaturalWidth = naturalWidth;
        NaturalHeight = naturalHeight;
        HAlignment = Alignment.Stretch;
        VAlignment = Alignment.Stretch;
    }

    protected override Size MeasureOverride(Size available) => new Size(NaturalWidth, NaturalHeight);
    protected override void ArrangeOverride(Rect finalRect) { }
}
```

`StretchNode` при Measure возвращает natural размер, при Arrange растягивается до слота через `HAlignment/VAlignment = Stretch`.

## Жизненный цикл компонента

Компонент рендерера (`IRenderComponent`) — это обёртка над нативным контролом бэкенда.

### Создание

```csharp
// 1. Фабрика создаёт компонент без родителя
ILabelComponent label = factory.CreateLabel();

// 2. Устанавливаются свойства
label.Text = "Hello";
label.Font = font;
label.ForegroundBrush = brush;

// 3. Компонент привязывается к родителю
label.Parent = parentComponent;  // автоматически добавляется в Controls родителя

// 4. Устанавливается геометрия
label.Location = new Point(10, 20);
label.Size = new Size(100, 50);
```

### Обновление

При изменении `Observable<T>` подписчик обновляет свойства компонента:

```csharp
observable.Subscribe(value => label.Text = value);
```

### Освобождение

```csharp
// 1. Отвязать от родителя
label.Parent = null;  // автоматически удаляется из Controls родителя

// 2. Освободить ресурсы
(label as IDisposable)?.Dispose();
```

### Методы Apply/Rebuild/Dispose

```csharp
// Apply — Measure + Arrange + привязка компонентов к parent
root.Apply(parent, location, size);

// Rebuild — удалить старые компоненты + Apply
root.Rebuild(parent, location, size);

// Dispose — удалить все компоненты + DisposeTree
root.Dispose();
```

**Реализация Apply:**

```csharp
public static void Apply(this LayoutNode root, IRenderComponent parent,
                         Point location, Size size)
{
    _ = root.Measure(size);
    root.Arrange(new Rect(location, size));

    var components = new List<IRenderComponent>();
    ApplyRecursive(root, parent, components);
    root.AppliedComponents = components;
}

private static void ApplyRecursive(LayoutNode root, IRenderComponent parent,
                                   ICollection<IRenderComponent> components)
{
    if (root is WidgetNode widget)
    {
        widget.Component.Parent = parent;
        components.Add(widget.Component);
        return;
    }

    if (root is ClipNode clip)
    {
        clip.Mask.Parent = parent;
        components.Add(clip.Mask);
        foreach (LayoutNode child in root.Children)
            ApplyRecursive(child, clip.Mask, components);
        return;
    }

    if (root is TransformNode)
    {
        throw new NotSupportedException(
            "TransformNode is not supported by current adapters yet.");
    }

    foreach (LayoutNode child in root.Children)
        ApplyRecursive(child, parent, components);
}
```

**Примечание:** `TransformNode` бросает `NotSupportedException` в `ApplyRecursive`, потому что текущие адаптеры не поддерживают трансформации. Layout для `TransformNode` работает корректно — исключение возникает только при попытке рендеринга.

## Реактивность (Observable)

### Observable<T>

`Observable<T>` — реактивное свойство с подпиской на изменения.

```csharp
public class Observable<T>
{
    private readonly object _lock = new object();
    private readonly List<Action<T>> _subscribers = new List<Action<T>>();
    private T _value;

    public T Value
    {
        get { lock (_lock) return _value; }
        set
        {
            Action<T>[] toNotify;
            lock (_lock)
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;  // не уведомляем, если значение не изменилось
                _value = value;
                toNotify = _subscribers.ToArray();  // копия для безопасности
            }
            foreach (var cb in toNotify)
            {
                try { cb(value); }
                catch { /* не ломаем цепочку */ }
            }
        }
    }

    public IDisposable Subscribe(Action<T> callback);
    internal void Unsubscribe(Action<T> callback);
}
```

**Потокобезопасность:**
- `Value` getter/setter защищены `lock`.
- Подписки/отписки защищены `lock`.
- Уведомления происходят **вне** `lock`, чтобы избежать deadlock.
- Исключения в подписчиках перехватываются и игнорируются.

### IObservableSource

`IObservableSource` — не-generic интерфейс для подписки на `Observable<T>` с разными `T`. Используется в `ComputedObservable<T>`.

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
    return Subscribe(v => callback(v));
}
```

**Boxing:** для value-type это упаковка при уведомлении. Для ссылочных типов — без оверхеда.

**Зачем нужен:** из-за инвариантности generic'ов в C# нельзя передать `Observable<int>`, `Observable<string>` и `Observable<bool>` в один `params Observable<object>[]`. Через `IObservableSource` это возможно.

### ComputedObservable<T>

`ComputedObservable<T>` — реактивное свойство, значение которого вычисляется из других источников.

```csharp
public class ComputedObservable<T> : Observable<T>, IDisposable
{
    public ComputedObservable(Func<T> compute, params IObservableSource[] dependencies);

    public void Refresh();  // ручной пересчёт

    public void Dispose();  // отписка от всех зависимостей
}
```

**Поведение:**
1. При создании значение вычисляется один раз (`ComputeInitial`).
2. При изменении любой зависимости вызывается `compute()`.
3. Если новое значение отличается от текущего — уведомляются подписчики.
4. При `Dispose` отписывается от всех зависимостей.

**Порядок вызовов:**
```
a.Value = 1  → computed.OnDependencyChanged → compute() → Value = ...
b.Value = 2  → computed.OnDependencyChanged → compute() → Value = ...
```

Оба пересчёта корректны, но второй — избыточен (см. ROADMAP → «Batch-обновления ComputedObservable»).

### ObservableList<T>

`ObservableList<T>` — реактивная коллекция, реализующая `IList<T>`.

```csharp
public class ObservableList<T> : IList<T>, IDisposable
{
    public event Action<ListChange<T>> Changed;

    // IList<T>
    public void Add(T item);
    public void Insert(int index, T item);
    public bool Remove(T item);
    public void RemoveAt(int index);
    public void Clear();
    public void Move(int oldIndex, int newIndex);  // расширение

    // ...
}
```

**Типы изменений (`ListChangeType`):** `Add`, `Insert`, `Remove`, `Replace`, `Move`, `Reset`.

**Описание изменения (`ListChange<T>`):** `Type`, `OldIndex`, `NewIndex`, `Item`.

**Особенности:**
- Потокобезопасна (внутренний `lock`).
- Событие `Changed` вызывается **вне lock'а**.
- Все подписчики вызываются отдельно через `GetInvocationList()` — исключение в одном не ломает цепочку.
- `GetEnumerator` возвращает enumerator `List<T>` (`foreach` бросает `InvalidOperationException` при модификации, как в `List<T>`).
- `ToList()` — безопасный снимок.
- `Dispose` делает коллекцию непригодной: любая операция бросает `ObjectDisposedException`.

### Подписка и отписка

```csharp
// Ручная подписка (требует явной отписки)
var subscription = observable.Subscribe(v => Console.WriteLine(v));
// ...
subscription.Dispose();

// Автоматическая отписка через виджет
var label = UI.Label("text", font, brush).BindText(observable);
// при label.Dispose() подписка автоматически отписывается
```

## Визуальные эффекты (API)

Три визуальных эффекта описаны в API ядра, но **не поддерживаются** текущими адаптерами. При попытке использования они бросают `NotSupportedException`.

### Градиенты

```csharp
public interface IBrushFactory
{
    IBrush CreateSolidBrush(Color color);

    IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops);
    IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops);
}
```

Координаты градиента — нормализованные (0..100). Стопы — `GradientStop(Color, Percent)`.

**GDI-описания:**
- `GdiLinearGradientBrush` — описание линейного градиента (Start, End, Stops).
- `GdiRadialGradientBrush` — описание радиального градиента (Center, Radius, Stops).

Конкретная GDI+ кисть создаётся в момент отрисовки через `CreateGdiBrush(RectangleF)`, потому что GDI+ не поддерживает относительные координаты.

**Ограничение:** `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException` для градиентных кистей — адаптеры их не поддерживают.

### Shadow

```csharp
public readonly struct Shadow
{
    public readonly int OffsetX;
    public readonly int OffsetY;
    public readonly int BlurRadius;
    public readonly Color Color;
}

public interface IEffectComponent
{
    double Opacity { get; set; }
    double Brightness { get; set; }
    double Contrast { get; set; }
    Shadow? Shadow { get; set; }  // ← новое
}
```

**Ограничение:** адаптеры бросают `NotSupportedException` при попытке установить не-null `Shadow`.

### TransformNode

```csharp
public readonly struct Transform
{
    public readonly float ScaleX;
    public readonly float ScaleY;
    public readonly float Rotation;  // градусы
    public readonly float SkewX;
    public readonly float SkewY;
    public readonly Point Origin;     // нормализованный (0..100)
}

public class TransformNode : LayoutNode
{
    public Transform Transform { get; set; }
    public LayoutNode Child { get; set; }

    protected override Size MeasureOverride(Size available);
    protected override void ArrangeOverride(Rect finalRect);
}
```

`TransformNode` **учитывается в layout**: `Measure` возвращает bounding box трансформированного ребёнка. Порядок трансформации — scale → skew → rotate, относительно `Origin`.

**Ограничение:** `ApplyRecursive` бросает `NotSupportedException` при обнаружении `TransformNode` — адаптеры не поддерживают трансформации.

## Бэкенд-агностичность

Ядро не знает о конкретных технологиях рендеринга. Все ресурсы представлены интерфейсами.

### Интерфейсы ресурсов

```csharp
public interface IFont { }
public interface IBrush { }
public interface IImage { int Width { get; } int Height { get; } }
public interface ITextFormat { }
public interface IGraphicsPath { }
```

### Фабрики

```csharp
public interface IFontFactory
{
    IFont Create(string family, float size, bool bold = false, bool italic = false);
}

public interface IBrushFactory
{
    IBrush CreateSolidBrush(Color color);
    IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops);
    IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops);
}

public interface IImageFactory
{
    IImage CreateFromFile(string path);
    IImage CreateFromStream(Stream stream);
    IImage CreateFromBytes(byte[] bytes);
}

public interface ITextMeasurer
{
    Size MeasureArea(string text, IFont font, int maxWidth = 0);
}
```

### Компоненты рендерера

```csharp
public interface IRenderComponent
{
    IRenderComponent Parent { get; set; }
    Point Location { get; set; }
    Size Size { get; set; }
    bool Visible { get; set; }
}

public interface ILabelComponent : IRenderComponent, IEffectComponent
{
    string Text { get; set; }
    IFont Font { get; set; }
    IBrush ForegroundBrush { get; set; }
    IBrush BackgroundBrush { get; set; }
    ITextFormat Format { get; set; }
}

public interface IImageComponent : IRenderComponent, IEffectComponent
{
    IImage Image { get; set; }
    ImageSizeMode SizeMode { get; set; }
}

public interface IMaskComponent : IRenderComponent { }

public interface ILayoutComponent : IRenderComponent
{
    void Refresh();
}

public interface IEffectComponent
{
    double Opacity { get; set; }
    double Brightness { get; set; }
    double Contrast { get; set; }
    Shadow? Shadow { get; set; }
}
```

### Фабрика виджетов

```csharp
public interface IWidgetFactory
{
    ILabelComponent CreateLabel();
    IImageComponent CreateImage();
    IMaskComponent CreateRectMask();
    IMaskComponent CreateCircleMask();
    IMaskComponent CreateEllipseMask();
    IMaskComponent CreateRoundedRectMask(float cornerRadius);
    IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder);
}
```

### Корневой контейнер

```csharp
public interface IRenderRoot : IDisposable
{
    ILayoutComponent Root { get; }
    void Build(LayoutNode node, Point location, Size size);
    void Clear();
}

public interface IRenderRootFactory
{
    IRenderRoot Create();
}
```

## Адаптеры

Адаптер — конкретная реализация бэкенда рендеринга.

### Структура адаптера

```
YourAdapter/
├── Adapter.cs                    # Точка входа (статический класс)
├── WidgetFactory.cs              # Реализация IWidgetFactory
├── RenderRootFactory.cs          # Реализация IRenderRootFactory
├── RenderRoot.cs                 # Реализация IRenderRoot
└── Components/
    ├── ComponentBase.cs          # Базовый класс для компонентов
    ├── Layout.cs                 # ILayoutComponent
    ├── Label.cs                  # ILabelComponent
    ├── Image.cs                  # IImageComponent
    └── Masks/
        ├── MaskBase.cs           # Базовый класс для масок
        ├── RectMask.cs
        ├── CircleMask.cs
        ├── EllipseMask.cs
        ├── RoundedRectMask.cs
        └── PathMask.cs
```

### Инициализация

```csharp
public static class Adapter
{
    public static void Initialize()
    {
        UI.Factory = new WidgetFactory();
        UI.Measurer = new GdiTextMeasurer();
        UI.BrushFactory = new GdiBrushFactory();
        UI.FontFactory = new GdiFontFactory();
        UI.ImageFactory = new GdiImageFactory();
    }
}
```

### Клонирование ресурсов

Адаптер **клонирует** ресурсы при установке, чтобы consumer-код сохранял владение оригиналами:

```csharp
public IFont Font
{
    get => new GdiFont(_label.Font);
    set
    {
        _ownedFont?.Dispose();
        var gdi = value.ToGdi();
        _ownedFont = gdi != null ? (Font)gdi.Clone() : null;
        _label.Font = _ownedFont;
    }
}
```

**Почему это важно:**
- Consumer-код может диспоузить оригинал в любой момент.
- Адаптер владеет своим клоном и диспоузит его при `Dispose`.
- Это защищает от использования освобождённых ресурсов.

### Управление ресурсами

```csharp
public void Dispose()
{
    _ownedFont?.Dispose();      // клон, созданный адаптером
    _ownedFormat?.Dispose();    // клон, созданный адаптером
    _label.Dispose();           // внутренний контрол
}
```

## Потоки выполнения

### UI-поток

Все компоненты рендерера должны использоваться только из потока, в котором они созданы (обычно UI-поток).

```csharp
// Правильно: изменение в UI-потоке
Dispatcher.Invoke(() =>
{
    label.Text = "New text";
});

// Неправильно: изменение из фонового потока
Task.Run(() =>
{
    label.Text = "New text";  // исключение!
});
```

### Observable

`Observable<T>` потокобезопасен — можно изменять `Value` из любого потока:

```csharp
var counter = new Observable<int>(0);

// Изменение из фонового потока — безопасно
Task.Run(() => counter.Value = 10);

// Но подписчик будет вызван в потоке, который изменил Value
counter.Subscribe(v =>
{
    // Этот код выполнится в потоке, который вызвал counter.Value = 10
    // Если это фоновый поток, нужно использовать Dispatcher.Invoke
    // для обновления UI
});
```

### ObservableList

`ObservableList<T>` потокобезопасен — все операции защищены `lock`. Событие `Changed` вызывается вне lock'а, что позволяет подписчику безопасно изменять коллекцию.

### GdiTextMeasurer

Использует `[ThreadStatic]` для кэширования `Bitmap` и `Graphics`:

```csharp
[ThreadStatic] private static Bitmap _bmp;
[ThreadStatic] private static Graphics _g;
```

Каждый поток имеет свой кэш. Это снижает нагрузку на GC, но при интенсивном использовании пула потоков может привести к утечке GDI-handles.

## Управление ресурсами

### Кто владеет ресурсами?

| Кто создал | Кто владеет | Кто диспоузит |
|---|---|---|
| Фабрика (`UI.Font`, `UI.SolidBrush`) | Consumer-код | Consumer-код |
| Адаптер (клон при установке) | Адаптер | Адаптер при `Dispose` |
| `Observable<T>` | Consumer-код | Consumer-код |
| Подписка (`Subscribe`) | Consumer-код | Consumer-код или виджет при `Dispose` |

### Dispose деревьев

```csharp
// Автоматический Dispose через DisplayRoot
var displayRoot = new DisplayRoot(factory);
displayRoot.Build(rootNode, location, size);
// ... использование ...
displayRoot.Dispose();  // освободит все компоненты

// Ручной Dispose дерева
rootNode.Dispose();  // освободит все компоненты в дереве
```

**Реализация Dispose:**

```csharp
public static void Dispose(this LayoutNode root)
{
    if (root.AppliedComponents != null)
    {
        foreach (var c in root.AppliedComponents)
        {
            c.Parent = null;
            (c as IDisposable)?.Dispose();
        }
        root.AppliedComponents.Clear();
        root.AppliedComponents = null;
    }
    root.DisposeTree();
}

public static void DisposeTree(this LayoutNode node)
{
    foreach (var child in node.Children)
        child.DisposeTree();
    if (node is IDisposable d)
        d.Dispose();
}
```

## Производительность

### Избегание лишних Measure/Arrange

```csharp
// Правильно: один вызов Apply
rootNode.Apply(parent, location, size);

// Неправильно: многократные вызовы
rootNode.Measure(size);
rootNode.Arrange(rect);
rootNode.Measure(newSize);  // лишний вызов
rootNode.Arrange(newRect);
```

### Кэширование в GdiTextMeasurer

`[ThreadStatic]` переменные кэшируют `Bitmap` и `Graphics`, чтобы не создавать их на каждый вызов `MeasureArea`.

### Оптимизация GridNode

В `CalculateDimensions` используются массивы `double[]` для хранения размеров строк/колонок. При частых перерисовках это нагружает GC. Рекомендуется переиспользовать приватные поля-массивы.

### Оптимизация подсветки синтаксиса (Playground)

- Debounce-таймер: `HighlightSlowMs` (300 мс) при обычном вводе, `HighlightFastMs` (50 мс) при Backspace/Delete/Enter.
- `WM_SETREDRAW` отключает перерисовку на время применения подсветки.
- Позиция скролла и каретки сохраняются/восстанавливаются через `EM_GETSCROLLPOS`/`EM_SETSCROLLPOS`.

## Расширение

### Создание своего контейнера

```csharp
public class MyContainer : LayoutNode
{
    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }

    protected override Size MeasureOverride(Size available)
    {
        // Вычислить DesiredSize на основе детей
        return new Size(width, height);
    }

    protected override void ArrangeOverride(Rect finalRect)
    {
        // Разместить детей через child.Arrange(rect)
    }
}
```

### Создание своего виджета

```csharp
public class MyWidget : WidgetNode
{
    public MyWidget(IRenderComponent component) : base(component) { }

    protected override Size MeasureOverride(Size available)
    {
        return new Size(100, 50);
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

### Создание своего бэкенда

См. [Гайдлайн по созданию адаптеров](CUSTOM_ADAPTERS_GUIDELINE.md) для подробного руководства.

## Заключение

DisplayNodes — декларативная система компоновки UI с бэкенд-агностичной архитектурой.
Ядро реализует двухпроходный алгоритм Measure/Arrange, реактивные свойства через `Observable<T>`,
и абстрактные интерфейсы для ресурсов. Конкретные бэкенды (адаптеры) реализуют эти интерфейсы,
клонируют ресурсы при установке и управляют жизненным циклом компонентов.