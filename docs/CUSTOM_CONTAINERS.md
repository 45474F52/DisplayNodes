# Создание контейнеров

Руководство по созданию собственных контейнеров в DisplayNodes.  
Контейнер — это узел layout-системы, имеющий дочерние узлы и отвечающий за их размещение. В отличие от виджета, контейнер не оборачивает компонент рендерера, а вычисляет позиции и размеры своих детей.

## Содержание

1. [Отличие контейнера от виджета](#difference)
2. [Базовый класс LayoutNode](#layoutnode)
3. [Создание простого контейнера](#simple)
4. [Алгоритм Measure/Arrange](#algorithm)
5. [Работа с Children](#children)
6. [Padding и Margin](#padding-margin)
7. [Выравнивание детей](#alignment)
8. [Пример: WrapPanelNode](#wrappanel)
9. [Пример: ConditionalNode](#conditional)
10. [Пример: TransformNode](#transform)
11. [StretchNode](#stretch)
12. [Тестирование контейнеров](#testing)
13. [Производительность](#performance)
14. [Fluent-расширения](#fluent)
15. [Чек-лист создания контейнера](#checklist)
16. [Управление ресурсами](#resources)

---

<a id="difference"></a>
## Отличие контейнера от виджета

| Характеристика | Контейнер (`LayoutNode`) | Виджет (`WidgetNode`) |
|---|---|---|
| Имеет дочерние узлы | Да | Нет |
| Отвечает за layout детей | Да | Нет |
| Оборачивает `IRenderComponent` | Нет | Да |
| Примеры | `StackLayoutNode`, `GridNode`, `OverlayNode` | `LabelNode`, `ImageNode`, `BackgroundNode` |

Контейнер вычисляет размеры и позиции своих детей. Виджет вычисляет собственный размер и записывает его в свойства компонента рендерера.

---

<a id="layoutnode"></a>
## Базовый класс LayoutNode

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

**Ключевые особенности:**
- `Measure` вычисляет `DesiredSize` на основе доступного пространства.
- `Arrange` размещает узел в выделенном слоте.
- `MeasureOverride` и `ArrangeOverride` переопределяются в наследниках для реализации конкретной логики layout.

---

<a id="simple"></a>
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
            Assert.That(container.Children[1].Bounds.X, Is.EqualTo(60));
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

---

<a id="algorithm"></a>
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
    ├── 3. Применить MinWidth/MaxWidth/MinHeight/MaxHeight
    │
    └── 4. Прибавить Margin к результату
           DesiredSize = clamped + Margin
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

---

<a id="children"></a>
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

---

<a id="padding-margin"></a>
## Padding и Margin

### Padding (внутренние отступы)

Padding уменьшает доступное пространство для детей:

```csharp
protected override Size MeasureOverride(Size available)
{
    var inner = available.Deflate(Padding);
    // Измеряем детей с inner
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

---

<a id="alignment"></a>
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

---

<a id="wrappanel"></a>
## Пример: WrapPanelNode

Контейнер, располагающий детей в строку с автоматическим переносом на следующую строку.

```csharp
public class WrapPanelNode : LayoutNode
{
    public WrapDirection Direction { get; set; } = WrapDirection.Horizontal;
    public int Spacing { get; set; }      // между детьми в строке
    public int LineSpacing { get; set; }  // между строками

    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }

    protected override Size MeasureOverride(Size available)
    {
        if (Children.Count == 0)
            return new Size(Padding.Horizontal, Padding.Vertical);

        Size inner = available.Deflate(Padding);

        return Direction == WrapDirection.Horizontal
            ? MeasureHorizontal(inner)
            : MeasureVertical(inner);
    }

    protected override void ArrangeOverride(Rect finalRect)
    {
        Rect slot = finalRect.Deflate(Padding);

        if (Direction == WrapDirection.Horizontal)
            ArrangeHorizontal(slot);
        else
            ArrangeVertical(slot);
    }

    private Size MeasureHorizontal(Size available)
    {
        int maxRowWidth = 0;
        int currentRowWidth = 0;
        int currentRowHeight = 0;
        int totalHeight = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            LayoutNode child = Children[i];
            Size childSize = child.Measure(available);

            int addWidth = childSize.Width + (currentRowWidth > 0 ? Spacing : 0);

            bool fits = available.Width == int.MaxValue
                || currentRowWidth + addWidth <= available.Width
                || currentRowWidth == 0;

            if (fits)
            {
                currentRowWidth += addWidth;
                currentRowHeight = Math.Max(currentRowHeight, childSize.Height);
            }
            else
            {
                maxRowWidth = Math.Max(maxRowWidth, currentRowWidth);
                totalHeight += currentRowHeight + LineSpacing;
                currentRowWidth = childSize.Width;
                currentRowHeight = childSize.Height;
            }
        }

        maxRowWidth = Math.Max(maxRowWidth, currentRowWidth);
        totalHeight += currentRowHeight;

        return new Size(
            maxRowWidth + Padding.Horizontal,
            totalHeight + Padding.Vertical);
    }

    private void ArrangeHorizontal(Rect slot)
    {
        int availableWidth = slot.Size.Width;
        int x = slot.Point.X;
        int y = slot.Point.Y;
        int currentRowHeight = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            LayoutNode child = Children[i];
            Size childSize = child.DesiredSize;

            int addWidth = childSize.Width + (x > slot.Point.X ? Spacing : 0);

            bool fits = availableWidth == int.MaxValue
                || x + addWidth - slot.Point.X <= availableWidth
                || x == slot.Point.X;

            if (!fits)
            {
                y += currentRowHeight + LineSpacing;
                x = slot.Point.X;
                currentRowHeight = 0;
            }
            else
            {
                x += (x > slot.Point.X ? Spacing : 0);
            }

            Rect rect = new Rect(x, y, childSize.Width, childSize.Height);
            child.Arrange(rect);

            x += childSize.Width;
            currentRowHeight = Math.Max(currentRowHeight, childSize.Height);
        }
    }

    // MeasureVertical / ArrangeVertical — аналогично, зеркально
}
```

**Особенности:**
- `Direction` определяет ось переноса.
- Если ребёнок шире `available` — остаётся в строке и переполняет (не переносится бесконечно).
- Stateless: при Arrange логика Measure повторяется заново.

---

<a id="conditional"></a>
## Пример: ConditionalNode

Контейнер, отображающий одно из двух поддеревьев по `Observable<bool>`.

```csharp
public class ConditionalNode : LayoutNode, IDisposable
{
    private readonly Observable<bool> _condition;
    private IDisposable _subscription;
    private bool _disposed;

    public Observable<bool> Condition => _condition;
    public LayoutNode TrueNode { get; }
    public LayoutNode FalseNode { get; }

    internal event Action<bool> ConditionChanged;

    public ConditionalNode(Observable<bool> condition, LayoutNode trueNode, LayoutNode falseNode)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        TrueNode = trueNode;
        FalseNode = falseNode;

        if (TrueNode != null) Children.Add(TrueNode);
        if (FalseNode != null) Children.Add(FalseNode);

        _subscription = _condition.Subscribe(OnConditionChanged);
    }

    public LayoutNode ActiveNode => _condition.Value ? TrueNode : FalseNode;

    protected override Size MeasureOverride(Size available)
    {
        LayoutNode active = ActiveNode;
        if (active == null)
            return new Size(Padding.Horizontal, Padding.Vertical);

        Size inner = available.Deflate(Padding);
        Size childSize = active.Measure(inner);

        return new Size(
            childSize.Width + Padding.Horizontal,
            childSize.Height + Padding.Vertical);
    }

    public sealed override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }

    protected override void ArrangeOverride(Rect finalRect)
    {
        LayoutNode active = ActiveNode;
        if (active == null)
            return;

        Rect slot = finalRect.Deflate(Padding);
        active.Arrange(slot);
    }

    private void OnConditionChanged(bool value)
    {
        if (_disposed)
            return;
        ConditionChanged?.Invoke(value);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _subscription?.Dispose();
        _subscription = null;
    }
}
```

**Особенности:**
- Оба поддерева хранятся в `Children` **всегда**.
- Неактивное поддерево пропускается в `MeasureOverride` и `ArrangeOverride`.
- `ConditionChanged` (внутреннее событие) позволяет fluent-методу `.OnChanged(...)` подписаться.
- `Dispose` отписывается от `Observable<bool>`.

**Ограничение:** `ConditionalNode` **не пересчитывает layout автоматически** при переключении условия. Пользователь должен вызвать `Measure`+`Arrange`+`Refresh` вручную или перестроить дерево.

---

<a id="transform"></a>
## Пример: TransformNode

Контейнер, применяющий аффинное преобразование к единственному ребёнку.

```csharp
public class TransformNode : LayoutNode
{
    public Transform Transform { get; set; }

    public LayoutNode Child
    {
        get => Children.Count > 0 ? Children[0] : null;
        set
        {
            Children.Clear();
            if (value != null)
                Children.Add(value);
        }
    }

    public TransformNode(Transform transform)
    {
        Transform = transform;
    }

    public TransformNode() : this(Transform.Identity) { }

    protected override Size MeasureOverride(Size available)
    {
        if (Child == null)
            return new Size(Padding.Horizontal, Padding.Vertical);

        Size inner = available.Deflate(Padding);
        Size childSize = Child.Measure(inner);

        BoundingBox bbox = ComputeBoundingBox(childSize, Transform);

        return new Size(
            (int)Math.Ceiling(bbox.Width) + Padding.Horizontal,
            (int)Math.Ceiling(bbox.Height) + Padding.Vertical);
    }

    public override void Arrange(Rect finalRect)
    {
        Rect inner = finalRect.Deflate(Margin);
        Bounds = inner;
        ArrangeOverride(inner);
    }

    protected override void ArrangeOverride(Rect finalRect)
    {
        if (Child == null)
            return;

        Rect slot = finalRect.Deflate(Padding);

        Size childSize = Child.DesiredSize;
        BoundingBox bbox = ComputeBoundingBox(childSize, Transform);

        float ox = childSize.Width * Transform.Origin.X / 100f;
        float oy = childSize.Height * Transform.Origin.Y / 100f;

        int dx = (int)Math.Round(-ox - bbox.MinX);
        int dy = (int)Math.Round(-oy - bbox.MinY);

        Rect childRect = new Rect(
            slot.Point.X + dx,
            slot.Point.Y + dy,
            childSize.Width,
            childSize.Height);

        Child.Arrange(childRect);
    }

    private static BoundingBox ComputeBoundingBox(Size size, Transform t)
    {
        float w = size.Width;
        float h = size.Height;

        float ox = w * t.Origin.X / 100f;
        float oy = h * t.Origin.Y / 100f;

        float[] xs = { -ox, w - ox, w - ox, -ox };
        float[] ys = { -oy, -oy, h - oy, h - oy };

        float rad = t.Rotation * (float)Math.PI / 180f;
        float cos = (float)Math.Cos(rad);
        float sin = (float)Math.Sin(rad);

        float tanSkewX = (float)Math.Tan(t.SkewX * Math.PI / 180f);
        float tanSkewY = (float)Math.Tan(t.SkewY * Math.PI / 180f);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        for (int i = 0; i < 4; i++)
        {
            float x = xs[i] * t.ScaleX;
            float y = ys[i] * t.ScaleY;

            float xSkew = x + y * tanSkewX;
            float ySkew = x * tanSkewY + y;

            float xRot = xSkew * cos - ySkew * sin;
            float yRot = xSkew * sin + ySkew * cos;

            if (xRot < minX) minX = xRot;
            if (xRot > maxX) maxX = xRot;
            if (yRot < minY) minY = yRot;
            if (yRot > maxY) maxY = yRot;
        }

        return new BoundingBox(
            RoundNearInteger(minX),
            RoundNearInteger(minY),
            RoundNearInteger(maxX),
            RoundNearInteger(maxY));
    }

    private static float RoundNearInteger(float value, float epsilon = 1e-4f)
    {
        float rounded = (float)Math.Round(value);
        if (Math.Abs(value - rounded) < epsilon)
            return rounded;
        return value;
    }
}
```

**Особенности:**
- `Measure` возвращает bounding box трансформированного ребёнка — layout учитывает трансформацию.
- Порядок: scale → skew → rotate, относительно `Origin`.
- `RoundNearInteger` нейтрализует шумы `float`-математики (например, `cos(π/2) ≈ -4.37e-8` вместо `0`).

**Ограничение:** `ApplyRecursive` бросает `NotSupportedException` при обнаружении `TransformNode` — адаптеры не поддерживают трансформации.

---

<a id="stretch"></a>
## StretchNode

`StretchNode` — публичный узел, растягивающийся вдоль обеих осей.

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
- Не контейнер в строгом смысле (нет детей), но публичный узел, часто используемый как flex-ребёнок.
- При Measure возвращает свой natural размер.
- При Arrange растягивается до слота через `HAlignment/VAlignment = Stretch`.

**Использование:**

```csharp
var column = UI.Column(0)
    .Add(UI.Label("Top", font, brush))
    .Add(new StretchNode(0, 0).Flex(1))
    .Add(UI.Label("Bottom", font, brush));
```

---

<a id="testing"></a>
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
            Assert.That(size.Width, Is.EqualTo(70));
            Assert.That(size.Height, Is.EqualTo(70));
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
    }
}
```

---

<a id="performance"></a>
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

---

<a id="fluent"></a>
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

---

<a id="checklist"></a>
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

---

<a id="resources"></a>
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