# Примеры использования DisplayNodes

Полный набор примеров для декларативной системы компоновки UI. Все примеры правильно управляют ресурсами, не вызывают утечек памяти, следуют best practices фреймворка.

## Содержание

1. [Инициализация](#init)
2. [Базовые типы](#types)
3. [Контейнеры](#containers)
4. [Виджеты](#widgets)
5. [Ограничения размеров](#constraints)
6. [Flex-механика](#flex)
7. [WrapPanel](#wrappanel)
8. [Border](#border)
9. [Реактивность](#observable)
10. [ComputedObservable](#computed)
11. [ObservableList](#observable-list)
12. [ConditionalNode](#conditional)
13. [Эффекты](#effects)
14. [Градиенты](#gradients)
15. [Shadow](#shadow)
16. [TransformNode](#transform)
17. [Маски](#masks)
18. [Полные примеры](#full-examples)
19. [Best practices](#best-practices)

---

<a id="init"></a>
## Инициализация

Перед использованием `UI.*` необходимо инициализировать фабрики. Вызовите **один раз** при старте приложения.

### С бэкендом WinForms

```csharp
using DisplayNodes.WinFormsAdapter;

Adapter.Initialize();
```

### Ручная инициализация (для тестов или кастомных бэкендов)

```csharp
using DisplayNodes.Fluent;
using DisplayNodes.Gdi;

UI.Factory = new WidgetFactory();
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();
UI.ImageFactory = new GdiImageFactory();
```

---

<a id="types"></a>
## Базовые типы

### Point

```csharp
using DisplayNodes.Core;

// Создание точки
var p1 = new Point(10, 20);
var p2 = Point.Empty;        // (0, 0)
var p3 = Point.Infinity;     // (int.MaxValue, int.MaxValue)

// Смещение
var moved = p1.Offset(5, 5);              // (15, 25)
var movedByThickness = p1.Offset(new Thickness(10, 20));  // (20, 40)

// Арифметика
var result = p1 + new Size(10, 10);       // (20, 30)
var diff = p1 - new Size(5, 5);           // (5, 15)

// Сравнение
bool equal = p1 == new Point(10, 20);     // true
```

### Size

```csharp
using DisplayNodes.Core;

// Создание размера
var s1 = new Size(100, 200);
var s2 = Size.Empty;        // (0, 0)
var s3 = Size.Infinity;     // (int.MaxValue, int.MaxValue)

// Изменение отдельных измерений
var wider = s1.WithWidth(300);    // (300, 200)
var taller = s1.WithHeight(400);  // (100, 400)

// Inflate/Deflate (добавление/вычитание отступов)
var t = new Thickness(10, 20);
var inflated = s1.Inflate(t);     // (120, 240)
var deflated = s1.Deflate(t);     // (80, 160)

// Арифметика
var sum = new Size(50, 50) + new Size(30, 40);    // (80, 90)
var diff = new Size(100, 100) - new Size(30, 40); // (70, 60)
```

### Rect

```csharp
using DisplayNodes.Core;

// Создание
var r1 = new Rect(10, 20, 100, 200);
var r2 = new Rect(new Point(10, 20), new Size(100, 200));
var r3 = Rect.Empty;

// Свойства
int right = r1.Right;    // 110
int bottom = r1.Bottom;  // 220

// Inflate/Deflate
var t = new Thickness(10, 20);
var inflated = r1.Inflate(t);
var deflated = r1.Deflate(t);

// Проверка содержимого
bool containsPoint = r1.Contains(new Point(50, 50));
bool containsRect = r1.Contains(new Rect(20, 30, 50, 50));

// Пересечение
bool intersects = r1.IntersectsWith(new Rect(50, 50, 100, 100));
var intersection = r1.Intersect(new Rect(50, 50, 100, 100));
```

### Thickness

```csharp
using DisplayNodes.Core;

var t1 = new Thickness(10);              // все стороны = 10
var t2 = new Thickness(10, 20);          // горизонталь = 10, вертикаль = 20
var t3 = new Thickness(10, 20, 30, 40);  // L=10, T=20, R=30, B=40
var t4 = Thickness.Zero;

// Свойства
int horizontal = t3.Horizontal;  // 40
int vertical = t3.Vertical;      // 60
bool isZero = t4.IsZero;         // true

// Сложение
var sum = t1 + t2;
```

### Color

```csharp
using DisplayNodes.Core;

var c1 = new Color(255, 0, 0);
var c2 = Color.FromArgb(128, 255, 0, 0);
var c3 = Color.FromRgb(0, 255, 0);

// Предопределённые
var black = Color.Black;
var white = Color.White;
var red = Color.Red;
var transparent = Color.Transparent;

// Сравнение
bool equal = c1 == Color.Red;  // true
```

### Percent

```csharp
using DisplayNodes.Core;

// Значение в диапазоне [0..100]
var p1 = new Percent(50);       // 50%
var p2 = Percent.Zero;          // 0%
var p3 = Percent.Hundred;       // 100%

// Неявное преобразование из int
Percent p4 = 75;                // 75%
int value = p4;                 // 75

// Валидация: ArgumentOutOfRangeException при выходе за диапазон
// var invalid = new Percent(101);  // исключение
```

### GradientStop

```csharp
using DisplayNodes.Core;

var stop1 = new GradientStop(Color.Red, 0);       // 0% — красный
var stop2 = new GradientStop(Color.Blue, 100);    // 100% — синий
var stop3 = new GradientStop(Color.Green, 50);    // 50% — зелёный
```

---

<a id="containers"></a>
## Контейнеры

### StackLayoutNode (Row / Column)

#### Вертикальный стек (Column)

```csharp
using DisplayNodes.Fluent;
using DisplayNodes.Core;

var font = UI.Font("Segoe UI", 14f);
var brush = UI.SolidBrush(Color.White);

var column = UI.Column(spacing: 8)
    .Padding(20)
    .Add(UI.Label("Первый", font, brush))
    .Add(UI.Label("Второй", font, brush))
    .Add(UI.Label("Третий", font, brush));
```

#### Горизонтальный стек (Row)

```csharp
var row = UI.Row(spacing: 12)
    .Add(UI.Label("Логин:", font, brush))
    .Add(UI.Label("admin", font, UI.SolidBrush(Color.LimeGreen)));
```

#### MainAxisAlignment

```csharp
// Прижать к началу (по умолчанию)
var start = UI.Column(8)
    .MainAlignment(MainAxisAlignment.Start)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Label("B", font, brush));

// По центру
var center = UI.Column(8)
    .MainAlignment(MainAxisAlignment.Center)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Label("B", font, brush));

// Прижать к концу
var end = UI.Column(8)
    .MainAlignment(MainAxisAlignment.End)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Label("B", font, brush));

// Равномерно распределить пространство
var spaceBetween = UI.Column(8)
    .MainAlignment(MainAxisAlignment.SpaceBetween)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Label("B", font, brush))
    .Add(UI.Label("C", font, brush));
```

### GridNode

```csharp
var grid = UI.Grid()
    .Add(UI.Label("A", font, brush), row: 0, column: 0)
    .Add(UI.Label("B", font, brush), row: 0, column: 1)
    .Add(UI.Label("C", font, brush), row: 1, column: 0)
    .Add(UI.Label("D", font, brush), row: 1, column: 1);

grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(2)));
```

**Типы размеров (GridLength):**

```csharp
var pixel = GridLength.Pixels(100);   // фиксированный
var auto = GridLength.Auto;            // по содержимому
var star1 = GridLength.Star(1);        // пропорционально (вес 1)
var star2 = GridLength.Star(2);        // пропорционально (вес 2)
var starDefault = GridLength.Star();   // вес 1 по умолчанию
```

### UniformGridNode

```csharp
var uniformGrid = UI.UniformGrid(rows: 3, columns: 3, spacing: 10)
    .Add(UI.Label("1", font, brush))
    .Add(UI.Label("2", font, brush))
    .Add(UI.Label("3", font, brush))
    .Add(UI.Label("4", font, brush))
    .Add(UI.Label("5", font, brush))
    .Add(UI.Label("6", font, brush))
    .Add(UI.Label("7", font, brush))
    .Add(UI.Label("8", font, brush))
    .Add(UI.Label("9", font, brush));
```

### OverlayNode

```csharp
var overlay = UI.Overlay()
    .Add(UI.Background(Color.Blue))
    .Add(UI.Label("Текст поверх фона", font, brush));
```

### FixedNode (Spacer)

```csharp
// Распорка высотой 20px
var spacer = UI.Fixed(0, 20);

// Или через алиас
var spacer2 = UI.Spacer(0, 20);

// Использование в стеке
var column = UI.Column(0)
    .Add(UI.Label("Верх", font, brush))
    .Add(UI.Fixed(0, 40))
    .Add(UI.Label("Низ", font, brush));
```

---

<a id="widgets"></a>
## Виджеты

### LabelNode

```csharp
// Базовое использование
var label = UI.Label("Hello, World!", font, brush);

// С фиксированным размером
var fixedLabel = UI.Label("Fixed", font, brush)
    .SetSize(200, 50);

// С фоном
var labelWithBg = UI.Label("Текст", font, brush)
    .BackgroundBrush(Color.Blue);

// Или через IBrush
var customBrush = UI.SolidBrush(Color.FromArgb(128, 0, 0, 255));
var labelWithCustomBg = UI.Label("Текст", font, brush)
    .BackgroundBrush(customBrush);

// Растягивание текста
var stretched = UI.Label("Длинный текст", font, brush)
    .SetSize(100, 50)
    .Stretch(LabelStretch.Horizontal);

// Метод отрисовки
var autoFit = UI.Label("Автомасштаб", font, brush)
    .SetSize(200, 100)
    .DrawMethod(TextDrawMethod.AutoFitInConstantRectangleWithoutWrap);

// Перенос строк
var wrapped = UI.Label("Очень длинный текст, который не помещается в одну строку", font, brush)
    .SetSize(200, 100)
    .DrawMethod(TextDrawMethod.AutoWrapInConstantRectangle);
```

### ImageNode

```csharp
// Из файла
var image = UI.Image(UI.ImageFromFile("icon.png"));

// Из потока
using (var stream = File.OpenRead("image.jpg"))
{
    var imageFromStream = UI.Image(UI.ImageFromStream(stream));
}

// Из байтов
byte[] bytes = File.ReadAllBytes("image.png");
var imageFromBytes = UI.Image(UI.ImageFromBytes(bytes));

// С фиксированным размером
var sizedImage = UI.Image(UI.ImageFromFile("logo.png"))
    .SetSize(200, 150);

// Оригинальный размер (центрируется)
var normal = UI.Image(UI.ImageFromFile("icon.png"))
    .SizeMode(ImageSizeMode.Normal);

// Растягивание
var stretch = UI.Image(UI.ImageFromFile("background.jpg"))
    .SizeMode(ImageSizeMode.Stretch);
```

### BackgroundNode

```csharp
// Простой цветной фон
var bg = UI.Background(Color.Blue);

// Через IBrush
var bgWithBrush = UI.Background(UI.SolidBrush(Color.FromArgb(240, 240, 245)));

// В overlay
var overlay = UI.Overlay()
    .Add(UI.Background(Color.FromArgb(240, 240, 245)))
    .Add(UI.Label("Контент", font, brush));
```

---

<a id="constraints"></a>
## Ограничения размеров

`LayoutNode` поддерживает `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight`. Применяются в базовом `Measure` после `MeasureOverride`.

```csharp
// Через fluent API
var label = UI.Label("Текст", font, brush)
    .MinWidth(100)
    .MaxWidth(400)
    .MinHeight(30)
    .MaxHeight(200);

// Или через диапазон
var label2 = UI.Label("Текст", font, brush)
    .WidthRange(100, 400)
    .HeightRange(30, 200);

// Через свойства
var label3 = UI.Label("Текст", font, brush);
label3.MinWidth = 100;
label3.MaxWidth = 400;
```

**Поведение:**
- Если `DesiredSize.Width < MinWidth` — итоговый размер = `MinWidth`.
- Если `DesiredSize.Width > MaxWidth` — итоговый размер = `MaxWidth`.
- `Margin` прибавляется **после** ограничений — `MaxWidth` ограничивает контент, а не полный размер с margin.

---

<a id="flex"></a>
## Flex-механика

`StackLayoutNode` поддерживает `FlexWeight` — пропорциональное распределение свободного пространства.

```csharp
// Flex-ребёнок растягивается на всё свободное место
var column = UI.Column(8)
    .Add(UI.Label("Fixed", font, brush))               // обычный размер
    .Add(UI.Label("Flexible", font, brush).Flex(1));    // забирает остаток

// Два flex-ребёнка делят место пропорционально
var column2 = UI.Column(0)
    .Add(UI.Label("1/3", font, brush).Flex(1))         // 1 доля из 3
    .Add(UI.Label("2/3", font, brush).Flex(2));        // 2 доли из 3
```

**Поведение:**
- `FlexWeight == 0` (по умолчанию) — узел имеет фиксированный размер.
- `FlexWeight > 0` — узел получает долю свободного места пропорционально весу.
- Если свободного места нет (`free == 0`) — flex-дети остаются в natural размере.
- Если места не хватает (`free < 0`) — flex-дети сжимаются, fixed — нет.

### StretchNode

`StretchNode` — публичный узел, растягивающийся вдоль обеих осей. Полезен как flex-ребёнок:

```csharp
var column = UI.Column(8)
    .Add(UI.Label("Top", font, brush))
    .Add(new StretchNode(0, 0).Flex(1))   // растягивается на всё свободное место
    .Add(UI.Label("Bottom", font, brush));
```

---

<a id="wrappanel"></a>
## WrapPanel

`WrapPanelNode` — контейнер с автоматическим переносом детей на следующую строку/столбец.

```csharp
// Горизонтальный wrap (строки, перенос вниз)
var horizontal = UI.WrapPanel(
    direction: WrapDirection.Horizontal,
    spacing: 8,        // между детьми в строке
    lineSpacing: 4)    // между строками
    .Add(UI.Label("Tag 1", font, brush))
    .Add(UI.Label("Tag 2", font, brush))
    .Add(UI.Label("Tag 3", font, brush))
    .Add(UI.Label("Tag 4", font, brush));

// Вертикальный wrap (столбцы, перенос вправо)
var vertical = UI.WrapPanel(
    direction: WrapDirection.Vertical,
    spacing: 8,
    lineSpacing: 4)
    .Add(UI.Label("Item 1", font, brush))
    .Add(UI.Label("Item 2", font, brush));
```

**Поведение:**
- Если ребёнок помещается в текущую строку — добавляется.
- Если не помещается — начинается новая строка.
- Если ребёнок шире `available` — остаётся в строке и переполняет (не переносится бесконечно).

---

<a id="border"></a>
## Border

`UI.Border` — композиция `OverlayNode` + `ClipNode` + `BackgroundNode`. Возвращает `OverlayNode` с фоном внутри.

```csharp
// С прямоугольной маской
var border = UI.Border(UI.SolidBrush(Color.Gray))
    .Add(UI.Label("Content", font, brush).Padding(12));

// Со скруглёнными углами
var rounded = UI.Border(UI.SolidBrush(Color.FromArgb(45, 45, 48)), cornerRadius: 8)
    .Padding(12)
    .Add(UI.Label("Rounded", font, brush));

// С цветом напрямую
var withColor = UI.Border(Color.Blue, cornerRadius: 4)
    .Padding(8)
    .Add(UI.Label("Blue", font, brush));

// С градиентом (при поддержке в адаптере)
var withGradient = UI.Border(
    UI.LinearGradient(
        new Point(0, 0), new Point(0, 100),
        new GradientStop(Color.Blue, 0),
        new GradientStop(Color.White, 100)),
    cornerRadius: 8)
    .Add(UI.Label("Gradient", font, brush).Padding(12));
```

**Структура:**
- Первый ребёнок `OverlayNode` — `ClipNode` с `BackgroundNode` внутри.
- Контент, добавленный через `.Add(...)`, рисуется **поверх** фона.

---

<a id="observable"></a>
## Реактивность

### Observable\<T\>

```csharp
using DisplayNodes.Core;

var counter = new Observable<int>(0);

// Подписка
var subscription = counter.Subscribe(value => 
{
    Console.WriteLine($"Counter: {value}");
});

// Изменение — уведомит подписчиков
counter.Value = 10;  // выведет: "Counter: 10"

// Повторная установка того же значения не вызовет уведомление
counter.Value = 10;  // ничего не выведет

// Отписка
subscription.Dispose();
counter.Value = 20;  // ничего не выведет
```

### Привязка к виджетам

```csharp
var textObservable = new Observable<string>("Начальное значение");
var colorObservable = new Observable<IBrush>(UI.SolidBrush(Color.White));

var label = UI.Label("Текст", font, brush)
    .BindText(textObservable)
    .BindForegroundBrush(colorObservable);

// Изменение автоматически обновит UI
textObservable.Value = "Новый текст";
colorObservable.Value = UI.SolidBrush(Color.LimeGreen);
```

### Привязка видимости

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Видимый текст", font, brush)
    .BindVisible(isVisible);

isVisible.Value = false;  // метка скроется
isVisible.Value = true;   // метка появится
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

opacity.Value = 50.0;
brightness.Value = 20.0;
contrast.Value = -10.0;
```

### Привязка изображения

```csharp
var imageObservable = new Observable<IImage>(UI.ImageFromFile("default.png"));

var imageNode = UI.Image()
    .BindBitmap(imageObservable);

imageObservable.Value = UI.ImageFromFile("new.png");
```

### Привязка режима отображения

```csharp
var sizeModeObservable = new Observable<ImageSizeMode>(ImageSizeMode.Normal);

var imageNode = UI.Image(UI.ImageFromFile("icon.png"))
    .BindSizeMode(sizeModeObservable);

sizeModeObservable.Value = ImageSizeMode.Stretch;
```

### Привязка шрифта и фона

```csharp
var fontObservable = new Observable<IFont>(font);
var backgroundObservable = new Observable<IBrush>(UI.SolidBrush(Color.Black));

var label = UI.Label("Text", font, brush)
    .BindFont(fontObservable)
    .BindBackgroundBrush(backgroundObservable);
```

---

<a id="computed"></a>
## ComputedObservable

`ComputedObservable<T>` — реактивное свойство, значение которого вычисляется из других источников.

```csharp
using DisplayNodes.Core;

var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName,
    lastName);

// fullName.Value == "Ivan Petrov"
firstName.Value = "Petr";
// fullName.Value == "Petr Petrov" (автоматически)

// Подписка на изменения
var subscription = fullName.Subscribe(v => Console.WriteLine(v));

// Ручной пересчёт (если зависимости изменились в обход Observable)
fullName.Refresh();

// Освобождение
fullName.Dispose();
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
    num, text);

// result.Value == "Hello: 10"
num.Value = 20;
// result.Value == "Hello: 20"
```

---

<a id="observable-list"></a>
## ObservableList

`ObservableList<T>` — реактивная коллекция с событием `Changed`.

```csharp
using DisplayNodes.Core;

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

// Снимок для безопасного перебора
foreach (var item in list.ToList())
{
    // Можно модифицировать list без InvalidOperationException
}

// Освобождение
list.Dispose();
```

### Типы изменений

| Тип | OldIndex | NewIndex | Item |
|---|---|---|---|
| `Add` | -1 | индекс | добавленный |
| `Insert` | -1 | индекс | вставленный |
| `Remove` | индекс | -1 | удалённый |
| `Replace` | индекс | индекс | новый |
| `Move` | откуда | куда | перемещённый |
| `Reset` | -1 | -1 | default |

---

<a id="conditional"></a>
## ConditionalNode

`ConditionalNode` (через `UI.When`) — контейнер, отображающий одно из двух поддеревьев по `Observable<bool>`.

```csharp
var isLoggedIn = new Observable<bool>(false);

var conditional = UI.When(
    isLoggedIn,
    trueNode: UI.Label("Welcome!", font, greenBrush),
    falseNode: UI.Label("Please log in", font, grayBrush));

// При isLoggedIn.Value = true отображается "Welcome!"
// При isLoggedIn.Value = false отображается "Please log in"

// Подписка на переключение
conditional.OnChanged(value =>
{
    Console.WriteLine($"Condition changed to: {value}");
});
```

**Ограничение:** `ConditionalNode` **не пересчитывает layout автоматически** при переключении. Пользователь должен вызвать `Measure`+`Arrange`+`Refresh` вручную или перестроить дерево целиком.

### Только true-ветка

```csharp
var isLoading = new Observable<bool>(true);

var conditional = UI.When(
    isLoading,
    trueNode: UI.Label("Loading...", font, grayBrush));
    // falseNode не задан — при false отображается пустота
```

---

<a id="effects"></a>
## Эффекты

Применяются к виджетам, чей компонент реализует `IEffectComponent` (Label, Image).

### Opacity (прозрачность)

```csharp
var semiTransparent = UI.Label("Полупрозрачный", font, brush)
    .Opacity(50.0);

var image = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Opacity(75.0);
```

### Brightness (яркость)

```csharp
var darker = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Brightness(-30.0);

var brighter = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Brightness(50.0);
```

### Contrast (контраст)

```csharp
var lowContrast = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Contrast(-20.0);

var highContrast = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Contrast(40.0);
```

### Комбинирование

```csharp
var styledImage = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Opacity(90.0)
    .Brightness(10.0)
    .Contrast(20.0);
```

---

<a id="gradients"></a>
## Градиенты

> **Ограничение:** адаптеры **не поддерживают** градиентные кисти. Установка градиента в компонент приводит к `NotSupportedException`. API подготовлен для будущей реализации.

### Линейный градиент

```csharp
var gradient = UI.LinearGradient(
    new Point(0, 0),      // начало: левый верхний угол
    new Point(100, 0),    // конец: правый верхний угол
    new GradientStop(Color.Red, 0),
    new GradientStop(Color.Blue, 100));

var bg = UI.Background(gradient);
```

### Радиальный градиент

```csharp
var radial = UI.RadialGradient(
    new Point(50, 50),    // центр
    radius: 50,            // радиус в процентах
    new GradientStop(Color.White, 0),
    new GradientStop(Color.Black, 100));

var bg = UI.Background(radial);
```

### Многостоповый градиент

```csharp
var rainbow = UI.LinearGradient(
    new Point(0, 0), new Point(100, 0),
    new GradientStop(Color.Red, 0),
    new GradientStop(Color.Yellow, 25),
    new GradientStop(Color.Green, 50),
    new GradientStop(Color.Cyan, 75),
    new GradientStop(Color.Blue, 100));
```

---

<a id="shadow"></a>
## Shadow

> **Ограничение:** адаптеры **бросают `NotSupportedException`** при попытке установить тень. API подготовлен для будущей реализации.

```csharp
// Три перегрузки:
// 1. Готовая структура Shadow
var shadow1 = new Shadow(2, 4, 8, Color.FromArgb(80, 0, 0, 0));
var card1 = UI.Border(Color.White).Shadow(shadow1);

// 2. С явным цветом
var card2 = UI.Border(Color.White).Shadow(2, 4, 8, Color.FromArgb(80, 0, 0, 0));

// 3. Со стандартным цветом (полупрозрачный чёрный, RGBA = 0,0,0,80)
var card3 = UI.Border(Color.White).Shadow(2, 4, 8);
```

---

<a id="transform"></a>
## TransformNode

> **Ограничение:** `ApplyRecursive` **бросает `NotSupportedException`** при обнаружении `TransformNode`. Layout для `TransformNode` работает корректно — исключение возникает только при попытке рендеринга.

### Поворот

```csharp
var rotated = UI.Transform(rotation: 45f)
    .Add(UI.Label("Rotated", font, brush));
```

### Масштабирование

```csharp
var scaled = UI.Transform(scaleX: 1.5f, scaleY: 1.5f)
    .Add(UI.Label("Scaled", font, brush));
```

### Явный origin

```csharp
var rotatedAroundTopLeft = UI.Transform(
    scaleX: 1f, scaleY: 1f, rotation: 90f,
    origin: new Point(0, 0))
    .Add(UI.Label("Rotated around top-left", font, brush));
```

### Bounding box учитывается в layout

`TransformNode.Measure` возвращает bounding box трансформированного ребёнка, поэтому родительские контейнеры резервируют правильное место:

```csharp
UI.Column(spacing: 8)
    .Add(UI.Transform(rotation: 45f)
        .Add(UI.Fixed(100, 100)))
    // bounding box ~142x142
    .Add(UI.Label("Below", font, brush));  // расположится ниже, без перекрытия
```

---

<a id="masks"></a>
## Маски

### ClipNode

```csharp
// Прямоугольная маска (по умолчанию)
var clip = UI.Clip()
    .Add(UI.Image(UI.ImageFromFile("photo.jpg")));

// Круговая маска
var circleClip = UI.ClipCircle()
    .Add(UI.Image(UI.ImageFromFile("avatar.png")));

// Эллиптическая маска
var ellipseClip = UI.ClipEllipse()
    .Add(UI.Image(UI.ImageFromFile("photo.jpg")));

// Скруглённые углы
var roundedClip = UI.ClipRoundedRect(cornerRadius: 8f)
    .Add(UI.Image(UI.ImageFromFile("card.png")));

// Произвольная форма
var customClip = UI.ClipPath(rect =>
{
    var path = new System.Drawing.Drawing2D.GraphicsPath();
    // ... логика рисования пути внутри rect ...
    return new DisplayNodes.Gdi.GdiGraphicsPath(path);
})
.Add(UI.Image(UI.ImageFromFile("star.png")));
```

---

<a id="full-examples"></a>
## Полные примеры

### Пример 1: Простая форма с логином

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;

public static LayoutNode BuildLoginForm()
{
    var font = UI.Font("Segoe UI", 14f);
    var whiteBrush = UI.SolidBrush(Color.White);
    var grayBrush = UI.SolidBrush(Color.Gray);
    var greenBrush = UI.SolidBrush(Color.LimeGreen);

    return UI.Column(12)
        .Padding(20)
        .Add(UI.Background(Color.FromArgb(45, 45, 48)))
        .Add(UI.Label("Вход в систему", font, whiteBrush))
        .Add(UI.Fixed(0, 20))
        .Add(UI.Row(8)
            .Add(UI.Label("Логин:", font, whiteBrush))
            .Add(UI.Label("admin", font, greenBrush)))
        .Add(UI.Row(8)
            .Add(UI.Label("Пароль:", font, whiteBrush))
            .Add(UI.Label("••••••", font, grayBrush)))
        .Add(UI.Fixed(0, 20))
        .Add(UI.Row(12)
            .MainAlignment(MainAxisAlignment.Center)
            .Add(UI.Label("Войти", font, whiteBrush)
                .BackgroundBrush(Color.FromArgb(0, 120, 215)))
            .Add(UI.Label("Отмена", font, whiteBrush)
                .BackgroundBrush(Color.FromArgb(80, 80, 80))));
}
```

### Пример 2: Карточка пользователя

```csharp
public static LayoutNode BuildUserCard(string name, string role, string avatarPath)
{
    var font = UI.Font("Segoe UI", 14f);
    var smallFont = UI.Font("Segoe UI", 12f);
    var whiteBrush = UI.SolidBrush(Color.White);
    var grayBrush = UI.SolidBrush(Color.Gray);

    return UI.Overlay()
        .Add(UI.Background(Color.FromArgb(250, 250, 250)))
        .Add(UI.Column(12)
            .Padding(16)
            .Add(UI.Row(12)
                .Add(UI.ClipCircle()
                    .Add(UI.Image(UI.ImageFromFile(avatarPath))
                        .SetSize(60, 60)))
                .Add(UI.Column(4)
                    .Add(UI.Label(name, font, whiteBrush))
                    .Add(UI.Label(role, smallFont, grayBrush))))
            .Add(UI.Fixed(0, 8))
            .Add(UI.Label("Email: user@example.com", font, grayBrush))
            .Add(UI.Label("Телефон: +7 (999) 123-45-67", font, grayBrush)));
}
```

### Пример 3: Дашборд с сеткой

```csharp
public static LayoutNode BuildDashboard()
{
    var grid = UI.Grid()
        .Padding(10);

    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

    grid.Add(BuildCard("Продажи", "1,234", Color.Blue), 0, 0);
    grid.Add(BuildCard("Заказы", "567", Color.Green), 0, 1);
    grid.Add(BuildCard("Клиенты", "89", Color.Orange), 1, 0);
    grid.Add(BuildCard("Выручка", "$45,678", Color.Purple), 1, 1);
    grid.Add(BuildCard("Конверсия", "12.5%", Color.Red), 2, 0);
    grid.Add(BuildCard("Средний чек", "$89", Color.Teal), 2, 1);

    return grid;
}

private static LayoutNode BuildCard(string title, string value, Color accentColor)
{
    var titleFont = UI.Font("Segoe UI", 12f);
    var valueFont = UI.Font("Segoe UI", 24f, bold: true);
    var whiteBrush = UI.SolidBrush(Color.White);

    return UI.Overlay()
        .Add(UI.Background(accentColor))
        .Add(UI.Column(8)
            .Padding(16)
            .Add(UI.Label(title, titleFont, whiteBrush))
            .Add(UI.Label(value, valueFont, whiteBrush)));
}
```

### Пример 4: Список с Flex

```csharp
public static LayoutNode BuildListWithHeaderAndFooter()
{
    var font = UI.Font("Segoe UI", 14f);
    var brush = UI.SolidBrush(Color.White);

    return UI.Column(0)
        .Add(UI.Label("Header", font, brush).Padding(12).BackgroundBrush(Color.Blue))
        .Add(new StretchNode(0, 0).Flex(1))  // растягивается на всё свободное место
        .Add(UI.Label("Footer", font, brush).Padding(12).BackgroundBrush(Color.Gray));
}
```

### Пример 5: Реактивный счётчик

```csharp
public static LayoutNode BuildCounter()
{
    var counter = new Observable<int>(0);
    var counterText = new ComputedObservable<string>(
        () => counter.Value.ToString(),
        counter);

    var font = UI.Font("Segoe UI", 48f, bold: true);
    var whiteBrush = UI.SolidBrush(Color.White);

    var counterLabel = UI.Label("0", font, whiteBrush)
        .BindText(counterText);

    return UI.Column(20)
        .Padding(40)
        .MainAlignment(MainAxisAlignment.Center)
        .Add(counterLabel)
        .Add(UI.Row(20)
            .MainAlignment(MainAxisAlignment.Center)
            .Add(UI.Label("- decrement", UI.Font("Segoe UI", 16f), whiteBrush)
                .BackgroundBrush(Color.Red))
            .Add(UI.Label("+ increment", UI.Font("Segoe UI", 16f), whiteBrush)
                .BackgroundBrush(Color.Green)));
}
```

### Пример 6: Галерея изображений

```csharp
public static LayoutNode BuildImageGallery(string[] imagePaths)
{
    var grid = UI.UniformGrid(rows: 3, columns: 3, spacing: 8)
        .Padding(10);

    foreach (var path in imagePaths.Take(9))
    {
        grid.Add(UI.ClipRoundedRect(8f)
            .Add(UI.Image(UI.ImageFromFile(path))
                .SetSize(150, 150)
                .SizeMode(ImageSizeMode.Stretch)));
    }

    return grid;
}
```

### Пример 7: Условный рендеринг

```csharp
public static LayoutNode BuildConditionalView()
{
    var isLoggedIn = new Observable<bool>(false);
    var font = UI.Font("Segoe UI", 14f);
    var whiteBrush = UI.SolidBrush(Color.White);
    var grayBrush = UI.SolidBrush(Color.Gray);

    return UI.Column(12)
        .Padding(20)
        .Add(UI.When(
            isLoggedIn,
            trueNode: UI.Label("Welcome back!", font, whiteBrush),
            falseNode: UI.Label("Please log in", font, grayBrush)));
}
```

### Пример 8: Панель тегов с WrapPanel

```csharp
public static LayoutNode BuildTagPanel(string[] tags)
{
    var font = UI.Font("Segoe UI", 12f);
    var whiteBrush = UI.SolidBrush(Color.White);

    var panel = UI.WrapPanel(
        direction: WrapDirection.Horizontal,
        spacing: 8,
        lineSpacing: 8)
        .Padding(12);

    foreach (var tag in tags)
    {
        panel.Add(UI.Label(tag, font, whiteBrush)
            .BackgroundBrush(Color.FromArgb(60, 60, 60))
            .Padding(8, 4));
    }

    return panel;
}
```

---

<a id="best-practices"></a>
## Best practices

### 1. Инициализация UI.*

```csharp
// ✅ ПРАВИЛЬНО: один раз при старте
Adapter.Initialize();

// ❌ НЕПРАВИЛЬНО: многократная инициализация
UI.Factory = new WidgetFactory();
UI.Factory = new WidgetFactory();  // перезапись
```

### 2. Управление ресурсами

```csharp
// ✅ ПРАВИЛЬНО: consumer владеет ресурсом, адаптер клонирует
using (var font = new Font("Arial", 12f))
{
    var wrapped = font.Wrap();
    var label = UI.Label("Text", wrapped, brush);
    // font всё ещё жив, адаптер владеет своей копией
}

// ❌ НЕПРАВИЛЬНО: передача владения адаптеру
var font = new Font("Arial", 12f);
var label = UI.Label("Text", font.Wrap(), brush);
font.Dispose();  // адаптер может использовать уже освобождённый ресурс
```

### 3. Dispose деревьев

```csharp
// ✅ ПРАВИЛЬНО: автоматический Dispose через DisplayRoot
var displayRoot = new DisplayRoot(new RenderRootFactory(parent));
displayRoot.Build(rootNode, location, size);
// ... использование ...
displayRoot.Dispose();

// ✅ ПРАВИЛЬНО: ручной Dispose дерева
rootNode.Dispose();

// ❌ НЕПРАВИЛЬНО: забытый Dispose
var displayRoot = new DisplayRoot(factory);
displayRoot.Build(rootNode, location, size);
// забыли вызвать Dispose — утечка ресурсов
```

### 4. Observable и подписки

```csharp
// ✅ ПРАВИЛЬНО: привязка через fluent API (автоматическая отписка)
var observable = new Observable<string>("value");
var label = UI.Label("text", font, brush).BindText(observable);
// при Dispose label автоматически отпишется

// ❌ НЕПРАВИЛЬНО: ручная подписка без отписки
var observable = new Observable<string>("value");
observable.Subscribe(v => label.Component.Text = v);
// утечка памяти: observable держит ссылку на label
```

### 5. Избегание лишних Measure/Arrange

```csharp
// ✅ ПРАВИЛЬНО: один вызов Apply
rootNode.Apply(parent, location, size);

// ❌ НЕПРАВИЛЬНО: многократные вызовы
rootNode.Measure(size);
rootNode.Arrange(rect);
rootNode.Measure(newSize);  // лишний вызов
rootNode.Arrange(newRect);
```

### 6. Использование FixedNode для распорок

```csharp
// ✅ ПРАВИЛЬНО: FixedNode для отступов
var column = UI.Column(0)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Fixed(0, 20))  // распорка 20px
    .Add(UI.Label("B", font, brush));

// ❌ НЕПРАВИЛЬНО: пустые Label для отступов
var column = UI.Column(0)
    .Add(UI.Label("A", font, brush))
    .Add(UI.Label("", font, brush).SetSize(0, 20))  // лишний компонент
    .Add(UI.Label("B", font, brush));
```

### 7. Использование Flex для пропорциональных размеров

```csharp
// ✅ ПРАВИЛЬНО: flex для растягивания
var column = UI.Column(8)
    .Add(UI.Label("Header", font, brush).Flex(0))
    .Add(UI.Label("Content", font, brush).Flex(1))   // забирает остаток
    .Add(UI.Label("Footer", font, brush).Flex(0));

// ❌ НЕПРАВИЛЬНО: ручной расчёт размеров
var column = UI.Column(8)
    .Add(UI.Label("Header", font, brush))
    .Add(UI.Label("Content", font, brush).SetSize(800, 400))  // хардкод
    .Add(UI.Label("Footer", font, brush));
```

### 8. Использование StretchNode вместо FixedNode с Flex

```csharp
// ✅ ПРАВИЛЬНО: StretchNode растягивается сам
var column = UI.Column(0)
    .Add(UI.Label("Top", font, brush))
    .Add(new StretchNode(0, 0).Flex(1))
    .Add(UI.Label("Bottom", font, brush));

// ❌ НЕПРАВИЛЬНО: FixedNode не растягивается
var column = UI.Column(0)
    .Add(UI.Label("Top", font, brush))
    .Add(UI.Fixed(0, 0).Flex(1))  // FixedNode игнорирует Flex
    .Add(UI.Label("Bottom", font, brush));
```

### 9. Избегание хардкода размеров

```csharp
// ✅ ПРАВИЛЬНО: Min/Max ограничения вместо фиксированного размера
var label = UI.Label("Text", font, brush)
    .MinWidth(100)
    .MaxWidth(400);

// ❌ НЕПРАВИЛЬНО: фиксированный размер
var label = UI.Label("Text", font, brush).SetSize(400, 50);
```

### 10. Потокобезопасность

```csharp
// ✅ ПРАВИЛЬНО: изменение Observable в UI-потоке
Dispatcher.Invoke(() => counter.Value++);

// ✅ ПРАВИЛЬНО: Observable потокобезопасен
Task.Run(() => counter.Value = 10);  // безопасно

// ❌ НЕПРАВИЛЬНО: изменение UI-компонентов из фонового потока
Task.Run(() =>
{
    label.Component.Text = "New text";  // исключение!
});
```

### 11. Использование ComputedObservable вместо ручных подписок

```csharp
// ✅ ПРАВИЛЬНО: ComputedObservable сам управляет подписками
var a = new Observable<int>(1);
var b = new Observable<int>(2);
var sum = new ComputedObservable<int>(() => a.Value + b.Value, a, b);

// ❌ НЕПРАВИЛЬНО: ручное управление
var a = new Observable<int>(1);
var b = new Observable<int>(2);
var sum = new Observable<int>(3);
a.Subscribe(_ => sum.Value = a.Value + b.Value);
b.Subscribe(_ => sum.Value = a.Value + b.Value);
// при Dispose каждой подписки легко забыть отписку
```