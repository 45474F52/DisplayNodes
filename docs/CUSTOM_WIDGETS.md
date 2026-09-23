# Создание виджетов

Руководство по созданию собственных виджетов в DisplayNodes.  
Виджет — это листовой узел дерева, оборачивающий компонент рендерера (`IRenderComponent`). В отличие от контейнера, виджет не имеет дочерних узлов и отвечает за отрисовку конкретного элемента (текст, изображение, фон и т.д.).

## Содержание

1. [Отличие виджета от контейнера](#difference)
2. [Базовый класс WidgetNode](#widgetnode)
3. [Создание простого виджета](#simple)
4. [Примеры виджетов](#examples)
5. [StretchNode](#stretch)
6. [BackgroundNode и UI.Border](#background)
7. [Реактивные привязки](#bindings)
8. [Fluent-расширения](#fluent)
9. [Тестирование виджетов](#testing)
10. [Чек-лист создания виджета](#checklist)
11. [Управление ресурсами](#resources)

---

<a id="difference"></a>
## Отличие виджета от контейнера

| Характеристика | Контейнер (`LayoutNode`) | Виджет (`WidgetNode`) |
|---|---|---|
| Имеет дочерние узлы | Да | Нет |
| Отвечает за layout детей | Да | Нет |
| Оборачивает `IRenderComponent` | Нет | Да |
| Примеры | `StackLayoutNode`, `GridNode`, `OverlayNode` | `LabelNode`, `ImageNode`, `BackgroundNode` |

Контейнер вычисляет размеры и позиции своих детей. Виджет вычисляет собственный размер и записывает его в свойства компонента рендерера.

---

<a id="widgetnode"></a>
## Базовый класс WidgetNode

```csharp
public abstract class WidgetNode : LayoutNode, IDisposable
{
    public IRenderComponent Component { get; }

    protected WidgetNode(IRenderComponent component)
    {
        Component = component ?? throw new ArgumentNullException(nameof(component));
        HAlignment = Alignment.Start;
        VAlignment = Alignment.Start;
    }

    protected override void ArrangeOverride(Rect finalRect) => ApplyBounds();
    protected abstract void ApplyBounds();

    public IDisposable AddSubscription(IDisposable subscription);
    public void Dispose();
}
```

**Ключевые особенности:**
- `Component` — обёрнутый компонент рендерера (создаётся фабрикой `IWidgetFactory`).
- `ArrangeOverride` вызывает `ApplyBounds`, который наследники переопределяют для записи `Bounds` в свойства компонента.
- `AddSubscription` регистрирует подписку, которая автоматически отписывается при `Dispose`.
- `Dispose` освобождает все подписки.

---

<a id="simple"></a>
## Создание простого виджета

### Шаг 1: Определить интерфейс компонента (если нужен)

Если стандартных интерфейсов (`ILabelComponent`, `IImageComponent`) недостаточно, создайте свой:

```csharp
public interface IMyWidgetComponent : IRenderComponent
{
    string Caption { get; set; }
    int Value { get; set; }
}
```

### Шаг 2: Создать класс виджета

```csharp
public class MyWidgetNode : WidgetNode
{
    public new IMyWidgetComponent Component => (IMyWidgetComponent)base.Component;

    private Size? _fixedSize;

    public MyWidgetNode(string caption, int value, IMyWidgetComponent component)
        : base(component)
    {
        Component.Caption = caption ?? string.Empty;
        Component.Value = value;
    }

    public MyWidgetNode SetSize(int width, int height)
    {
        _fixedSize = new Size(width, height);
        return this;
    }

    protected override Size MeasureOverride(Size available)
    {
        if (_fixedSize.HasValue)
            return _fixedSize.Value;

        // Вычислить желаемый размер на основе содержимого
        return new Size(100, 50);
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

### Шаг 3: Добавить метод-фабрику в UI

```csharp
public static class UI
{
    public static MyWidgetNode MyWidget(string caption, int value)
    {
        if (Factory == null)
            throw new InvalidOperationException("UI.Factory is not initialized.");

        var component = Factory.CreateMyWidget();  // новый метод в IWidgetFactory
        return new MyWidgetNode(caption, value, component);
    }
}
```

### Шаг 4: Добавить метод в IWidgetFactory

```csharp
public interface IWidgetFactory
{
    // ... существующие методы ...
    IMyWidgetComponent CreateMyWidget();
}
```

### Шаг 5: Реализовать в адаптерах

Каждый адаптер должен предоставить реализацию `IMyWidgetComponent`.

### Шаг 6: Добавить fluent-расширения

```csharp
public static class MyWidgetNodeFluent
{
    public static MyWidgetNode Caption(this MyWidgetNode node, string caption)
    {
        node.Component.Caption = caption;
        return node;
    }

    public static MyWidgetNode Value(this MyWidgetNode node, int value)
    {
        node.Component.Value = value;
        return node;
    }
}
```

---

<a id="examples"></a>
## Примеры виджетов

### Пример 1: Виджет с фиксированным размером

```csharp
public class SpacerNode : WidgetNode
{
    private readonly Size _size;

    public SpacerNode(int width, int height, IRenderComponent component)
        : base(component)
    {
        _size = new Size(width, height);
    }

    protected override Size MeasureOverride(Size available) => _size;

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

**Особенности:**
- `MeasureOverride` всегда возвращает фиксированный размер.
- Не зависит от доступного пространства.
- Полезен для распорок и разделителей.

### Пример 2: Виджет с измеряемым содержимым

```csharp
public class TextNode : WidgetNode
{
    public new ILabelComponent Component => (ILabelComponent)base.Component;

    private readonly ITextMeasurer _measurer;
    private Size? _fixedSize;

    public TextNode(string text, IFont font, IBrush brush,
                    ILabelComponent component, ITextMeasurer measurer)
        : base(component)
    {
        _measurer = measurer ?? throw new ArgumentNullException(nameof(measurer));
        Component.Text = text ?? string.Empty;
        Component.Font = font;
        Component.ForegroundBrush = brush;
    }

    public TextNode SetSize(int width, int height)
    {
        _fixedSize = new Size(width, height);
        return this;
    }

    protected override Size MeasureOverride(Size available)
    {
        if (_fixedSize.HasValue)
            return _fixedSize.Value;

        if (string.IsNullOrEmpty(Component.Text) || Component.Font == null)
            return Size.Empty;

        int maxWidth = available.Width == int.MaxValue
            ? 0
            : Math.Max(0, available.Width);

        return _measurer.MeasureArea(Component.Text, Component.Font, maxWidth);
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

**Особенности:**
- Использует `ITextMeasurer` для измерения текста.
- Поддерживает фиксированный размер через `SetSize`.
- Если `maxWidth == 0`, текст измеряется в одну строку.

### Пример 3: Виджет с изображением

```csharp
public class PictureNode : WidgetNode
{
    public new IImageComponent Component => (IImageComponent)base.Component;

    private Size? _fixedSize;

    public PictureNode(IImage image, IImageComponent component)
        : base(component)
    {
        Component.Image = image;
    }

    public PictureNode SetSize(int width, int height)
    {
        _fixedSize = new Size(width, height);
        return this;
    }

    protected override Size MeasureOverride(Size available)
    {
        if (_fixedSize.HasValue)
            return _fixedSize.Value;

        if (Component.Image != null)
            return new Size(Component.Image.Width, Component.Image.Height);

        return Size.Empty;
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

**Особенности:**
- Размер по умолчанию — размер изображения.
- Можно переопределить через `SetSize`.

### Пример 4: Виджет-фон

```csharp
public class BackgroundNode : WidgetNode
{
    public new ILabelComponent Component => (ILabelComponent)base.Component;

    public BackgroundNode(IBrush brush, ILabelComponent component)
        : base(component)
    {
        Component.BackgroundBrush = brush ?? throw new ArgumentNullException(nameof(brush));

        Component.Text = string.Empty;
        if (Component is ITextLayoutComponent t)
            t.Stretch = LabelStretch.None;

        HAlignment = Alignment.Stretch;
        VAlignment = Alignment.Stretch;
    }

    protected override Size MeasureOverride(Size available) => Size.Empty;

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

**Особенности:**
- `MeasureOverride` возвращает `Size.Empty` — фон не занимает место в контейнере.
- `HAlignment` и `VAlignment` установлены в `Stretch` — фон растягивается на весь слот.
- Использует `ILabelComponent` как обёртку для отрисовки фона.
- Имеет перегрузку конструктора `BackgroundNode(Color, ILabelComponent, Func<Color, IBrush>)`, делегирующую в основной через `brushFactory`.

---

<a id="stretch"></a>
## StretchNode

`StretchNode` — публичный узел, растягивающийся вдоль обеих осей. Формально это не виджет в смысле `WidgetNode` (не оборачивает `IRenderComponent`), но часто используется как flex-ребёнок в `StackLayoutNode`.

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

    protected override Size MeasureOverride(Size available)
        => new Size(NaturalWidth, NaturalHeight);

    protected override void ArrangeOverride(Rect finalRect) { }
}
```

**Особенности:**
- При `Measure` возвращает natural размер.
- При `Arrange` растягивается до слота через `HAlignment/VAlignment = Stretch`.
- Не имеет `Component` — это чистый layout-узел.

**Использование как flex-ребёнок:**

```csharp
var column = UI.Column(0)
    .Add(UI.Label("Top", font, brush))
    .Add(new StretchNode(0, 0).Flex(1))   // растягивается на свободное место
    .Add(UI.Label("Bottom", font, brush));
```

**Использование в тестах:**

`StretchNode` — публичный класс, поэтому может использоваться в тестах для проверки flex-механики, распределения свободного пространства и т.д.

---

<a id="background"></a>
## BackgroundNode и UI.Border

`BackgroundNode` редко создаётся напрямую — обычно используется через `UI.Background(...)` или `UI.Border(...)`.

### UI.Background

```csharp
// Через цвет
var bg1 = UI.Background(Color.Blue);

// Через кисть
var bg2 = UI.Background(UI.SolidBrush(Color.FromArgb(240, 240, 245)));
```

### UI.Border — композиция

`UI.Border` — статический метод, возвращающий `OverlayNode` с фоном внутри.

```csharp
public static OverlayNode Border(IBrush background, int cornerRadius = 0)
{
    if (background == null)
        throw new ArgumentNullException(nameof(background));

    ClipNode clip = cornerRadius > 0
        ? ClipRoundedRect(cornerRadius)
        : Clip();

    clip.Add(Background(background));

    var overlay = new OverlayNode();
    overlay.Children.Add(clip);
    return overlay;
}

public static OverlayNode Border(Color color, int cornerRadius = 0)
    => Border(SolidBrush(color), cornerRadius);
```

**Как это работает:**

1. Создаётся `ClipNode` — либо прямоугольный (`cornerRadius == 0`), либо скруглённый.
2. Внутрь клипа добавляется `BackgroundNode` — он растянется на весь клип.
3. Всё оборачивается в `OverlayNode` — это позволяет добавлять контент **поверх** фона.

**Использование:**

```csharp
var card = UI.Border(Color.FromArgb(45, 45, 48), cornerRadius: 8)
    .Padding(12)
    .Add(UI.Label("Content", font, brush));
```

**Структура результирующего дерева:**

```
OverlayNode
├── ClipNode (с rounded-rect или rect маской)
│   └── BackgroundNode (заливка)
└── LabelNode (контент, добавленный пользователем)
```

Порядок детей в `OverlayNode` — порядок отрисовки: фон рисуется первым, контент — поверх.

---

<a id="bindings"></a>
## Реактивные привязки

Виджеты могут подписываться на `Observable<T>` для автоматического обновления при изменении данных.

### Базовая привязка

```csharp
public class CounterNode : WidgetNode
{
    public new ILabelComponent Component => (ILabelComponent)base.Component;

    private readonly ITextMeasurer _measurer;

    public CounterNode(ILabelComponent component, ITextMeasurer measurer)
        : base(component)
    {
        _measurer = measurer;
        Component.Text = "0";
    }

    public CounterNode BindCount(Observable<int> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        Component.Text = source.Value.ToString();
        _ = AddSubscription(source.Subscribe(v => Component.Text = v.ToString()));
        return this;
    }

    protected override Size MeasureOverride(Size available)
    {
        if (string.IsNullOrEmpty(Component.Text) || Component.Font == null)
            return Size.Empty;
        return _measurer.MeasureArea(Component.Text, Component.Font, 0);
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

**Использование:**

```csharp
var counter = new Observable<int>(0);

var counterNode = new CounterNode(factory.CreateLabel(), measurer)
    .BindCount(counter);

counter.Value = 10;  // UI обновится автоматически
```

**Особенности:**
- `AddSubscription` регистрирует подписку, которая автоматически отписывается при `Dispose`.
- Подписчик вызывается в том же потоке, который изменил `Value`.
- Если значение не изменилось (через `EqualityComparer<T>.Default`), уведомление не происходит.

### Привязка нескольких свойств

```csharp
public class StyledTextNode : WidgetNode
{
    public new ILabelComponent Component => (ILabelComponent)base.Component;

    private readonly ITextMeasurer _measurer;

    public StyledTextNode(ILabelComponent component, ITextMeasurer measurer)
        : base(component)
    {
        _measurer = measurer;
    }

    public StyledTextNode BindText(Observable<string> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        Component.Text = source.Value ?? string.Empty;
        _ = AddSubscription(source.Subscribe(v => Component.Text = v ?? string.Empty));
        return this;
    }

    public StyledTextNode BindFont(Observable<IFont> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        Component.Font = source.Value;
        _ = AddSubscription(source.Subscribe(v => Component.Font = v));
        return this;
    }

    public StyledTextNode BindForegroundBrush(Observable<IBrush> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        Component.ForegroundBrush = source.Value;
        _ = AddSubscription(source.Subscribe(v => Component.ForegroundBrush = v));
        return this;
    }

    protected override Size MeasureOverride(Size available)
    {
        if (string.IsNullOrEmpty(Component.Text) || Component.Font == null)
            return Size.Empty;
        return _measurer.MeasureArea(Component.Text, Component.Font, 0);
    }

    protected override void ApplyBounds()
    {
        Component.Location = Bounds.Point;
        Component.Size = Bounds.Size;
    }
}
```

### Привязка видимости

```csharp
public static class WidgetNodeFluent
{
    public static T BindVisible<T>(this T node, Observable<bool> source)
        where T : WidgetNode
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        node.Component.Visible = source.Value;
        _ = node.AddSubscription(source.Subscribe(v => node.Component.Visible = v));
        return node;
    }
}
```

### Соглашения об именовании

Для методов привязки используется префикс `Bind`:

| Свойство компонента | Метод привязки |
|---|---|
| `ILabelComponent.Text` | `BindText` |
| `ILabelComponent.Font` | `BindFont` |
| `ILabelComponent.ForegroundBrush` | `BindForegroundBrush` |
| `ILabelComponent.BackgroundBrush` | `BindBackgroundBrush` |
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

<a id="fluent"></a>
## Fluent-расширения

Fluent-расширения упрощают настройку виджетов и позволяют строить цепочки вызовов.

### Базовые расширения

```csharp
public static class WidgetNodeFluent
{
    public static T Margin<T>(this T node, Thickness m) where T : LayoutNode
    {
        node.Margin = m;
        return node;
    }

    public static T Padding<T>(this T node, Thickness p) where T : LayoutNode
    {
        node.Padding = p;
        return node;
    }

    public static T HAlignment<T>(this T node, Alignment a) where T : LayoutNode
    {
        node.HAlignment = a;
        return node;
    }

    public static T VAlignment<T>(this T node, Alignment a) where T : LayoutNode
    {
        node.VAlignment = a;
        return node;
    }
}
```

### Специализированные расширения

```csharp
public static class MyWidgetNodeFluent
{
    public static MyWidgetNode Caption(this MyWidgetNode node, string caption)
    {
        node.Component.Caption = caption;
        return node;
    }

    public static MyWidgetNode Value(this MyWidgetNode node, int value)
    {
        node.Component.Value = value;
        return node;
    }

    public static MyWidgetNode BindCaption(this MyWidgetNode node, Observable<string> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        node.Component.Caption = source.Value ?? string.Empty;
        _ = node.AddSubscription(source.Subscribe(v => node.Component.Caption = v ?? string.Empty));
        return node;
    }
}
```

### Fluent-расширения для LabelNode

```csharp
public static class LabelNodeFluent
{
    public static LabelNode BackgroundBrush(this LabelNode node, Color color)
    {
        if (UI.BrushFactory == null)
            throw new InvalidOperationException("UI.BrushFactory is not initialized.");

        node.Component.BackgroundBrush = UI.BrushFactory.CreateSolidBrush(color);
        return node;
    }

    public static LabelNode BackgroundBrush(this LabelNode node, IBrush brush)
    {
        node.Component.BackgroundBrush = brush;
        return node;
    }

    public static LabelNode Stretch(this LabelNode node, LabelStretch stretch)
    {
        if (node.Component is ITextLayoutComponent t)
            t.Stretch = stretch;
        return node;
    }

    public static LabelNode DrawMethod(this LabelNode node, TextDrawMethod method)
    {
        if (node.Component is ITextLayoutComponent t)
            t.DrawMethod = method;
        return node;
    }
}
```

**История переименований:**
- `FullBrush(Color)` → `BackgroundBrush(Color)`.
- `FullBrush(IBrush)` → `BackgroundBrush(IBrush)`.

---

<a id="testing"></a>
## Тестирование виджетов

### Обязательные сценарии

1. **Measure с фиксированным размером** (`SetSize`).
2. **Measure без фиксированного размера** — по содержимому.
3. **Arrange** — корректная запись `Location` и `Size` в компонент.
4. **Привязки** (`Bind*`) — обновление при изменении `Observable`.
5. **Dispose** — освобождение подписок.

### Пример теста

```csharp
[TestFixture]
public class MyWidgetNodeTests
{
    private IWidgetFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _factory = new WidgetFactory();  // из адаптера
    }

    [Test]
    public void Measure_WithFixedSize_ReturnsFixedSize()
    {
        var widget = new MyWidgetNode("Test", 42, _factory.CreateMyWidget());
        widget.SetSize(100, 50);
        var size = widget.Measure(Size.Infinity);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(100));
            Assert.That(size.Height, Is.EqualTo(50));
        }
        (widget as IDisposable)?.Dispose();
    }

    [Test]
    public void Arrange_WritesBoundsToComponent()
    {
        var widget = new MyWidgetNode("Test", 42, _factory.CreateMyWidget());
        widget.SetSize(100, 50);
        _ = widget.Measure(Size.Infinity);
        widget.Arrange(new Rect(10, 20, 100, 50));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(widget.Component.Location, Is.EqualTo(new Point(10, 20)));
            Assert.That(widget.Component.Size, Is.EqualTo(new Size(100, 50)));
        }
        (widget as IDisposable)?.Dispose();
    }

    [Test]
    public void BindCaption_UpdatesOnObservableChange()
    {
        var observable = new Observable<string>("Initial");
        var widget = new MyWidgetNode("", 0, _factory.CreateMyWidget());
        widget.BindCaption(observable);

        Assert.That(widget.Component.Caption, Is.EqualTo("Initial"));

        observable.Value = "Updated";
        Assert.That(widget.Component.Caption, Is.EqualTo("Updated"));

        (widget as IDisposable)?.Dispose();
    }

    [Test]
    public void Dispose_UnsubscribesFromObservable()
    {
        var observable = new Observable<string>("Initial");
        var widget = new MyWidgetNode("", 0, _factory.CreateMyWidget());
        widget.BindCaption(observable);

        (widget as IDisposable)?.Dispose();

        observable.Value = "Updated";
        // Подписка отписана — Caption не изменился
        Assert.That(widget.Component.Caption, Is.EqualTo("Initial"));
    }
}
```

---

<a id="checklist"></a>
## Чек-лист создания виджета

- [ ] Определён интерфейс компонента (если нужен).
- [ ] Создан класс виджета, наследник `WidgetNode`.
- [ ] Переопределён `MeasureOverride` — возвращает желаемый размер.
- [ ] Переопределён `ApplyBounds` — записывает `Bounds` в свойства компонента.
- [ ] Добавлен метод-фабрика в `UI` (статический класс).
- [ ] Добавлен метод в `IWidgetFactory`.
- [ ] Реализовано в адаптерах (каждый бэкенд предоставляет свою реализацию).
- [ ] Добавлены fluent-расширения.
- [ ] Написаны тесты (Measure, Arrange, привязки, Dispose).
- [ ] Публичный API покрыт XML-документацией.

---

<a id="resources"></a>
## Управление ресурсами

### Кто владеет компонентом?

- **Фабрика** (`IWidgetFactory.Create*`) создаёт компонент.
- **Виджет** владеет компонентом и диспоузит его при `Dispose`.
- **Адаптер** клонирует ресурсы (`Font`, `Brush`, `Image`) при установке, чтобы consumer-код сохранял владение оригиналами.

### Dispose

```csharp
// Автоматический Dispose через LayoutNodeFluent.Dispose
rootNode.Dispose();  // освободит все компоненты в дереве

// Ручной Dispose
(widget as IDisposable)?.Dispose();
```

**Реализация Dispose в WidgetNode:**

```csharp
public void Dispose()
{
    if (_disposed) return;
    _disposed = true;

    lock (_subscriptions)
    {
        foreach (var s in _subscriptions)
            s?.Dispose();
        _subscriptions.Clear();
    }
}
```

### Dispose в StretchNode

`StretchNode` — не `WidgetNode`, не реализует `IDisposable`. Он не владеет ресурсами (у него нет `Component`). `DisposeTree` его пропустит (проверка `is IDisposable`).

### Dispose в BackgroundNode

`BackgroundNode` — `WidgetNode`, реализует `Dispose` через базовый класс. При `Dispose` освобождается `Component` (если он `IDisposable`) и все подписки.

---

## Производительность

### Избегание лишних Measure

```csharp
// Правильно: один вызов Apply
rootNode.Apply(parent, location, size);

// Неправильно: многократные вызовы
rootNode.Measure(size);
rootNode.Arrange(rect);
rootNode.Measure(newSize);  // лишний вызов
rootNode.Arrange(newRect);
```

### Кэширование измерений

Если виджет измеряет текст или другие ресурсы, кэшируйте результаты:

```csharp
private Size? _cachedSize;
private string _cachedText;

protected override Size MeasureOverride(Size available)
{
    if (_cachedText == Component.Text && _cachedSize.HasValue)
        return _cachedSize.Value;

    _cachedSize = _measurer.MeasureArea(Component.Text, Component.Font, 0);
    _cachedText = Component.Text;
    return _cachedSize.Value;
}
```

### Избегание аллокаций

```csharp
// Правильно: переиспользовать приватные поля
private double[] _buffer = new double[8];

protected override Size MeasureOverride(Size available)
{
    Array.Resize(ref _buffer, Children.Count);
    // ...
}

// Неправильно: создавать массивы на каждый вызов
protected override Size MeasureOverride(Size available)
{
    var buffer = new double[Children.Count];  // аллокация на каждый Measure
    // ...
}
```