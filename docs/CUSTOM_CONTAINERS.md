# Создание контейнеров

Руководство по созданию собственных контейнеров в DisplayNodes.  
Контейнер — это узел layout-системы, имеющий дочерние узлы и отвечающий за их размещение. В отличие от виджета, контейнер не оборачивает компонент рендерера, а вычисляет позиции и размеры своих детей.

## Отличие контейнера от виджета

| Характеристика | Контейнер (`LayoutNode`) | Виджет (`WidgetNode`) |
|---|---|---|
| Имеет дочерние узлы | Да | Нет |
| Отвечает за layout детей | Да | Нет |
| Оборачивает `IRenderComponent` | Нет | Да |
| Примеры | `StackLayoutNode`, `GridNode`, `OverlayNode` | `LabelNode`, `ImageNode`, `BackgroundNode` |

Контейнер вычисляет размеры и позиции своих детей. Виджет вычисляет собственный размер и записывает его в свойства компонента рендерера.

## Базовый класс LayoutNode

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

**Ключевые особенности:**
- `Measure` вычисляет `DesiredSize` на основе доступного пространства.
- `Arrange` размещает узел в выделенном слоте.
- `MeasureOverride` и `ArrangeOverride` переопределяются в наследниках для реализации конкретной логики layout.

## Создание простого контейнера

### Шаг 1: Создать класс контейнера

```csharp
public class MyContainerNode : LayoutNode
{
    // Свойства контейнера
    public int Spacing { get; set; }
    
    // Контейнеры всегда занимают весь предоставленный слот (с учётом Margin).
    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }
    
    protected override Size MeasureOverride(Size available)
    {
        // Вычислить DesiredSize на основе детей
        var inner = available.Deflate(Padding);
        int totalWidth = 0;
        int maxHeight = 0;
        
        for (int i = 0; i < Children.Count; i++)
        {
            var childSize = Children[i].Measure(inner);
            totalWidth += childSize.Width;
            maxHeight = Math.Max(maxHeight, childSize.Height);
            if (i > 0)
                totalWidth += Spacing;
        }
        
        return new Size(
            totalWidth + Padding.Horizontal,
            maxHeight + Padding.Vertical
        );
    }
    
    protected override void ArrangeOverride(Rect finalRect)
    {
        // Разместить детей
        var slot = finalRect.Deflate(Padding);
        int offset = slot.Point.X;
        
        foreach (var child in Children)
        {
            var rect = new Rect(
                offset,
                slot.Point.Y,
                child.DesiredSize.Width,
                slot.Size.Height
            );
            child.Arrange(rect);
            offset += child.DesiredSize.Width + Spacing;
        }
    }
}
```

### Шаг 2: Добавить метод-фабрику в UI

```csharp
public static class UI
{
    public static MyContainerNode MyContainer(int spacing = 0)
        => new MyContainerNode { Spacing = spacing };
}
```

### Шаг 3: Добавить fluent-расширения (опционально)

```csharp
public static class MyContainerNodeFluent
{
    public static MyContainerNode Spacing(this MyContainerNode node, int spacing)
    {
        node.Spacing = spacing;
        return node;
    }
}
```

### Шаг 4: Написать тесты

```csharp
[TestFixture]
public class MyContainerNodeTests
{
    [Test]
    public void Measure_SumsChildrenWidths()
    {
        var container = new MyContainerNode { Spacing = 10 };
        container.Children.Add(new FixedNode(50, 30));
        container.Children.Add(new FixedNode(80, 40));
        
        var size = container.Measure(Size.Infinity);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(140));  // 50 + 80 + 10
            Assert.That(size.Height, Is.EqualTo(40));  // max(30, 40)
        }
    }
    
    [Test]
    public void Arrange_PlacesChildrenHorizontally()
    {
        var container = new MyContainerNode { Spacing = 10 };
        container.Children.Add(new FixedNode(50, 30));
        container.Children.Add(new FixedNode(80, 40));
        
        _ = container.Measure(new Size(200, 100));
        container.Arrange(new Rect(0, 0, 200, 100));
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.Children[0].Bounds.X, Is.EqualTo(0));
            Assert.That(container.Children[0].Bounds.Width, Is.EqualTo(50));
            Assert.That(container.Children[1].Bounds.X, Is.EqualTo(60));  // 50 + 10
            Assert.That(container.Children[1].Bounds.Width, Is.EqualTo(80));
        }
    }
    
    [Test]
    public void WithPadding_IncludesPaddingInMeasure()
    {
        var container = new MyContainerNode
        {
            Spacing = 10,
            Padding = new Thickness(20, 30)
        };
        container.Children.Add(new FixedNode(50, 30));
        
        var size = container.Measure(Size.Infinity);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(90));   // 50 + 40
            Assert.That(size.Height, Is.EqualTo(90));  // 30 + 60
        }
    }
}
```

