# Архитектура DisplayNodes

Документ описывает архитектуру DisplayNodes: модули, алгоритмы, жизненный цикл компонентов, потоки выполнения и взаимодействия между слоями.

## Обзор модулей

```
┌─────────────────────────────────────────────────────────────────┐
│                       DisplayNodes                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Kernel (ядро)                                           │   │
│  │  ├── Core/            типы, LayoutNode, контейнеры       │   │
│  │  ├── Core/Rendering/  интерфейсы бэкенда                 │   │
│  │  ├── Widgets/         листовые узлы (Label, Image, ...)  │   │
│  │  ├── Fluent/          UI-фабрика, DisplayRoot            │   │
│  │  └── Helpers/         методы расширения                  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                  │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Gdi (GDI+-реализации)                                   │   │
│  │  GdiFont, GdiBrush, GdiImage, GdiTextMeasurer, ...       │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                  │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Adapters (бэкенды рендеринга)                           │   │
│  │  ├── WinFormsAdapter/                                    │   │
│  │  └── ... (другие бэкенды)                                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                  │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Playground (IDE для скриптов)                           │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
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
| `StackLayoutNode` | Строка или столбец. Дети располагаются последовательно вдоль главной оси |
| `GridNode` | Сетка с произвольными размерами строк/колонок (Pixel, Auto, Star) |
| `UniformGridNode` | Равномерная сетка с фиксированным числом строк/колонок |
| `OverlayNode` | Все дети в одном слоте (друг поверх друга) |
| `FixedNode` | Узел с фиксированным размером (распорка) |

**Общее правило для контейнеров:** контейнер всегда занимает весь предоставленный слот (с учётом `Margin`). Переопределение `Arrange` помечается как `sealed`:

```csharp
public sealed override void Arrange(Rect finalRect)
{
    Rect inner = finalRect.Deflate(Margin);
    Bounds = inner;
    ArrangeOverride(inner);
}
```

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
    └── 3. Прибавить Margin к результату
           DesiredSize = MeasureOverride(inner) + Margin
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

// 4. Устанавливаются геометрия
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
    
    foreach (LayoutNode child in root.Children)
        ApplyRecursive(child, parent, components);
}
```

## Реактивность (Observable)

`Observable<T>` — реактивное свойство с подпиской на изменения.

### Устройство

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

### Потокобезопасность

- `Value` getter/setter защищены `lock`.
- Подписки/отписки защищены `lock`.
- Уведомления происходят **вне** `lock`, чтобы избежать deadlock.
- Исключения в подписчиках не прерывают цепочку.

### Привязка к виджетам

```csharp
public LabelNode BindText(Observable<string> observable)
{
    Component.Text = observable.Value ?? string.Empty;
    _ = AddSubscription(observable.Subscribe(v => Component.Text = v ?? string.Empty));
    return this;
}
```

**Важно:** подписка автоматически отписывается при `Dispose` виджета через `AddSubscription`.

### Подписка и отписка

```csharp
// Ручная подписка (требует явной отписки)
var subscription = observable.Subscribe(v => Console.WriteLine(v));
// ...
subscription.Dispose();  // отписка

// Автоматическая отписка через виджет
var label = UI.Label("text", font, brush).BindText(observable);
// при label.Dispose() подписка автоматически отписывается
```

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
});
```

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
| Фабрика (`UI.Font`, `UI.Brush`) | Consumer-код | Consumer-код |
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

См. `CUSTOM_ADAPTERS_GUIDELINE.md` для подробного руководства.

## Заключение

DisplayNodes — декларативная система компоновки UI с бэкенд-агностичной архитектурой. Ядро реализует двухпроходный алгоритм Measure/Arrange, реактивные свойства через `Observable<T>`, и абстрактные интерфейсы для ресурсов. Конкретные бэкенды (адаптеры) реализуют эти интерфейсы, клонируют ресурсы при установке и управляют жизненным циклом компонентов.