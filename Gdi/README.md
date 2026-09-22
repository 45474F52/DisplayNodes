# DisplayNodes.Gdi

GDI+–реализация абстрактных интерфейсов рендеринга из `DisplayNodes.Core.Rendering`. Предоставляет конкретные обёртки над `System.Drawing` для шрифтов, кистей, изображений и измерения текста.

## Структура

```
Gdi/
├── GdiBrush.cs            # Обёртка над SolidBrush
├── GdiBrushFactory.cs     # Фабрика кистей
├── GdiFont.cs             # Обёртка над Font
├── GdiFontFactory.cs      # Фабрика шрифтов
├── GdiGraphicsPath.cs     # Обёртка над GraphicsPath
├── GdiImage.cs            # Обёртка над Bitmap
├── GdiImageFactory.cs     # Фабрика изображений
├── GdiTextFormat.cs       # Обёртка над StringFormat
├── GdiTextMeasurer.cs     # Измеритель текста
└── GdiConversions.cs      # Методы конвертации между Core и GDI
```

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

## Фабрики

### GdiBrushFactory
Создаёт сплошные кисти из абстрактного цвета `Core.Color`.

```csharp
public IBrush CreateSolidBrush(Color color)
{
    var gdiColor = Color.FromArgb(color.A, color.R, color.G, color.B);
    return new GdiBrush(new SolidBrush(gdiColor));
}
```

### GdiFontFactory
Создаёт шрифты из семейства, размера и стилей.

```csharp
public IFont Create(string family, float size, bool bold = false, bool italic = false)
{
    FontStyle style = FontStyle.Regular;
    if (bold) style |= FontStyle.Bold;
    if (italic) style |= FontStyle.Italic;
    return new GdiFont(new Font(family, size, style, GraphicsUnit.Point));
}
```

**Важно:** Размер указывается в пунктах (`GraphicsUnit.Point`), как в WPF/Flutter.

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

**Особенность:** При создании из потока/байт изображение клонируется (`new Bitmap(safetyCopy)`), чтобы освободить исходный поток. Это защищает от блокировки файла и позволяет закрыть `Stream` сразу после создания.

## GdiTextMeasurer

Измеряет размеры текста с помощью GDI+. Использует кэшированные `Bitmap` и `Graphics` для производительности.

```csharp
public sealed class GdiTextMeasurer : ITextMeasurer
{
    private const float WIDTH_OFFSET = 5f;
    private const float HEIGHT_OFFSET = 1f;
    
    [ThreadStatic] private static Bitmap _bmp;
    [ThreadStatic] private static Graphics _g;
    
    private static Graphics GetGraphics()
    {
        if (_g == null)
        {
            _bmp = new Bitmap(1, 1);
            _g = Graphics.FromImage(_bmp);
        }
        return _g;
    }
    
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
- `[ThreadStatic]` — каждый поток имеет свой `Graphics`, что снижает нагрузку на GC
- `WIDTH_OFFSET` и `HEIGHT_OFFSET` — компенсация погрешности `MeasureString` (GDI+ добавляет небольшой padding)
- Если `maxWidth > 0`, текст измеряется с переносом строк
- Если `maxWidth == 0`, текст измеряется в одну строку без ограничений

## GdiConversions

Методы расширения для конвертации между абстрактными типами `Core` и GDI-типами.

### Извлечение внутреннего объекта
```csharp
// Извлечь GDI Font из IFont
Font gdiFont = font.ToGdi();

// Извлечь GDI SolidBrush из IBrush
SolidBrush gdiBrush = brush.ToGdi();

// Извлечь GDI Bitmap из IImage
Bitmap gdiBitmap = image.ToGdi();

// Извлечь GDI StringFormat из ITextFormat
StringFormat gdiFormat = format.ToGdi();

// Извлечь GDI GraphicsPath из IGraphicsPath
GraphicsPath gdiPath = path.ToGdi();
```

**Важно:** Методы возвращают `null`, если объект не является GDI-обёрткой. Это позволяет безопасно работать с разными бэкендами.

### Обёртывание GDI-объектов
```csharp
// Обернуть Font в IFont
IFont wrappedFont = font.Wrap();