## Примеры контейнеров

### Пример 1: WrapPanelNode (контейнер с переносом строк)

Контейнер, располагающий детей в строку с автоматическим переносом на следующую строку, если не хватает места.

```csharp
public class WrapPanelNode : LayoutNode
{
    public int HorizontalSpacing { get; set; }
    public int VerticalSpacing { get; set; }
    
    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }
    
    protected override Size MeasureOverride(Size available)
    {
        var inner = available.Deflate(Padding);
        
        if (Children.Count == 0)
            return new Size(Padding.Horizontal, Padding.Vertical);
        
        int maxWidth = 0;
        int currentRowWidth = 0;
        int currentRowHeight = 0;
        int totalHeight = 0;
        
        foreach (var child in Children)
        {
            var childSize = child.Measure(inner);
            
            // Проверяем, помещается ли ребёнок в текущую строку
            int requiredWidth = currentRowWidth > 0
                ? currentRowWidth + HorizontalSpacing + childSize.Width
                : childSize.Width;
            
            if (requiredWidth > inner.Width && currentRowWidth > 0)
            {
                // Перенос на новую строку
                maxWidth = Math.Max(maxWidth, currentRowWidth);
                totalHeight += currentRowHeight + VerticalSpacing;
                currentRowWidth = childSize.Width;
                currentRowHeight = childSize.Height;
            }
            else
            {
                // Добавляем в текущую строку
                currentRowWidth = requiredWidth;
                currentRowHeight = Math.Max(currentRowHeight, childSize.Height);
            }
        }
        
        // Учитываем последнюю строку
        maxWidth = Math.Max(maxWidth, currentRowWidth);
        totalHeight += currentRowHeight;
        
        return new Size(
            maxWidth + Padding.Horizontal,
            totalHeight + Padding.Vertical
        );
    }
    
    protected override void ArrangeOverride(Rect finalRect)
    {
        var slot = finalRect.Deflate(Padding);
        
        int x = slot.Point.X;
        int y = slot.Point.Y;
        int rowHeight = 0;
        
        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            
            // Проверяем, помещается ли ребёнок в текущую строку
            int requiredX = x > slot.Point.X
                ? x + HorizontalSpacing + childSize.Width
                : x + childSize.Width;
            
            if (requiredX > slot.Right && x > slot.Point.X)
            {
                // Перенос на новую строку
                x = slot.Point.X;
                y += rowHeight + VerticalSpacing;
                rowHeight = 0;
            }
            
            // Размещаем ребёнка
            var rect = new Rect(x, y, childSize.Width, childSize.Height);
            child.Arrange(rect);
            
            x += childSize.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, childSize.Height);
        }
    }
}
```

**Использование:**

```csharp
var wrapPanel = UI.WrapPanel(horizontalSpacing: 10, verticalSpacing: 10)
    .Add(UI.Label("Item 1", font, brush))
    .Add(UI.Label("Item 2", font, brush))
    .Add(UI.Label("Item 3", font, brush))
    .Add(UI.Label("Item 4", font, brush));
```

### Пример 2: BorderNode (контейнер с рамкой)

Контейнер, добавляющий рамку вокруг содержимого.

