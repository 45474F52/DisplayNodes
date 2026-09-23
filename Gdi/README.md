# DisplayNodes.Gdi

GDI+–реализация абстрактных интерфейсов рендеринга из `DisplayNodes.Core.Rendering`. Предоставляет конкретные обёртки над `System.Drawing` для шрифтов, кистей, изображений и измерения текста.

## Содержание

1. [Структура](#structure)
2. [Назначение](#purpose)
3. [Обёртки](#wrappers)
4. [Фабрики](#factories)
5. [Градиенты](#gradients)
6. [GdiTextMeasurer](#measurer)
7. [GdiConversions](#conversions)
8. [Использование](#usage)
9. [Управление ресурсами](#resources)
10. [Ограничения](#limitations)
11. [Расширение](#extension)

---

<a id="structure"></a>
## Структура

```
Gdi/
├── GdiBrush.cs                  # Обёртка над SolidBrush
├── GdiBrushFactory.cs           # Фабрика кистей
├── GdiFont.cs                   # Обёртка над Font
├── GdiFontFactory.cs            # Фабрика шрифтов
├── GdiGraphicsPath.cs           # Обёртка над GraphicsPath
├── GdiImage.cs                  # Обёртка над Bitmap
├── GdiImageFactory.cs           # Фабрика изображений
├── GdiTextFormat.cs             # Обёртка над StringFormat
├── GdiTextMeasurer.cs           # Измеритель текста
├── GdiLinearGradientBrush.cs    # Описание линейного градиента
├── GdiRadialGradientBrush.cs    # Описание радиального градиента
└── GdiConversions.cs            # Методы конвертации между Core и GDI
```

---

<a id="purpose"></a>
## Назначение

Модуль реализует бэкенд-агностичные интерфейсы из `Core.Rendering` с помощью `System.Drawing`:

| Интерфейс | Реализация |
|---|---|
| `IBrush` | `GdiBrush` (обёртка над `SolidBrush`) |
| `IFont` | `GdiFont` (обёртка над `Font`) |
| `IImage` | `GdiImage` (обёртка над `Bitmap`) |
| `ITextFormat` | `GdiTextFormat` (обёртка над `StringFormat`) |
| `IGraphicsPath` | `GdiGraphicsPath` (обёртка над `GraphicsPath`) |
| `IBrushFactory` | `GdiBrushFactory` |
| `IFontFactory` | `GdiFontFactory` |
| `IImageFactory` | `GdiImageFactory` |
| `ITextMeasurer` | `GdiTextMeasurer` |

---

<a id="wrappers"></a>
## Обёртки

Все классы-обёртки хранят внутренний объект GDI+ в свойстве `Inner` и реализуют соответствующий интерфейс из `Core.Rendering`.

### GdiBrush

```csharp
public sealed class GdiBrush : IBrush
{
    public SolidBrush Inner { get; }

    public GdiBrush(SolidBrush brush)
    {
        Inner = brush ?? throw new ArgumentNullException(nameof(brush));
    }
}
```

### GdiFont

```csharp
public sealed class GdiFont : IFont
{
    public Font Inner { get; }

    public GdiFont(Font font)
    {
        Inner = font ?? throw new ArgumentNullException(nameof(font));
    }
}
```

### GdiImage

```csharp
public sealed class GdiImage : IImage
{
    public Bitmap Inner { get; }
    public int Width => Inner?.Width ?? 0;
    public int Height => Inner?.Height ?? 0;

    public GdiImage(Bitmap bitmap)
    {
        Inner = bitmap;
    }
}
```

**Особенность:** `Width` и `Height` возвращают `0`, если `Inner == null`. Это позволяет создавать пустые изображения-заглушки.

### GdiTextFormat

```csharp
public sealed class GdiTextFormat : ITextFormat
{
    public StringFormat Inner { get; }

    public GdiTextFormat(StringFormat format)
    {
        Inner = format ?? throw new ArgumentNullException(nameof(format));
    }
}
```

### GdiGraphicsPath

```csharp
public sealed class GdiGraphicsPath : IGraphicsPath
{
    public GraphicsPath Inner { get; }

    public GdiGraphicsPath(GraphicsPath path)
    {
        Inner = path ?? throw new ArgumentNullException(nameof(path));
    }
}
```

---

<a id="factories"></a>
## Фабрики

### GdiBrushFactory

Создаёт сплошные и градиентные кисти.

```csharp
public IBrush CreateSolidBrush(Color color)
{
    var gdiColor = Color.FromArgb(color.A, color.R, color.G, color.B);
    return new GdiBrush(new SolidBrush(gdiColor));
}

public IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops)
{
    ValidateStops(stops);
    return new GdiLinearGradientBrush(start, end, (GradientStop[])stops.Clone());
}

public IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops)
{
    ValidateStops(stops);
    return new GdiRadialGradientBrush(center, radius, (GradientStop[])stops.Clone());
}
```

**Валидация `stops`:** null → `ArgumentNullException`, меньше двух → `ArgumentException`.

### GdiFontFactory

Создаёт шрифты.

```csharp
public IFont Create(string family, float size, bool bold = false, bool italic = false)
{
    FontStyle style = FontStyle.Regular;
    if (bold) style |= FontStyle.Bold;
    if (italic) style |= FontStyle.Italic;
    return new GdiFont(new Font(family, size, style, GraphicsUnit.Point));
}
```

**Важно:** размер указывается в пунктах (`GraphicsUnit.Point`), как в WPF/Flutter.

### GdiImageFactory

Создаёт изображения из файла, потока или массива байт.

```csharp
public IImage CreateFromFile(string path)
{
    return new GdiImage(new Bitmap(path));
}

public IImage CreateFromStream(Stream stream)
{
    using (Bitmap safetyCopy = new Bitmap(stream))
    {
        return new GdiImage(new Bitmap(safetyCopy));
    }
}

public IImage CreateFromBytes(byte[] bytes)
{
    using (MemoryStream ms = new MemoryStream(bytes))
    {
        using (Bitmap safetyCopy = new Bitmap(ms))
        {
            return new GdiImage(new Bitmap(safetyCopy));
        }
    }
}
```

**Особенность:** при создании из потока/байт изображение клонируется (`new Bitmap(safetyCopy)`), чтобы освободить исходный поток. Это защищает от блокировки файла и позволяет закрыть `Stream` сразу после создания.

---

<a id="gradients"></a>
## Градиенты

### GdiLinearGradientBrush

Описание линейного градиента. Хранит нормализованные координаты (0..100) и стопы.

```csharp
public sealed class GdiLinearGradientBrush : IBrush
{
    public Point Start { get; }
    public Point End { get; }
    public GradientStop[] Stops { get; }

    public GdiLinearGradientBrush(Point start, Point end, GradientStop[] stops)
    {
        Start = start;
        End = end;
        Stops = stops;
    }
}
```

### GdiRadialGradientBrush

```csharp
public sealed class GdiRadialGradientBrush : IBrush
{
    public Point Center { get; }
    public Percent Radius { get; }
    public GradientStop[] Stops { get; }

    public GdiRadialGradientBrush(Point center, Percent radius, GradientStop[] stops)
    {
        Center = center;
        Radius = radius;
        Stops = stops;
    }
}
```

### Создание конкретной GDI-кисти

GDI+ не поддерживает относительные координаты, поэтому конкретная кисть создаётся под заданный прямоугольник:

```csharp
public static LinearGradientBrush CreateGdiBrush(this GdiLinearGradientBrush brush, RectangleF bounds)
{
    float x1 = bounds.X + bounds.Width * brush.Start.X / 100f;
    float y1 = bounds.Y + bounds.Height * brush.Start.Y / 100f;
    float x2 = bounds.X + bounds.Width * brush.End.X / 100f;
    float y2 = bounds.Y + bounds.Height * brush.End.Y / 100f;

    var gdiBrush = new LinearGradientBrush(
        new PointF(x1, y1),
        new PointF(x2, y2),
        brush.Stops[0].Color.ToGdi(),
        brush.Stops[brush.Stops.Length - 1].Color.ToGdi());

    var blend = new ColorBlend(brush.Stops.Length)
    {
        Positions = new float[brush.Stops.Length],
        Colors = new System.Drawing.Color[brush.Stops.Length]
    };

    for (int i = 0; i < brush.Stops.Length; i++)
    {
        blend.Positions[i] = brush.Stops[i].Offset.Value / 100f;
        blend.Colors[i] = brush.Stops[i].Color.ToGdi();
    }

    gdiBrush.InterpolationColors = blend;

    return gdiBrush;
}
```

**Аналогично для `GdiRadialGradientBrush`** — через `PathGradientBrush`.

**Ограничение:** `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException` для градиентных кистей — адаптеры не поддерживают градиенты.

---

<a id="measurer"></a>
## GdiTextMeasurer

Измеряет размеры текста с помощью GDI+. Использует кэшированные `Bitmap` и `Graphics` для производительности.

```csharp
public sealed class GdiTextMeasurer : ITextMeasurer
{
    private const float WIDTH_OFFSET = 5f;
    private const float HEIGHT_OFFSET = 1f;

    [ThreadStatic] private static Bitmap _bmp;
    [ThreadStatic] private static Graphics _g;

    public Size MeasureArea(string text, IFont font, int maxWidth = 0)
    {
        if (string.IsNullOrEmpty(text))
            return Size.Empty;

        var gdiFont = (font as GdiFont)?.Inner;
        if (gdiFont == null)
            return Size.Empty;

        var g = GetGraphics();
        using (var sf = new StringFormat())
        {
            var size = maxWidth > 0
                ? g.MeasureString(text, gdiFont, maxWidth, sf)
                : g.MeasureString(text, gdiFont, int.MaxValue, sf);

            return new Size(
                (int)Math.Ceiling(size.Width + WIDTH_OFFSET),
                (int)Math.Ceiling(size.Height + HEIGHT_OFFSET));
        }
    }
}
```

**Особенности:**
- `[ThreadStatic]` — каждый поток имеет свой `Graphics`, что снижает нагрузку на GC.
- `WIDTH_OFFSET` и `HEIGHT_OFFSET` — компенсация погрешности `MeasureString`.
- Если `maxWidth > 0`, текст измеряется с переносом строк.
- Если `maxWidth == 0`, текст измеряется в одну строку без ограничений.

---

<a id="conversions"></a>
## GdiConversions

Методы расширения для конвертации между абстрактными типами `Core` и GDI-типами.

### Извлечение внутреннего объекта

```csharp
Font gdiFont = font.ToGdi();
SolidBrush gdiBrush = brush.ToGdi();
Bitmap gdiBitmap = image.ToGdi();
StringFormat gdiFormat = format.ToGdi();
GraphicsPath gdiPath = path.ToGdi();
```

### Оборачивание GDI-объектов

```csharp
IFont wrappedFont = font.Wrap();
IBrush wrappedBrush = brush.Wrap();
IImage wrappedImage = bitmap.Wrap();
ITextFormat wrappedFormat = format.Wrap();
IGraphicsPath wrappedPath = path.Wrap();
```

### Конвертация цветов

```csharp
System.Drawing.Color gdiColor = coreColor.ToGdi();
Core.Color coreColor = gdiColor.FromGdi();
```

### Конвертация прямоугольников

```csharp
RectangleF gdiRect = rect.ToGdi();
Core.Rect coreRect = gdiRect.FromGdi();
```

### ToGdi(IBrush) — NotSupportedException

```csharp
public static SolidBrush ToGdi(this IBrush brush)
{
    if (brush == null)
        return null;

    if (brush is GdiBrush solid)
        return solid.Inner;

    throw new NotSupportedException(
        "Brush type '" + brush.GetType().Name + "' is not supported by this adapter. " +
        "Only GdiBrush (solid) is supported at the moment.");
}
```

**Почему:** GDI+ кисть для `Label2D.Brush` — `SolidBrush`, не `Brush`. Градиенты требуют кастомной отрисовки через `Graphics.FillRectangle`.

---

<a id="usage"></a>
## Использование

### Инициализация (один раз)

```csharp
UI.Factory = new WidgetFactory();  // например, из WinFormsAdapter
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();
UI.ImageFactory = new GdiImageFactory();
```

### Создание ресурсов

```csharp
IFont font = UI.Font("Segoe UI", 14f, bold: true);
IBrush brush = UI.SolidBrush(Color.White);
IImage image = UI.ImageFromFile("icon.png");
Size textSize = UI.Measurer.MeasureArea("Hello", font, maxWidth: 200);
```

### Работа с внутренними объектами

```csharp
Font gdiFont = font.ToGdi();
if (gdiFont != null)
{
    float size = gdiFont.SizeInPoints;
}

Bitmap bitmap = new Bitmap(100, 100);
IImage wrapped = bitmap.Wrap();
```

---

<a id="resources"></a>
## Управление ресурсами

### Кто владеет ресурсами?

**Фабрики** создают новые объекты — вызывающий код владеет ими и должен диспоузить.

**Адаптеры** клонируют ресурсы при установке, чтобы защитить consumer-код от неожиданного освобождения.

```csharp
// Consumer-код
using (Font myFont = new Font("Arial", 12f))
{
    IFont wrapped = myFont.Wrap();
    label.Font = wrapped;  // Адаптер клонирует myFont

    // myFont всё ещё жив
    // Адаптер владеет своей копией и диспоузит её при Dispose
}
```

### Dispose

GDI-обёртки **не реализуют `IDisposable`** — они не владеют внутренними объектами. Ресурсы освобождаются адаптерами или consumer-кодом.

```csharp
// Неправильно: GdiBrush не диспоузит Inner
var brush = new GdiBrush(new SolidBrush(Color.Red));
brush = null;  // SolidBrush не освобождён!

// Правильно: consumer-код владеет ресурсом
using (var solidBrush = new SolidBrush(Color.Red))
{
    var brush = new GdiBrush(solidBrush);
    // Использовать brush
}  // solidBrush освобождён
```

---

<a id="limitations"></a>
## Ограничения

1. **Только сплошные кисти** в `GdiConversions.ToGdi(IBrush)` — градиенты описаны в `GdiLinearGradientBrush` / `GdiRadialGradientBrush`, но для установки в компонент нужен адаптер с кастомной отрисовкой.

2. **Только растровые изображения** — `GdiImage` работает только с `Bitmap`. Векторная графика (EMF, WMF) не поддерживается.

3. **Округление при конвертации** — `RectangleF.FromGdi()` может терять точность на 1 пиксель.

4. **Потокобезопасность `GdiTextMeasurer`** — `[ThreadStatic]` переменные не диспоузятся явно. При интенсивном использовании пула потоков возможна утечка GDI-handles.

---

<a id="extension"></a>
## Расширение

### Создание своей фабрики

```csharp
public class CustomBrushFactory : IBrushFactory
{
    public IBrush CreateSolidBrush(Color color)
    {
        return new MyCustomBrush(color);
    }

    public IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops)
    {
        // Ваша логика
    }

    public IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops)
    {
        // Ваша логика
    }
}
```

### Создание своей обёртки

```csharp
public class CustomFont : IFont
{
    public MyFont Inner { get; }

    public CustomFont(MyFont font)
    {
        Inner = font ?? throw new ArgumentNullException(nameof(font));
    }
}
```

### Добавление методов конвертации

```csharp
public static class MyConversions
{
    public static MyType ToMyType(this IMyInterface obj)
    {
        return (obj as MyWrapper)?.Inner;
    }

    public static IMyInterface Wrap(this MyType obj)
    {
        return new MyWrapper(obj);
    }
}
```

---

## Зависимости

- `DisplayNodes.Core` — абстрактные интерфейсы и типы
- `System.Drawing` — GDI+ (только Windows)