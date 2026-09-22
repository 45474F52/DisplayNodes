# DisplayNodes.Kernel

Ядро декларативной системы компоновки UI. Реализует двухпроходный алгоритм layout (Measure → Arrange), реактивные свойства и бэкенд-агностичную архитектуру.

## Структура

```
Kernel/
├── Core/
│   ├── Types.cs              # Point, Size, Rect, Thickness, Color
│   ├── LayoutNode.cs         # Базовый абстрактный класс
│   ├── Containers/           # StackLayoutNode, GridNode, OverlayNode, UniformGridNode, FixedNode
│   ├── Observable.cs         # Реактивное свойство
│   └── Rendering/            # Интерфейсы бэкенда
├── Widgets/                  # LabelNode, ImageNode, BackgroundNode, ClipNode
├── Fluent/                   # UI фабрика, DisplayRoot, fluent-расширения
└── Helpers/                  # Extensions для обхода дерева
```

## Основные концепции

### Двухпроходный алгоритм

1. **Measure** — вычисление желаемого размера (`DesiredSize`) на основе доступного пространства
2. **Arrange** — размещение в выделенном слоте (`Bounds`) с учётом выравнивания

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

Конкретная реализация предоставляется адаптером (например: `WinFormsAdapter`).

## Типы

### Point
```csharp
var p = new Point(10, 20);
var offset = p.Offset(5, 5);        // (15, 25)
var moved = p + new Size(10, 10);   // (20, 30)
```

### Size
```csharp
var s = new Size(100, 200);
var inflated = s.Inflate(new Thickness(10));   // (120, 220)
var deflated = s.Deflate(new Thickness(10));   // (80, 180)
```

### Rect
```csharp
var r = new Rect(10, 20, 100, 200);
bool contains = r.Contains(new Point(50, 50));  // true
var intersection = r.Intersect(otherRect);
```

### Thickness
```csharp
var t1 = new Thickness(10);              // все стороны = 10
var t2 = new Thickness(10, 20);          // горизонталь = 10, вертикаль = 20
var t3 = new Thickness(10, 20, 30, 40);  // L, T, R, B
```

### Color
```csharp
var c1 = new Color(255, 0, 0);           // красный, alpha = 255
var c2 = Color.FromArgb(128, 255, 0, 0); // полупрозрачный красный
var c3 = Color.Red;                      // предопределённый
```

## Контейнеры

### StackLayoutNode
Располагает детей в строку или столбец.

```csharp
var stack = new StackLayoutNode
{
    IsVertical = true,           // столбец
    Spacing = 8,                 // расстояние между детьми
    MainAxisAlignment = MainAxisAlignment.Center
};
stack.Children.Add(child1);
stack.Children.Add(child2);
```

**MainAxisAlignment:**
- `Start` — прижать к началу
- `Center` — по центру
- `End` — прижать к концу
- `SpaceBetween` — равномерно распределить

### GridNode
Сетка с произвольными размерами строк/колонок.

```csharp
var grid = new GridNode();
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(2)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

grid.Add(child1, 0, 0);  // строка 0, колонка 0
grid.Add(child2, 1, 1);  // строка 1, колонка 1
```

**GridLength:**
- `Pixels(value)` — фиксированный размер
- `Auto` — по содержимому
- `Star(weight)` — пропорционально свободному месту

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
// ...
```

### FixedNode
Узел с фиксированным размером. Полезен для распорок.

```csharp
var spacer = new FixedNode(0, 20);  // высота 20px
```

## Виджеты

### LabelNode
Текстовая метка.

```csharp
var label = new LabelNode("Hello", font, brush, labelComponent, textMeasurer);
label.SetSize(100, 50);  // опционально: фиксированный размер
```

**Привязки:**
```csharp
label.BindText(observableString);
label.BindFont(observableFont);
label.BindBrush(observableBrush);
```

### ImageNode
Изображение.

```csharp
var image = new ImageNode(bitmap, imageComponent);
image.SetSize(200, 150);
image.Component.SizeMode = ImageSizeMode.Stretch;
```

### BackgroundNode
Фон. Растягивается на весь слот, `DesiredSize = 0`.

```csharp
var bg = new BackgroundNode(Color.Blue, labelComponent, brushFactory);
```

### ClipNode
Маска для обрезки содержимого.

```csharp
var clip = new ClipNode(maskComponent);
clip.Children.Add(content);
```

## Observable<T>

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
- Автоматическое сравнение значений (не уведомляет, если значение не изменилось)
- Потокобезопасность через `lock`
- Исключения в подписчиках не ломают цепочку

## Fluent API

### UI фабрика
```csharp
// Инициализация (один раз)
UI.Factory = new WidgetFactory();
UI.Measurer = new TextMeasurer();
UI.BrushFactory = new BrushFactory();
UI.FontFactory = new FontFactory();
UI.ImageFactory = new ImageFactory();

// Создание узлов
var font = UI.Font("Segoe UI", 14f);
var brush = UI.Brush(Color.White);

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
    .VAlignment(Alignment.Stretch);
```

**Доступные расширения:**
- `Margin(Thickness)` / `Margin(int)` / `Margin(int h, int v)`
- `Padding(Thickness)` / `Padding(int)` / `Padding(int h, int v)`
- `HAlignment(Alignment)` / `VAlignment(Alignment)`
- `Add(LayoutNode child)`
- `Opacity(double)` / `Brightness(double)` / `Contrast(double)`
- `BindVisible(Observable<bool>)`
- `BindOpacity(Observable<double>)`

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

## Применение дерева

```csharp
// Measure + Arrange + привязка компонентов
rootNode.Apply(parentComponent, location, size);

// Перестроение (удаляет старые компоненты)
rootNode.Rebuild(parentComponent, location, size);

// Полное удаление
rootNode.Dispose();
```

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

## Обход дерева

```csharp
// Собрать все компоненты
var components = new List<IRenderComponent>();
rootNode.CollectComponents(components);

// Рекурсивно освободить ресурсы
rootNode.DisposeTree();
```

## Пример: полный UI

```csharp
// Инициализация
UI.Factory = new WidgetFactory();
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();

// Создание
var font = UI.Font("Segoe UI", 14f);
var white = UI.Brush(Color.White);
var green = UI.Brush(Color.LimeGreen);

var root = UI.Column(8)
    .Padding(20)
    .Add(UI.Label("Hello, DisplayNodes!", font, white))
    .Add(UI.Fixed(0, 4))
    .Add(UI.Row(12)
        .Add(UI.Label("Status:", font, UI.Brush(Color.Gray)))
        .Add(UI.Label("OK", font, green)));

// Применение
var displayRoot = new DisplayRoot(new RenderRootFactory(parent));
displayRoot.Build(root, Point.Empty, new Size(800, 600));
```

## Расширение

### Создание своего контейнера

```csharp
public class MyContainer : LayoutNode
{
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

## Лицензия

Copyright © 2026 AES