```csharp
public class BorderNode : LayoutNode
{
    public Thickness BorderThickness { get; set; }
    public Color BorderColor { get; set; }
    public Color BackgroundColor { get; set; }
    
    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }
    
    protected override Size MeasureOverride(Size available)
    {
        var inner = available.Deflate(Padding).Deflate(BorderThickness);
        
        int maxWidth = 0;
        int maxHeight = 0;
        
        foreach (var child in Children)
        {
            var childSize = child.Measure(inner);
            maxWidth = Math.Max(maxWidth, childSize.Width);
            maxHeight = Math.Max(maxHeight, childSize.Height);
        }
        
        return new Size(
            maxWidth + Padding.Horizontal + BorderThickness.Horizontal,
            maxHeight + Padding.Vertical + BorderThickness.Vertical
        );
    }
    
    protected override void ArrangeOverride(Rect finalRect)
    {
        var slot = finalRect.Deflate(Padding).Deflate(BorderThickness);
        
        foreach (var child in Children)
        {
            child.Arrange(slot);
        }
        
        // Здесь можно создать компоненты для отрисовки рамки и фона
        // (требует интеграции с адаптером)
    }
}
```

### Пример 3: ConditionalNode (условный контейнер)

Контейнер, отображающий одного из детей в зависимости от условия.

```csharp
public class ConditionalNode : LayoutNode
{
    private readonly Func<bool> _condition;
    private readonly LayoutNode _trueNode;
    private readonly LayoutNode _falseNode;
    
    public ConditionalNode(Func<bool> condition, LayoutNode trueNode, LayoutNode falseNode = null)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        _trueNode = trueNode ?? throw new ArgumentNullException(nameof(trueNode));
        _falseNode = falseNode;
        
        // Добавляем детей в базовый список для корректного обхода дерева
        Children.Add(_trueNode);
        if (_falseNode != null)
            Children.Add(_falseNode);
    }
    
    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }
    
    protected override Size MeasureOverride(Size available)
    {
        var activeNode = _condition() ? _trueNode : _falseNode;
        
        if (activeNode == null)
            return Size.Empty;
        
        var inner = available.Deflate(Padding);
        var size = activeNode.Measure(inner);
        
        return new Size(
            size.Width + Padding.Horizontal,
            size.Height + Padding.Vertical
        );
    }
    
    protected override void ArrangeOverride(Rect finalRect)
    {
        var activeNode = _condition() ? _trueNode : _falseNode;
        
        if (activeNode == null)
            return;
        
        var slot = finalRect.Deflate(Padding);
        activeNode.Arrange(slot);
        
        // Скрываем неактивный узел
        var inactiveNode = _condition() ? _falseNode : _trueNode;
        if (inactiveNode != null)
        {
            // Можно установить Visible = false для компонентов неактивного узла
        }
    }
}
```

**Использование:**

```csharp
var isVisible = new Observable<bool>(true);

var conditional = new ConditionalNode(
    () => isVisible.Value,
    UI.Label("Visible", font, brush),
    UI.Label("Hidden", font, grayBrush)
);
```

## Алгоритм Measure/Arrange

### Measure (первый проход)

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

### Arrange (второй проход)

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

### sealed Arrange для контейнеров

Контейнеры всегда занимают весь предоставленный слот (с учётом `Margin`). Переопределение `Arrange` помечается как `sealed`:

```csharp
public sealed override void Arrange(Rect finalRect)
{
    Rect inner = finalRect.Deflate(Margin);
    Bounds = inner;
    ArrangeOverride(inner);
}
```

**Почему это важно:**
- Контейнеры не должны выравниваться внутри слота — они занимают весь слот.
- Выравнивание работает для виджетов внутри контейнера.
- `sealed` предотвращает случайное переопределение в наследниках.

## Работа с Children

### Добавление детей

```csharp
// Прямое добавление
container.Children.Add(child);

// Fluent-метод
public static T Add<T>(this T node, LayoutNode child) where T : LayoutNode
{
    node.Children.Add(child);
    return node;
}
```

### Обход детей

```csharp
// Прямой обход
foreach (var child in Children)
{
    child.Measure(available);
}

// Индексированный доступ
for (int i = 0; i < Children.Count; i++)
{
    var child = Children[i];
    // ...
}
```

### Синхронизация с базовым списком

Если контейнер хранит детей в отдельной коллекции (например, `GridNode` хранит `GridChild` с координатами), необходимо синхронизировать с базовым `Children`:

```csharp
public GridNode Add(LayoutNode child, int row, int column)
{
    _gridChildren.Add(new GridChild(child, row, column));
    base.Children.Add(child);  // Синхронизация для DisposeTree() и других методов
    return this;
}
```