// Обернуть SolidBrush в IBrush
IBrush wrappedBrush = brush.Wrap();

// Обернуть Bitmap в IImage
IImage wrappedImage = bitmap.Wrap();

// Обернуть StringFormat в ITextFormat
ITextFormat wrappedFormat = format.Wrap();

// Обернуть GraphicsPath в IGraphicsPath
IGraphicsPath wrappedPath = path.Wrap();
```

### Конвертация цветов
```csharp
// Core.Color → System.Drawing.Color
System.Drawing.Color gdiColor = coreColor.ToGdi();

// System.Drawing.Color → Core.Color
Core.Color coreColor = gdiColor.FromGdi();
```

### Конвертация прямоугольников
```csharp
// Core.Rect → System.Drawing.RectangleF
RectangleF gdiRect = rect.ToGdi();

// System.Drawing.RectangleF → Core.Rect
Core.Rect coreRect = gdiRect.FromGdi();
```

**Особенность:** `FromGdi` использует `Convert.ToInt32`, что округляет значения. Это может привести к потере точности на 1 пиксель.

## Использование

### Инициализация (один раз)
```csharp
// В Program.cs или точке входа
UI.Factory = new WidgetFactory();  // например, из WinFormsAdapter
UI.Measurer = new GdiTextMeasurer();
UI.BrushFactory = new GdiBrushFactory();
UI.FontFactory = new GdiFontFactory();
UI.ImageFactory = new GdiImageFactory();
```

### Создание ресурсов
```csharp
// Шрифт
IFont font = UI.Font("Segoe UI", 14f, bold: true);

// Кисть
IBrush brush = UI.Brush(Color.White);

// Изображение
IImage image = UI.ImageFromFile("icon.png");

// Измерение текста
Size textSize = UI.Measurer.MeasureArea("Hello", font, maxWidth: 200);
```

### Работа с внутренними объектами
```csharp
// Получить GDI Font из IFont
Font gdiFont = font.ToGdi();
if (gdiFont != null)
{
    // Использовать gdiFont напрямую
    float size = gdiFont.SizeInPoints;
}

// Обернуть GDI Bitmap в IImage
Bitmap bitmap = new Bitmap(100, 100);
IImage wrapped = bitmap.Wrap();
```

## Управление ресурсами

### Кто владеет ресурсами?

**Фабрики** создают новые объекты — вызывающий код владеет ими и должен диспоузить.

**Адаптеры** клонируют ресурсы при установке, чтобы защитить consumer-код от неожиданного освобождения.

Пример:
```csharp
// Consumer-код
using (Font myFont = new Font("Arial", 12f))
{
    IFont wrapped = myFont.Wrap();
    label.Font = wrapped;  // Адаптер клонирует myFont
    
    // myFont всё ещё жив и может использоваться
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

## Ограничения

1. **Только сплошные кисти** — `GdiBrushFactory` создаёт только `SolidBrush`. Градиенты и текстуры не поддерживаются.

2. **Только растровые изображения** — `GdiImage` работает только с `Bitmap`. Векторная графика (EMF, WMF) не поддерживается.

3. **Округление при конвертации** — `RectangleF.FromGdi()` может терять точность на 1 пиксель.

4. **Потокобезопасность `GdiTextMeasurer`** — `[ThreadStatic]` переменные не диспоузитcя явно. При интенсивном использовании пула потоков возможна утечка GDI-handles.

## Расширение

### Создание своей фабрики

```csharp
public class CustomBrushFactory : IBrushFactory
{
    public IBrush CreateSolidBrush(Color color)
    {
        // Ваша логика создания кисти
        return new MyCustomBrush(color);
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

## Зависимости

- `DisplayNodes.Core` — абстрактные интерфейсы и типы
- `System.Drawing` — GDI+ (только Windows)
