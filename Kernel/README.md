# DisplayNodes.Kernel

Ядро декларативной системы компоновки UI. Реализует двухпроходный алгоритм layout (Measure → Arrange), реактивные свойства и бэкенд-агностичную архитектуру.

## Содержание

1. [Структура](#structure)
2. [Основные концепции](#concepts)
3. [Типы](#types)
4. [Контейнеры](#containers)
5. [Виджеты](#widgets)
6. [Ограничения размеров](#constraints)
7. [Flex-механика](#flex)
8. [Observable](#observable)
9. [ComputedObservable](#computed)
10. [ObservableList](#list)
11. [Fluent API](#fluent)
12. [DisplayRoot](#display-root)
13. [Применение дерева](#apply)
14. [Выравнивание](#alignment)
15. [Обход дерева](#tree)
16. [Пример: полный UI](#full-example)
17. [Расширение](#extension)

---

<a id="structure"></a>
## Структура

```
Kernel/
├── Core/
│   ├── Types.cs              # Point, Size, Rect, Thickness, Color, Percent, GradientStop, Shadow, Transform
│   ├── LayoutNode.cs         # Базовый абстрактный класс
│   ├── Containers/           # StackLayoutNode, GridNode, OverlayNode, UniformGridNode, WrapPanelNode, ConditionalNode, TransformNode, FixedNode, StretchNode
│   ├── Observable.cs         # Реактивное свойство
│   ├── ComputedObservable.cs # Вычисляемое свойство
│   ├── ObservableList.cs     # Реактивная коллекция
│   └── Rendering/            # Интерфейсы бэкенда
├── Widgets/                  # LabelNode, ImageNode, BackgroundNode, ClipNode
├── Fluent/                   # UI-фабрика, DisplayRoot, fluent-расширения
└── Helpers/                  # Extensions для обхода дерева
```

---

<a id="concepts"></a>
## Основные концепции

### Двухпроходный алгоритм

1. **Measure** — вычисление желаемого размера (`DesiredSize`) на основе доступного пространства.
2. **Arrange** — размещение в выделенном слоте (`Bounds`) с учётом выравнивания.

```csharp
// Родитель вызывает Measure для всех детей
var desiredSize = child.Measure(availableSize);

// Затем Arrange с финальным слотом
child.Arrange(finalRect);
```

### Бэкенд-агностичность

Ядро не знает про GDI+, WinForms или другие технологии рендеринга. Все ресурсы (шрифты, кисти, изображения) представлены интерфейсами:

- `IFont`, `IBrush`, `IImage`, `ITextFormat`, `IGraphicsPath`
- `IRenderComponent` — базовый компонент рендерера
- `ILabelComponent`, `IImageComponent`, `IMaskComponent` — специализированные компоненты
- `IWidgetFactory`, `IFontFactory`, `IBrushFactory`, `IImageFactory` — фабрики

Конкретная реализация предоставляется адаптером.

---

<a id="types"></a>
## Типы

### Point, Size, Rect, Thickness, Color

```csharp
var p = new Point(10, 20);
var s = new Size(100, 200);
var r = new Rect(10, 20, 100, 200);
var t = new Thickness(10, 20, 30, 40);
var c = Color.FromArgb(128, 255, 0, 0);
```

### Percent

```csharp
var p1 = new Percent(50);       // 50%
var p2 = Percent.Zero;          // 0%
var p3 = Percent.Hundred;       // 100%
Percent p4 = 75;                // неявное преобразование из int
```

### GradientStop

```csharp
var stop1 = new GradientStop(Color.Red, 0);       // 0% — красный
var stop2 = new GradientStop(Color.Blue, 100);    // 100% — синий
```

### Shadow

```csharp
var shadow = new Shadow(2, 4, 8, Color.FromArgb(80, 0, 0, 0));
var shadowDefault = new Shadow(2, 4, 8);  // стандартный полупрозрачный чёрный
```

### Transform

```csharp
var t = new Transform(scaleX: 1.5f, scaleY: 1.5f, rotation: 45f, origin: new Point(50, 50));
var identity = Transform.Identity;
```

---

<a id="containers"></a>
## Контейнеры

### StackLayoutNode

Располагает детей в строку или столбец.

```csharp
var stack = new StackLayoutNode
{
    IsVertical = true,
    Spacing = 8,
    MainAxisAlignment = MainAxisAlignment.Center
};
stack.Children.Add(child1);
stack.Children.Add(child2);
```

**MainAxisAlignment:** `Start`, `Center`, `End`, `SpaceBetween`.

### GridNode

Сетка с произвольными размерами строк/колонок.

```csharp
var grid = new GridNode();
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(2)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

grid.Add(child1, 0, 0);
grid.Add(child2, 1, 1);
```

**GridLength:** `Pixels(value)`, `Auto`, `Star(weight)`.

### OverlayNode

Размещает всех детей в одном слоте (друг поверх друга).

```csharp
var overlay = new OverlayNode();
overlay.Children.Add(background);
overlay.Children.Add(content);
```

### UniformGridNode

Равномерная сетка с фиксированным числом строк/колонок.

```csharp
var grid = new UniformGridNode(rows: 3, columns: 3, spacing: 10);
grid.Children.Add(child1);
grid.Children.Add(child2);
```

### WrapPanelNode

Перенос детей на следующую строку/столбец.

```csharp
var panel = new WrapPanelNode
{
    Direction = WrapDirection.Horizontal,
    Spacing = 8,
    LineSpacing = 4
};
```

### ConditionalNode

Отображает одно из двух поддеревьев по `Observable<bool>`.

```csharp
var conditional = new ConditionalNode(
    condition: new Observable<bool>(true),
    trueNode: labelA,
    falseNode: labelB);
```

**Ограничение:** layout не пересчитывается автоматически при переключении.

### TransformNode

Применяет аффинное преобразование к единственному ребёнку.

```csharp
var transform = new TransformNode(new Transform(1f, 1f, 45f, new Point(50, 50)))
{
    Child = new FixedNode(100, 50)
};
```

**Ограничение:** `ApplyRecursive` бросает `NotSupportedException` — адаптеры не поддерживают трансформации.

### FixedNode

Узел с фиксированным размером. Полезен для распорок.

```csharp
var spacer = new FixedNode(0, 20);
```

**Примечание:** `FixedNode` с `HAlignment = Start, VAlignment = Start` по умолчанию — не растягивается.

### StretchNode

Публичный узел, растягивающийся вдоль обеих осей.

```csharp
var stretch = new StretchNode(0, 0);
```

Используется как flex-ребёнок в `StackLayoutNode`.

---

<a id="widgets"></a>
## Виджеты

### LabelNode

```csharp
var label = new LabelNode("Hello", font, brush, labelComponent, textMeasurer);
label.SetSize(100, 50);  // опционально

// Привязки:
label.BindText(observableString);
label.BindFont(observableFont);
label.BindForegroundBrush(observableBrush);
label.BindBackgroundBrush(observableBrush);
```

### ImageNode

```csharp
var image = new ImageNode(bitmap, imageComponent);
image.SetSize(200, 150);
image.Component.SizeMode = ImageSizeMode.Stretch;
```

### BackgroundNode

```csharp
var bg = new BackgroundNode(brush, labelComponent);
// или
var bg = new BackgroundNode(color, labelComponent, brushFactory);
```

Растягивается на весь слот, `DesiredSize = 0`.

### ClipNode

```csharp
var clip = new ClipNode(maskComponent);
clip.Children.Add(content);
```

---

<a id="constraints"></a>
## Ограничения размеров

`LayoutNode` поддерживает `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight`. Применяются после `MeasureOverride`.

```csharp
var label = new LabelNode(...);
label.MinWidth = 100;
label.MaxWidth = 400;
label.MinHeight = 30;
```

Через fluent API:

```csharp
UI.Label("Text", font, brush)
    .MinWidth(100)
    .MaxWidth(400)
    .WidthRange(100, 400);
```

---

<a id="flex"></a>
## Flex-механика

`StackLayoutNode` поддерживает `FlexWeight` для пропорционального распределения свободного пространства.

```csharp
UI.Column(8)
    .Add(UI.Label("Fixed", font, brush))
    .Add(UI.Label("Flexible", font, brush).Flex(1))
    .Add(UI.Label("Double", font, brush).Flex(2));
```

**Поведение:**
- `FlexWeight == 0` — natural размер.
- `FlexWeight > 0` — доля свободного места пропорционально весу.
- При `free < 0` — flex-дети сжимаются, fixed — нет.

---

<a id="observable"></a>
## Observable

Реактивное свойство с подпиской на изменения.

```csharp
var counter = new Observable<int>(0);

// Подписка
var subscription = counter.Subscribe(value =>
{
    Console.WriteLine($"Counter: {value}");
});

// Изменение — уведомит подписчиков
counter.Value = 10;

// Отписка
subscription.Dispose();
```

**Особенности:**
- Автоматическое сравнение значений (не уведомляет, если значение не изменилось).
- Потокобезопасность через `lock`.
- Исключения в подписчиках не ломают цепочку.

---

<a id="computed"></a>
## ComputedObservable

Вычисляемое свойство на основе других источников.

```csharp
var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName, lastName);

// fullName.Value == "Ivan Petrov"
firstName.Value = "Petr";
// fullName.Value == "Petr Petrov"
```

**Особенности:**
- Начальное значение вычисляется в конструкторе.
- При изменении любой зависимости вызывается `compute()`.
- `Refresh()` — ручной пересчёт.
- `Dispose` отписывается от зависимостей.

---

<a id="list"></a>
## ObservableList

Реактивная коллекция.

```csharp
var list = new ObservableList<string>();

list.Changed += change =>
{
    Console.WriteLine($"{change.Type} at {change.NewIndex}: {change.Item}");
};

list.Add("Item 1");          // Add at 0: Item 1
list.RemoveAt(0);            // Remove at 0: Item 1
list[0] = "Replaced";        // Replace at 0: Replaced
list.Move(0, 1);             // Move [0→1]
list.Clear();                // Reset
```

**Особенности:**
- Потокобезопасна через `lock`.
- Событие `Changed` — вне lock.
- Все подписчики вызываются отдельно через `GetInvocationList()`.
- `Dispose` делает коллекцию непригодной — любая операция бросает `ObjectDisposedException`.

---

<a id="fluent"></a>
## Fluent API

### UI-фабрика

```csharp
// Инициализация
UI.Factory = new WidgetFactory();
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();
UI.ImageFactory = new GdiImageFactory();

// Создание узлов
var font = UI.Font("Segoe UI", 14f);
var brush = UI.SolidBrush(Color.White);

var root = UI.Column(8)
    .Padding(20)
    .Add(UI.Label("Hello", font, brush))
    .Add(UI.Row(12)
        .Add(UI.Label("Status:", font, grayBrush))
        .Add(UI.Label("OK", font, greenBrush)));
```

### Расширения

```csharp
var node = UI.Label("Text", font, brush)
    .Margin(10)
    .Padding(5)
    .HAlignment(Alignment.Center)
    .VAlignment(Alignment.Stretch)
    .MinWidth(100)
    .MaxWidth(400)
    .Flex(1)
    .Shadow(2, 4, 8);
```

**Основные расширения:**
- `Margin` / `Padding`
- `HAlignment` / `VAlignment`
- `Add(child)`
- `MinWidth` / `MaxWidth` / `MinHeight` / `MaxHeight` / `WidthRange` / `HeightRange`
- `Flex(weight)`
- `Shadow(...)` — три перегрузки
- `Opacity` / `Brightness` / `Contrast`
- `BindText` / `BindFont` / `BindForegroundBrush` / `BindBackgroundBrush` / `BindVisible` / `BindOpacity` ...

**Градиенты:**
```csharp
var gradient = UI.LinearGradient(
    new Point(0, 0), new Point(100, 0),
    new GradientStop(Color.Red, 0),
    new GradientStop(Color.Blue, 100));

var radial = UI.RadialGradient(
    new Point(50, 50), 50,
    new GradientStop(Color.White, 0),
    new GradientStop(Color.Black, 100));
```

---

<a id="display-root"></a>
## DisplayRoot

Управляет жизненным циклом корневого контейнера.

```csharp
var factory = new RenderRootFactory(parentComponent);
var displayRoot = new DisplayRoot(factory);

// Построение дерева
displayRoot.Build(rootNode, new Point(0, 0), new Size(800, 600));

// Очистка
displayRoot.Dispose();
```

---

<a id="apply"></a>
## Применение дерева

```csharp
// Measure + Arrange + привязка компонентов
rootNode.Apply(parentComponent, location, size);

// Перестроение (удаляет старые компоненты)
rootNode.Rebuild(parentComponent, location, size);

// Полное удаление
rootNode.Dispose();
```

---

<a id="alignment"></a>
## Выравнивание

```csharp
public enum Alignment
{
    Start,    // Прижать к началу
    Center,   // По центру
    End,      // Прижать к концу
    Stretch   // Растянуть
}
```

Применяется к каждому узлу через `HAlignment` и `VAlignment`.

---

<a id="tree"></a>
## Обход дерева

```csharp
// Собрать все компоненты
var components = new List<IRenderComponent>();
rootNode.CollectComponents(components);

// Рекурсивно освободить ресурсы
rootNode.DisposeTree();
```

---

<a id="full-example"></a>
## Пример: полный UI

```csharp
// Инициализация
UI.Factory = new WidgetFactory();
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();

// Создание
var font = UI.Font("Segoe UI", 14f);
var white = UI.SolidBrush(Color.White);
var green = UI.SolidBrush(Color.LimeGreen);

var root = UI.Column(8)
    .Padding(20)
    .Add(UI.Label("Hello, DisplayNodes!", font, white))
    .Add(UI.Fixed(0, 4))
    .Add(UI.Row(12)
        .Add(UI.Label("Status:", font, UI.SolidBrush(Color.Gray)))
        .Add(UI.Label("OK", font, green)));

// Применение
var displayRoot = new DisplayRoot(new RenderRootFactory(parent));
displayRoot.Build(root, Point.Empty, new Size(800, 600));
```

---

<a id="extension"></a>
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

---

## Документация

- [EXAMPLES.md](../docs/EXAMPLES.md) — примеры
- [ARCHITECTURE.md](../docs/ARCHITECTURE.md) — архитектура
- [CUSTOM_CONTAINERS.md](../docs/CUSTOM_CONTAINERS.md) — свои контейнеры
- [CUSTOM_WIDGETS.md](../docs/CUSTOM_WIDGETS.md) — свои виджеты
- [OBSERVABLE_DEEP_DIVE.md](../docs/OBSERVABLE_DEEP_DIVE.md) — реактивность