## Padding и Margin

### Padding (внутренние отступы)

Padding уменьшает доступное пространство для детей:

```csharp
protected override Size MeasureOverride(Size available)
{
    var inner = available.Deflate(Padding);
    // Измеряем детей с inner
    // ...
    return new Size(
        contentWidth + Padding.Horizontal,
        contentHeight + Padding.Vertical
    );
}

protected override void ArrangeOverride(Rect finalRect)
{
    var slot = finalRect.Deflate(Padding);
    // Размещаем детей в slot
}
```

### Margin (внешние отступы)

Margin обрабатывается в базовом классе `LayoutNode`:

```csharp
public Size Measure(Size available)
{
    var inner = new Size(
        Math.Max(0, available.Width - Margin.Horizontal),
        Math.Max(0, available.Height - Margin.Vertical)
    );
    DesiredSize = MeasureOverride(inner);
    return new Size(
        DesiredSize.Width + Margin.Horizontal,
        DesiredSize.Height + Margin.Vertical
    );
}
```

## Выравнивание детей

Дети могут иметь собственные `HAlignment` и `VAlignment`, которые применяются в базовом методе `Arrange`:

```csharp
public virtual void Arrange(Rect finalRect)
{
    Rect inner = finalRect.Deflate(Margin);
    Bounds = AlignRect(DesiredSize, inner, HAlignment, VAlignment);
    ArrangeOverride(Bounds);
}
```

Контейнер может передавать детям слот с учётом их выравнивания:

```csharp
protected override void ArrangeOverride(Rect finalRect)
{
    var slot = finalRect.Deflate(Padding);
    
    foreach (var child in Children)
    {
        // Передаём весь слот — ребёнок сам выровняется внутри
        child.Arrange(slot);
    }
}
```

## Тестирование контейнеров

### Обязательные сценарии

1. **Measure с `Size.Infinity`** — возврат корректного `DesiredSize`.
2. **Measure с ограниченным пространством** — корректное поведение.
3. **Arrange** — проверка `Bounds` детей.
4. **Padding/Margin** — включаются в расчёты.
5. **Пустой контейнер** (без детей) — не падает.
6. **Граничные случаи** (отрицательные размеры, нулевые дети).

### Пример теста

```csharp
[TestFixture]
public class MyContainerNodeTests
{
    [Test]
    public void Measure_WithInfinity_ReturnsContentSize()
    {
        var container = new MyContainerNode();
        container.Children.Add(new FixedNode(100, 50));
        container.Children.Add(new FixedNode(80, 60));
        
        var size = container.Measure(Size.Infinity);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.GreaterThanOrEqualTo(100));
            Assert.That(size.Height, Is.GreaterThanOrEqualTo(60));
        }
    }
    
    [Test]
    public void Measure_WithPadding_IncludesPadding()
    {
        var container = new MyContainerNode
        {
            Padding = new Thickness(10, 20)
        };
        container.Children.Add(new FixedNode(50, 30));
        
        var size = container.Measure(Size.Infinity);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(70));   // 50 + 20
            Assert.That(size.Height, Is.EqualTo(70));  // 30 + 40
        }
    }
    
    [Test]
    public void Measure_WithMargin_AddsToTotalSize()
    {
        var container = new MyContainerNode
        {
            Margin = new Thickness(10, 20)
        };
        container.Children.Add(new FixedNode(50, 30));
        
        var size = container.Measure(Size.Infinity);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(70));   // 50 + 20
            Assert.That(size.Height, Is.EqualTo(70));  // 30 + 40
        }
    }
    
    [Test]
    public void Arrange_PlacesChildrenCorrectly()
    {
        var container = new MyContainerNode();
        container.Children.Add(new FixedNode(50, 30));
        container.Children.Add(new FixedNode(80, 40));
        
        _ = container.Measure(new Size(200, 100));
        container.Arrange(new Rect(10, 20, 200, 100));
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.Children[0].Bounds.X, Is.EqualTo(10));
            Assert.That(container.Children[0].Bounds.Y, Is.EqualTo(20));
            Assert.That(container.Children[1].Bounds.X, Is.GreaterThan(10));
        }
    }
    
    [Test]
    public void EmptyContainer_DoesNotThrow()
    {
        var container = new MyContainerNode();
        
        Assert.DoesNotThrow(() =>
        {
            var size = container.Measure(Size.Infinity);
            container.Arrange(new Rect(0, 0, 100, 100));
        });
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(size.Width, Is.EqualTo(0));
            Assert.That(size.Height, Is.EqualTo(0));
        }
    }
}
```

