# Примеры использования DisplayNodes

Полный набор примеров для декларативной системы компоновки UI. Все примеры правильно управляют ресурсами, не вызывают утечек памяти, следуют best practices фреймворка.

## Содержание

1. [Инициализация](#инициализация)
2. [Базовые типы](#базовые-типы)
3. [Контейнеры](#контейнеры)
4. [Виджеты](#виджеты)
5. [Реактивность](#реактивность)
6. [Эффекты](#эффекты)
7. [Маски](#маски)
8. [Полные примеры](#полные-примеры)
9. [Best practices](#best-practices)

---

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
var inflated = s1.Inflate(t);     // (120, 240) = (100+20, 200+40)
var deflated = s1.Deflate(t);     // (80, 160) = (100-20, 200-40)

// Deflate никогда не возвращает отрицательные значения
var small = new Size(10, 10);
var safe = small.Deflate(new Thickness(20, 20));  // (0, 0), не (-30, -30)

// Арифметика
var sum = new Size(50, 50) + new Size(30, 40);    // (80, 90)
var diff = new Size(100, 100) - new Size(30, 40); // (70, 60)
```

### Rect

```csharp
using DisplayNodes.Core;

// Создание прямоугольника
var r1 = new Rect(10, 20, 100, 200);
var r2 = new Rect(new Point(10, 20), new Size(100, 200));
var r3 = Rect.Empty;  // (0, 0, 0, 0)

// Свойства
int right = r1.Right;    // 110 (X + Width)
int bottom = r1.Bottom;  // 220 (Y + Height)
bool isEmpty = r1.IsEmpty;  // false

// Inflate/Deflate
var t = new Thickness(10, 20);
var inflated = r1.Inflate(t);   // (0, 0, 120, 240)
var deflated = r1.Deflate(t);   // (20, 40, 80, 160)

// Проверка содержимого
bool containsPoint = r1.Contains(new Point(50, 50));   // true
bool containsRect = r1.Contains(new Rect(20, 30, 50, 50));  // true

// Пересечение
bool intersects = r1.IntersectsWith(new Rect(50, 50, 100, 100));  // true
var intersection = r1.Intersect(new Rect(50, 50, 100, 100));
// intersection = (50, 50, 60, 170)
```

### Thickness

```csharp
using DisplayNodes.Core;

// Создание отступов
var t1 = new Thickness(10);              // все стороны = 10
var t2 = new Thickness(10, 20);          // горизонталь = 10, вертикаль = 20
var t3 = new Thickness(10, 20, 30, 40);  // L=10, T=20, R=30, B=40
var t4 = Thickness.Zero;                 // (0, 0, 0, 0)

// Свойства
int horizontal = t3.Horizontal;  // 40 (Left + Right)
int vertical = t3.Vertical;      // 60 (Top + Bottom)
bool isZero = t4.IsZero;         // true

// Сложение
var sum = t1 + t2;  // (20, 30, 20, 30)
```

### Color

```csharp
using DisplayNodes.Core;

// Создание цвета
var c1 = new Color(255, 0, 0);           // красный, alpha = 255
var c2 = Color.FromArgb(128, 255, 0, 0); // полупрозрачный красный
var c3 = Color.FromRgb(0, 255, 0);       // зелёный, alpha = 255

// Предопределённые цвета
var black = Color.Black;
var white = Color.White;
var red = Color.Red;
var green = Color.Green;
var blue = Color.Blue;
var transparent = Color.Transparent;

// Сравнение
bool equal = c1 == Color.Red;  // true
```

---

## Контейнеры

### StackLayoutNode (Row / Column)

Располагает детей в строку или столбец.

#### Вертикальный стек (Column)

```csharp
using DisplayNodes.Fluent;
using DisplayNodes.Core;

var font = UI.Font("Segoe UI", 14f);
var brush = UI.Brush(Color.White);

var column = UI.Column(spacing: 8)
    .Padding(20)
    .Add(UI.Label("Первый", font, brush))
    .Add(UI.Label("Второй", font, brush))
    .Add(UI.Label("Третий", font, brush));

// Размер: сумма высот детей + spacing + padding
// DesiredSize = (maxWidth + 40, sumHeights + 16 + 40)
```

#### Горизонтальный стек (Row)

```csharp
var row = UI.Row(spacing: 12)
    .Add(UI.Label("Логин:", font, brush))
    .Add(UI.Label("admin", font, UI.Brush(Color.LimeGreen)));

// Размер: сумма ширин детей + spacing
```

#### Выравнивание по главной оси (MainAxisAlignment)

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

Сетка с произвольными размерами строк/колонок.

#### Базовая сетка

```csharp
var grid = UI.Grid()
    .Add(UI.Label("A", font, brush), row: 0, column: 0)
    .Add(UI.Label("B", font, brush), row: 0, column: 1)
    .Add(UI.Label("C", font, brush), row: 1, column: 0)
    .Add(UI.Label("D", font, brush), row: 1, column: 1);

// Добавить определения строк и колонок
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(2)));
```

#### Типы размеров (GridLength)

```csharp
// Фиксированный размер в пикселях
var pixel = GridLength.Pixels(100);

// По содержимому (автоматически)
var auto = GridLength.Auto;

// Пропорционально свободному месту
var star1 = GridLength.Star(1);  // коэффициент 1
var star2 = GridLength.Star(2);  // коэффициент 2 (в 2 раза больше)
var starDefault = GridLength.Star();  // коэффициент 1 (по умолчанию)
```

#### Пример: форма с фиксированной и растягивающейся частью

```csharp
var form = UI.Grid()
    .Padding(10);

// Строки: заголовок (auto), контент (star), кнопки (auto)
form.RowDefinitions.Add(new RowDefinition(GridLength.Auto));    // заголовок
form.RowDefinitions.Add(new RowDefinition(GridLength.Star(1))); // контент
form.RowDefinitions.Add(new RowDefinition(GridLength.Auto));    // кнопки

// Колонки: лейбл (auto), поле ввода (star)
form.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
form.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

// Заполнение
form.Add(UI.Label("Имя:", font, brush), 0, 0);
form.Add(UI.Label("Описание:", font, brush), 1, 0);
form.Add(UI.Label("Поле ввода...", font, grayBrush), 0, 1);
form.Add(UI.Label("Многострочное поле...", font, grayBrush), 1, 1);
form.Add(UI.Label("Заголовок формы", font, brush), 2, 0);
// Кнопки в последней строке
form.Add(UI.Row(8)
    .Add(UI.Label("OK", font, brush))
    .Add(UI.Label("Отмена", font, brush)), 2, 1);
```

### UniformGridNode

Равномерная сетка с фиксированным числом строк/колонок. Все ячейки одинакового размера.

```csharp
// Сетка 3x3 с расстоянием 10px между ячейками
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

// Размер ячейки = max(доступное_пространство / columns, maxChildSize)
```

### OverlayNode

Размещает всех детей в одном слоте (друг поверх друга).

```csharp
var overlay = UI.Overlay()
    .Add(UI.Background(Color.Blue))  // фон
    .Add(UI.Label("Текст поверх фона", font, brush));  // контент

// Размер overlay = размер наибольшего ребёнка
// Все дети получают одинаковый слот
```

### FixedNode (Spacer)

Узел с фиксированным размером. Полезен для распорок.

```csharp
// Распорка высотой 20px
var spacer = UI.Fixed(0, 20);

// Или через алиас
var spacer2 = UI.Spacer(0, 20);

// Использование в стеке
var column = UI.Column(0)
    .Add(UI.Label("Верх", font, brush))
    .Add(UI.Fixed(0, 40))  // отступ 40px
    .Add(UI.Label("Низ", font, brush));
```

---

## Виджеты

### LabelNode

Текстовая метка.

#### Базовое использование

```csharp
var label = UI.Label("Hello, World!", font, brush);

// С фиксированным размером
var fixedLabel = UI.Label("Fixed", font, brush)
    .SetSize(200, 50);
```

#### Фон метки

```csharp
var labelWithBg = UI.Label("Текст", font, brush)
    .FullBrush(Color.Blue);  // цвет фона

// Или через IBrush
var customBrush = UI.Brush(Color.FromArgb(128, 0, 0, 255));
var labelWithCustomBg = UI.Label("Текст", font, brush)
    .FullBrush(customBrush);
```

#### Форматирование текста

```csharp
// Режим растяжки текста
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

Изображение.

#### Базовое использование

```csharp
// Из файла
var image = UI.Image(UI.ImageFromFile("icon.png"));

// Из потока
using (var stream = File.OpenRead("image.jpg"))
{
    var imageFromStream = UI.Image(UI.ImageFromStream(stream));
}

// Из байт
byte[] bytes = File.ReadAllBytes("image.png");
var imageFromBytes = UI.Image(UI.ImageFromBytes(bytes));

// С фиксированным размером
var sizedImage = UI.Image(UI.ImageFromFile("logo.png"))
    .SetSize(200, 150);
```

#### Режим отображения

```csharp
// Оригинальный размер (центрируется)
var normal = UI.Image(UI.ImageFromFile("icon.png"))
    .SizeMode(ImageSizeMode.Normal);

// Растягивание с сохранением пропорций
var stretch = UI.Image(UI.ImageFromFile("background.jpg"))
    .SizeMode(ImageSizeMode.Stretch);
```

### BackgroundNode

Фон. Растягивается на весь слот, `DesiredSize = 0`.

```csharp
// Простой цветной фон
var bg = UI.Background(Color.Blue);

// Использование в overlay
var overlay = UI.Overlay()
    .Add(UI.Background(Color.FromArgb(240, 240, 245)))
    .Add(UI.Label("Контент", font, brush));

// В стеке (фон занимает весь слот)
var column = UI.Column(0)
    .Add(UI.Background(Color.DarkBlue))
    .Add(UI.Label("Текст", font, brush));
```

---

## Реактивность

### Observable<T>

Реактивное свойство с подпиской на изменения.

#### Базовое использование

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

#### Привязка к виджетам

```csharp
var textObservable = new Observable<string>("Начальное значение");
var colorObservable = new Observable<IBrush>(UI.Brush(Color.White));

var label = UI.Label("Текст", font, brush)
    .BindText(textObservable)
    .BindBrush(colorObservable);

// Изменение автоматически обновит UI
textObservable.Value = "Новый текст";
colorObservable.Value = UI.Brush(Color.LimeGreen);

// Подписки автоматически отписываются при Dispose виджета
```

#### Привязка видимости

```csharp
var isVisible = new Observable<bool>(true);

var label = UI.Label("Видимый текст", font, brush)
    .BindVisible(isVisible);

isVisible.Value = false;  // метка скроется
isVisible.Value = true;   // метка появится
```

#### Привязка эффектов

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

#### Привязка изображения

```csharp
var imageObservable = new Observable<IImage>(UI.ImageFromFile("default.png"));

var imageNode = UI.Image()
    .BindBitmap(imageObservable);

imageObservable.Value = UI.ImageFromFile("new.png");  // изображение обновится
```

#### Привязка режима отображения

```csharp
var sizeModeObservable = new Observable<ImageSizeMode>(ImageSizeMode.Normal);

var imageNode = UI.Image(UI.ImageFromFile("icon.png"))
    .BindSizeMode(sizeModeObservable);

sizeModeObservable.Value = ImageSizeMode.Stretch;  // режим изменится
```

---

## Эффекты

Применяются к виджетам, чей компонент реализует `IEffectComponent` (Label, Image).

### Opacity (прозрачность)

```csharp
// Прозрачность 0-100
var semiTransparent = UI.Label("Полупрозрачный", font, brush)
    .Opacity(50.0);

var image = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Opacity(75.0);
```

### Brightness (яркость)

```csharp
// Яркость -100...100
var darker = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Brightness(-30.0);

var brighter = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Brightness(50.0);
```

### Contrast (контраст)

```csharp
// Контраст -100...100
var lowContrast = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Contrast(-20.0);

var highContrast = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Contrast(40.0);
```

### Комбинация эффектов

```csharp
var styledImage = UI.Image(UI.ImageFromFile("photo.jpg"))
    .Opacity(90.0)
    .Brightness(10.0)
    .Contrast(20.0);
```

---

## Маски

### ClipNode

Контейнер-маска. Обрезает содержимое по форме маски.

#### Прямоугольная маска (по умолчанию)

```csharp
var clip = UI.Clip()
    .Add(UI.Image(UI.ImageFromFile("photo.jpg")));

// Обрезает по прямоугольнику (эквивалентно отсутствию маски)
```

#### Круговая маска

```csharp
var circleClip = UI.ClipCircle()
    .Add(UI.Image(UI.ImageFromFile("avatar.png")));

// Обрезает изображение в круг
```

#### Эллиптическая маска

```csharp
var ellipseClip = UI.ClipEllipse()
    .Add(UI.Image(UI.ImageFromFile("photo.jpg")));

// Обрезает в эллипс
```

#### Скруглённые углы

```csharp
var roundedClip = UI.ClipRoundedRect(cornerRadius: 8f)
    .Add(UI.Image(UI.ImageFromFile("card.png")));

// Скруглённые углы радиусом 8px
```

#### Произвольная форма

```csharp
var customClip = UI.ClipPath(rect =>
{
    var path = new System.Drawing.Drawing2D.GraphicsPath();
    
    // Звезда
    float centerX = rect.Width / 2;
    float centerY = rect.Height / 2;
    float outerRadius = Math.Min(rect.Width, rect.Height) / 2;
    float innerRadius = outerRadius / 2;
    
    for (int i = 0; i < 10; i++)
    {
        float angle = (float)(i * Math.PI / 5);
        float radius = (i % 2 == 0) ? outerRadius : innerRadius;
        float x = centerX + radius * (float)Math.Cos(angle);
        float y = centerY + radius * (float)Math.Sin(angle);
        
        if (i == 0)
            path.AddLine(x, y, x, y);
        else
            path.AddLine(x, y, x, y);
    }
    path.CloseFigure();
    
    return new DisplayNodes.Gdi.GdiGraphicsPath(path);
})
.Add(UI.Image(UI.ImageFromFile("star.png")));
```

---

## Полные примеры

### Пример 1: Простая форма с логином

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;

public static LayoutNode BuildLoginForm()
{
    var font = UI.Font("Segoe UI", 14f);
    var whiteBrush = UI.Brush(Color.White);
    var grayBrush = UI.Brush(Color.Gray);
    var greenBrush = UI.Brush(Color.LimeGreen);
    
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
                .FullBrush(Color.FromArgb(0, 120, 215)))
            .Add(UI.Label("Отмена", font, whiteBrush)
                .FullBrush(Color.FromArgb(80, 80, 80))));
}
```

### Пример 2: Карточка пользователя

```csharp
public static LayoutNode BuildUserCard(string name, string role, string avatarPath)
{
    var font = UI.Font("Segoe UI", 14f);
    var whiteBrush = UI.Brush(Color.White);
    var grayBrush = UI.Brush(Color.Gray);
    
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
                    .Add(UI.Label(role, UI.Font("Segoe UI", 12f), grayBrush))))
            .Add(UI.Fixed(0, 8))
            .Add(UI.Label("Email: user@example.com", font, grayBrush))
            .Add(UI.Label("Телефон: +7 (999) 123-45-67", font, grayBrush)));
}
```

### Пример 3: Дашборд с сеткой

```csharp
public static LayoutNode BuildDashboard()
{
    var font = UI.Font("Segoe UI", 14f);
    var whiteBrush = UI.Brush(Color.White);
    
    var grid = UI.Grid()
        .Padding(10);
    
    // 3 строки, 2 колонки
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star(1)));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
    
    // Заполнение карточками
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
    var whiteBrush = UI.Brush(Color.White);
    
    return UI.Overlay()
        .Add(UI.Background(accentColor))
        .Add(UI.Column(8)
            .Padding(16)
            .Add(UI.Label(title, titleFont, whiteBrush))
            .Add(UI.Label(value, valueFont, whiteBrush)));
}
```

### Пример 4: Реактивный счётчик

```csharp
public static LayoutNode BuildCounter()
{
    var counter = new Observable<int>(0);
    var font = UI.Font("Segoe UI", 48f, bold: true);
    var whiteBrush = UI.Brush(Color.White);
    
    var counterLabel = UI.Label("0", font, whiteBrush)
        .BindText(new Observable<string>("0"));
    
    // Подписка на изменения счётчика
    counter.Subscribe(value =>
    {
        // В реальном приложении здесь было бы обновление Observable<string>
        // Для примера просто выводим в консоль
        Console.WriteLine($"Counter: {value}");
    });
    
    return UI.Column(20)
        .Padding(40)
        .MainAlignment(MainAxisAlignment.Center)
        .Add(counterLabel)
        .Add(UI.Row(20)
            .MainAlignment(MainAxisAlignment.Center)
            .Add(UI.Label("- decrement", UI.Font("Segoe UI", 16f), whiteBrush)
                .FullBrush(Color.Red))
            .Add(UI.Label("+ increment", UI.Font("Segoe UI", 16f), whiteBrush)
                .FullBrush(Color.Green)));
}
```

### Пример 5: Галерея изображений

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

---

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
var displayRoot = new DisplayRoot(new RenderRootFactory(parent, timers));
displayRoot.Build(rootNode, location, size);
// ... использование ...
displayRoot.Dispose();  // освободит все компоненты

// ✅ ПРАВИЛЬНО: ручной Dispose дерева
rootNode.Dispose();  // освободит все компоненты в дереве

// ❌ НЕПРАВИЛЬНО: забытый Dispose
var displayRoot = new DisplayRoot(factory);
displayRoot.Build(rootNode, location, size);
// забыли вызвать Dispose — утечка ресурсов
```