## Производительность

### Избегание аллокаций в Measure

```csharp
// Правильно: переиспользовать приватные поля
private double[] _buffer = new double[8];

protected override Size MeasureOverride(Size available)
{
    if (_buffer.Length < Children.Count)
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

### Кэширование вычислений

Если контейнер выполняет сложные вычисления, кэшируйте результаты:

```csharp
private Size? _cachedDesiredSize;
private Size _lastAvailable;

protected override Size MeasureOverride(Size available)
{
    if (_cachedDesiredSize.HasValue && _lastAvailable == available)
        return _cachedDesiredSize.Value;
    
    // Сложные вычисления
    var result = ComputeDesiredSize(available);
    
    _cachedDesiredSize = result;
    _lastAvailable = available;
    return result;
}
```

### Оптимизация обхода детей

```csharp
// Правильно: индексированный доступ
for (int i = 0; i < Children.Count; i++)
{
    var child = Children[i];
    // ...
}

// Неправильно: LINQ (аллокация enumerator)
foreach (var child in Children.Where(c => c.Visible))
{
    // ...
}
```

## Fluent-расширения

Fluent-расширения упрощают настройку контейнеров:

```csharp
public static class MyContainerNodeFluent
{
    public static MyContainerNode Spacing(this MyContainerNode node, int spacing)
    {
        node.Spacing = spacing;
        return node;
    }
    
    public static MyContainerNode HorizontalSpacing(this MyContainerNode node, int spacing)
    {
        node.HorizontalSpacing = spacing;
        return node;
    }
    
    public static MyContainerNode VerticalSpacing(this MyContainerNode node, int spacing)
    {
        node.VerticalSpacing = spacing;
        return node;
    }
}
```

**Использование:**

```csharp
var container = UI.MyContainer()
    .Spacing(10)
    .Padding(20)
    .Add(child1)
    .Add(child2);
```

## Чек-лист создания контейнера

- [ ] Создан класс контейнера, наследник `LayoutNode`.
- [ ] Переопределён `MeasureOverride` — вычисляет `DesiredSize` на основе детей.
- [ ] Переопределён `ArrangeOverride` — размещает детей через `child.Arrange(rect)`.
- [ ] `Arrange` помечен как `sealed` (контейнер занимает весь слот).
- [ ] Учтены `Padding` и `Margin` в расчётах.
- [ ] Добавлен метод-фабрика в `UI` (статический класс).
- [ ] Добавлены fluent-расширения (опционально).
- [ ] Написаны тесты (Measure, Arrange, Padding, Margin, пустой контейнер).
- [ ] Публичный API покрыт XML-документацией.
- [ ] Оптимизированы аллокации (переиспользование буферов, кэширование).

## Управление ресурсами

Контейнеры не владеют ресурсами (компонентами рендерера). Ресурсы создаются и освобождаются виджетами.

### Dispose дерева

```csharp
// Автоматический Dispose через LayoutNodeFluent.Dispose
rootNode.Dispose();  // освободит все компоненты в дереве

// Ручной Dispose
container.DisposeTree();  // рекурсивно освободит всех детей
```

**Реализация DisposeTree:**

```csharp
public static void DisposeTree(this LayoutNode node)
{
    foreach (var child in node.Children)
        child.DisposeTree();
    if (node is IDisposable d)
        d.Dispose();
}
```

## Заключение

Контейнеры — узлы layout-системы, имеющие дочерние узлы и отвечающие за их размещение. Они вычисляют `DesiredSize` через `MeasureOverride` и размещают детей через `ArrangeOverride`. Контейнеры всегда занимают весь предоставленный слот (с учётом `Margin`), поэтому `Arrange` помечается как `sealed`. Fluent-расширения упрощают настройку и позволяют строить цепочки вызовов.