### 4. Observable и подписки

```csharp
// ✅ ПРАВИЛЬНО: привязка через fluent API (автоматическая отписка)
var observable = new Observable<string>("value");
var label = UI.Label("text", font, brush)
    .BindText(observable);
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

### 7. Выравнивание в контейнерах

```csharp
// ✅ ПРАВИЛЬНО: выравнивание через свойства узла
var label = UI.Label("Text", font, brush)
    .HAlignment(Alignment.Center)
    .VAlignment(Alignment.Center);

// ✅ ПРАВИЛЬНО: MainAxisAlignment для распределения в стеке
var stack = UI.Column(8)
    .MainAlignment(MainAxisAlignment.SpaceBetween)
    .Add(child1)
    .Add(child2);

// ❌ НЕПРАВИЛЬНО: ручное вычисление позиций
var stack = UI.Column(0)
    .Add(child1.Margin(0, 50, 0, 0))  // хардкод отступов
    .Add(child2);
```

### 8. Маски и производительность

```csharp
// ✅ ПРАВИЛЬНО: маска создаётся один раз
var clip = UI.ClipCircle()
    .Add(UI.Image(UI.ImageFromFile("avatar.png")));

// ❌ НЕПРАВИЛЬНО: пересоздание маски на каждый кадр
// (в реальном приложении это происходит автоматически, но избегайте
// создания новых ClipNode в цикле Measure/Arrange)
```

### 9. Обработка ошибок компиляции (в Playground)

```csharp
// ✅ ПРАВИЛЬНО: проверка результата компиляции
var result = _vm.Run(code);
if (!result.Success)
{
    // показать ошибки пользователю
    ShowErrors(result.ErrorOutput);
    return;
}
// использовать result.Root

// ❌ НЕПРАВИЛЬНО: игнорирование ошибок
var result = _vm.Run(code);
var root = result.Root;  // может быть null!
```

### 10. Потокобезопасность

```csharp
// ✅ ПРАВИЛЬНО: изменение Observable в UI-потоке
Dispatcher.Invoke(() =>
{
    counter.Value++;
});

// ✅ ПРАВИЛЬНО: Observable потокобезопасен (использует lock)
var counter = new Observable<int>(0);
Task.Run(() => counter.Value = 10);  // безопасно

// ❌ НЕПРАВИЛЬНО: изменение UI-компонентов из фонового потока
Task.Run(() =>
{
    label.Component.Text = "New text";  // исключение!
});
```

---

## Лицензия

Copyright © 2026 AES