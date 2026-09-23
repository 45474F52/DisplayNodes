# Создание адаптера для DisplayNodes

Руководство по созданию собственного бэкенда рендеринга для DisplayNodes.

## Содержание

1. [Архитектура адаптера](#architecture)
2. [Шаг 1: Точка входа](#step-1)
3. [Шаг 2: ComponentBase](#step-2)
4. [Шаг 3: Label](#step-3)
5. [Шаг 4: Image](#step-4)
6. [Шаг 5: Маски](#step-5)
7. [Шаг 6: WidgetFactory](#step-6)
8. [Шаг 7: Корневой контейнер](#step-7)
9. [Шаг 8: Конвертеры](#step-8)
10. [Ограничения адаптеров](#limitations)
11. [Управление ресурсами](#resources)
12. [Пример использования](#usage)
13. [Чек-лист](#checklist)

---

<a id="architecture"></a>
## Архитектура адаптера

Адаптер состоит из следующих компонентов:

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

---

<a id="step-1"></a>
## Шаг 1: Точка входа (Adapter.cs)

Статический класс для инициализации фабрик. Вызывается **один раз** при старте приложения.

```csharp
using DisplayNodes.Fluent;
using DisplayNodes.Gdi;

namespace YourNamespace
{
    public static class Adapter
    {
        public static void Initialize()
        {
            UI.Factory = new WidgetFactory();
            UI.Measurer = new GdiTextMeasurer();  // или свой измеритель
            UI.BrushFactory = new GdiBrushFactory();
            UI.FontFactory = new GdiFontFactory();
            UI.ImageFactory = new GdiImageFactory();
        }
    }
}
```

**Важно:** `UI.Measurer`, `UI.BrushFactory`, `UI.FontFactory`, `UI.ImageFactory` можно переиспользовать из `DisplayNodes.Gdi`, если ваш бэкенд работает с GDI+ ресурсами.

---

<a id="step-2"></a>
## Шаг 2: Базовый класс компонентов (ComponentBase.cs)

Маппит абстрактный `IRenderComponent` на ваш внутренний тип.

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace YourNamespace.Components
{
    internal abstract class ComponentBase : IRenderComponent
    {
        // Ваш внутренний тип (Control, IComponent, и т.д.)
        public YourInnerType Inner { get; }

        private IRenderComponent _parentAdapter;

        protected ComponentBase(YourInnerType inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IRenderComponent Parent
        {
            get => _parentAdapter;
            set
            {
                _parentAdapter = value;
                // Извлечь внутренний тип из родительского адаптера
                Inner.Parent = ComponentHelper.ExtractInner(value);
            }
        }

        public Point Location
        {
            get => new Point(Inner.Location.X, Inner.Location.Y);
            set => Inner.Location = new YourPointType(value.X, value.Y);
        }

        public Size Size
        {
            get => new Size(Inner.Size.Width, Inner.Size.Height);
            set => Inner.Size = new YourSizeType(value.Width, value.Height);
        }

        public bool Visible
        {
            get => Inner.Visible;
            set => Inner.Visible = value;
        }
    }

    // Хелпер для извлечения внутреннего типа
    internal static class ComponentHelper
    {
        public static YourInnerType ExtractInner(IRenderComponent component)
        {
            if (component is ComponentBase c)
                return c.Inner;
            if (component is MaskBase m)
                return m.Inner;  // если маски наследуются от ComponentBase
            return null;
        }
    }
}
```

---

<a id="step-3"></a>
## Шаг 3: Текстовая метка (Label.cs)

Реализует `ILabelComponent` и опционально `ITextLayoutComponent`, `IEffectComponent`.

```csharp
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;

namespace YourNamespace.Components
{
    internal sealed class Label : ComponentBase, ILabelComponent, IDisposable
    {
        private YourLabelType _label;

        // Владеет клонами ресурсов
        private YourFontType _ownedFont;
        private YourBrushType _ownedForeground;
        private YourBrushType _ownedBackground;
        private YourFormatType _ownedFormat;

        private Shadow? _shadow;

        public Label() : base(new YourLabelType())
        {
            _label = (YourLabelType)Inner;
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public IFont Font
        {
            get => new GdiFont(_label.Font);  // или ваш wrapper
            set
            {
                _ownedFont?.Dispose();
                var gdi = value.ToGdi();
                _ownedFont = gdi != null ? (YourFontType)gdi.Clone() : null;
                _label.Font = _ownedFont;
            }
        }

        public IBrush ForegroundBrush
        {
            get => new GdiBrush(_label.ForegroundBrush);
            set
            {
                _ownedForeground?.Dispose();
                var gdi = value.ToGdi();
                _ownedForeground = gdi != null ? (YourBrushType)gdi.Clone() : null;
                _label.ForegroundBrush = _ownedForeground;
            }
        }

        public IBrush BackgroundBrush
        {
            get => new GdiBrush(_label.BackgroundBrush);
            set
            {
                _ownedBackground?.Dispose();
                var gdi = value.ToGdi();
                _ownedBackground = gdi != null ? (YourBrushType)gdi.Clone() : null;
                _label.BackgroundBrush = _ownedBackground;
            }
        }

        public ITextFormat Format
        {
            get => new GdiTextFormat(_label.Format);
            set
            {
                _ownedFormat?.Dispose();
                var gdi = value.ToGdi();
                _ownedFormat = gdi != null ? (YourFormatType)gdi.Clone() : null;
                _label.Format = _ownedFormat;
            }
        }

        // Опционально: ITextLayoutComponent
        public TextDrawMethod DrawMethod { get; set; }
        public LabelStretch Stretch { get; set; }

        // Опционально: IEffectComponent
        public double Opacity { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }

        public Shadow? Shadow
        {
            get => _shadow;
            set
            {
                if (value.HasValue)
                    throw new NotSupportedException(
                        "Shadow is not supported by this adapter yet.");
                _shadow = null;
            }
        }

        public void Dispose()
        {
            _ownedFont?.Dispose();
            _ownedForeground?.Dispose();
            _ownedBackground?.Dispose();
            _ownedFormat?.Dispose();
            _label.Dispose();
        }
    }
}
```

**Критически важно:** адаптер **клонирует** ресурсы (Font, Brush, StringFormat) при установке, чтобы consumer-код сохранял владение оригиналами. Иначе Dispose адаптера освободит ресурсы, которые ещё используются.

---

<a id="step-4"></a>
## Шаг 4: Изображение (Image.cs)

Реализует `IImageComponent` и опционально `IEffectComponent`.

```csharp
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;

namespace YourNamespace.Components
{
    internal sealed class Image : ComponentBase, IImageComponent, IDisposable
    {
        private YourImageType _image;
        private YourBitmapType _ownedBitmap;

        private Shadow? _shadow;

        public Image() : base(new YourImageType())
        {
            _image = (YourImageType)Inner;
        }

        public IImage ImageData
        {
            get => _image.Bitmap != null ? new GdiImage(_image.Bitmap) : null;
            set
            {
                _ownedBitmap?.Dispose();
                var gdi = value.ToGdi();
                _ownedBitmap = gdi != null ? (YourBitmapType)gdi.Clone() : null;
                _image.Bitmap = _ownedBitmap;
            }
        }

        IImage IImageComponent.Image
        {
            get => ImageData;
            set => ImageData = value;
        }

        public ImageSizeMode SizeMode
        {
            get => /* конвертировать из вашего типа */;
            set => /* конвертировать в ваш тип */;
        }

        // Опционально: IEffectComponent
        public double Opacity { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }

        public Shadow? Shadow
        {
            get => _shadow;
            set
            {
                if (value.HasValue)
                    throw new NotSupportedException(
                        "Shadow is not supported by this adapter yet.");
                _shadow = null;
            }
        }

        public void Dispose()
        {
            _ownedBitmap?.Dispose();
            _image.Dispose();
        }
    }
}
```

---

<a id="step-5"></a>
## Шаг 5: Маски (Masks/)

### Базовый класс (MaskBase.cs)

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace YourNamespace.Components.Masks
{
    internal abstract class MaskBase : ComponentBase, IMaskComponent
    {
        protected MaskBase() : base(new YourMaskType()) { }

        // Переопределяется в наследниках для создания формы маски
        protected abstract YourRegionType CreateRegion(YourRectType bounds);

        // Вызывается при отрисовке для применения маски
        public void ApplyMask(YourGraphicsType graphics)
        {
            var bounds = new YourRectType(
                Inner.Location.X, Inner.Location.Y,
                Inner.Size.Width, Inner.Size.Height);

            using (var region = CreateRegion(bounds))
            {
                graphics.Clip = region;
            }
        }
    }
}
```

### Конкретные маски

```csharp
// RectMask.cs
internal sealed class RectMask : MaskBase
{
    protected override YourRegionType CreateRegion(YourRectType bounds)
    {
        return new YourRegionType(bounds);
    }
}

// CircleMask.cs
internal sealed class CircleMask : MaskBase
{
    protected override YourRegionType CreateRegion(YourRectType bounds)
    {
        float diameter = Math.Min(bounds.Width, bounds.Height);
        float x = bounds.X + (bounds.Width - diameter) / 2;
        float y = bounds.Y + (bounds.Height - diameter) / 2;

        using (var path = new YourPathType())
        {
            path.AddEllipse(x, y, diameter, diameter);
            return new YourRegionType(path);
        }
    }
}

// RoundedRectMask.cs
internal sealed class RoundedRectMask : MaskBase
{
    public float CornerRadius { get; }

    public RoundedRectMask(float cornerRadius)
    {
        CornerRadius = cornerRadius;
    }

    protected override YourRegionType CreateRegion(YourRectType bounds)
    {
        using (var path = new YourPathType())
        {
            float r = Math.Min(CornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);
            // Добавить скруглённые углы в path
            path.CloseFigure();
            return new YourRegionType(path);
        }
    }
}

// PathMask.cs
internal sealed class PathMask : MaskBase
{
    public Func<Rect, IGraphicsPath> PathBuilder { get; }

    public PathMask(Func<Rect, IGraphicsPath> pathBuilder)
    {
        PathBuilder = pathBuilder;
    }

    protected override YourRegionType CreateRegion(YourRectType bounds)
    {
        if (PathBuilder == null)
            return new YourRegionType(bounds);

        var rect = new Rect((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
        var abstractPath = PathBuilder(rect);
        var gdiPath = abstractPath.ToGdi();

        return new YourRegionType(gdiPath);
    }
}
```

---

<a id="step-6"></a>
## Шаг 6: Фабрика виджетов (WidgetFactory.cs)

Создаёт экземпляры компонентов.

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using YourNamespace.Components;
using YourNamespace.Components.Masks;

namespace YourNamespace
{
    internal sealed class WidgetFactory : IWidgetFactory
    {
        public ILabelComponent CreateLabel() => new Label();
        public IImageComponent CreateImage() => new Image();
        public IMaskComponent CreateRectMask() => new RectMask();
        public IMaskComponent CreateCircleMask() => new CircleMask();
        public IMaskComponent CreateEllipseMask() => new EllipseMask();

        public IMaskComponent CreateRoundedRectMask(float cornerRadius)
            => new RoundedRectMask(cornerRadius);

        public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
            => new PathMask(pathBuilder);
    }
}
```

---

<a id="step-7"></a>
## Шаг 7: Корневой контейнер

### RenderRootFactory.cs

```csharp
using DisplayNodes.Core.Rendering;

namespace YourNamespace
{
    public sealed class RenderRootFactory : IRenderRootFactory
    {
        private readonly YourParentType _parent;

        public RenderRootFactory(YourParentType parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public IRenderRoot Create() => new RenderRoot(_parent);
    }
}
```

### RenderRoot.cs

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using YourNamespace.Components;

namespace YourNamespace
{
    internal sealed class RenderRoot : IRenderRoot
    {
        private readonly YourParentType _parent;
        private Layout _rootAdapter;

        public RenderRoot(YourParentType parent)
        {
            _parent = parent;
        }

        public ILayoutComponent Root => _rootAdapter;

        public void Build(LayoutNode node, Point location, Size size)
        {
            Clear();

            var layout = new YourLayoutType(
                _parent,
                new YourPointType(location.X, location.Y),
                new YourSizeType(size.Width, size.Height));

            _rootAdapter = new Layout(layout);
            node.Apply(_rootAdapter, location, size);
            _parent.Add(layout);
        }

        public void Clear()
        {
            if (_rootAdapter == null)
                return;

            _parent.Remove(_rootAdapter.Inner);
            _rootAdapter.Dispose();
            _rootAdapter = null;
        }

        public void Dispose() => Clear();
    }
}
```

### Layout.cs

```csharp
using DisplayNodes.Core.Rendering;

namespace YourNamespace.Components
{
    internal sealed class Layout : ComponentBase, ILayoutComponent, IDisposable
    {
        private YourLayoutType _layout;

        public Layout(YourLayoutType layout) : base(layout)
        {
            _layout = layout;
        }

        public void Refresh() => _layout.Refresh();

        public void Dispose() => _layout.Dispose();
    }
}
```

---

<a id="step-8"></a>
## Шаг 8: Конвертеры (опционально)

Если ваш бэкенд использует GDI+ типы, добавьте методы расширения:

```csharp
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Gdi;

namespace YourNamespace
{
    public static class Conversions
    {
        // Извлечение вашего типа из абстрактного интерфейса
        public static YourFontType ToYourType(this IFont font)
        {
            return (font as GdiFont)?.Inner as YourFontType;
        }

        // Оборачивание вашего типа в абстрактный интерфейс
        public static IFont Wrap(this YourFontType font)
        {
            return new GdiFont(font);
        }

        // Аналогично для Brush, Image, и т.д.
    }
}
```

---

<a id="limitations"></a>
## Ограничения адаптеров

Три визуальных эффекта описаны в API ядра, но **не поддерживаются** текущими адаптерами. При попытке использования они бросают `NotSupportedException`.

### Градиентные кисти

```csharp
public interface IBrushFactory
{
    IBrush CreateSolidBrush(Color color);
    IBrush CreateLinearGradient(Point start, Point end, params GradientStop[] stops);
    IBrush CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops);
}
```

**Ограничение:** `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException` для градиентных кистей:

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

**Причина:** GDI+ не поддерживает градиентные кисти напрямую в том виде, в котором они используются компонентами. Градиенты требуют кастомной отрисовки через `Graphics.FillRectangle` с `LinearGradientBrush`/`PathGradientBrush`.

**Реализация:** см. ROADMAP → «Реализация градиентов в адаптерах».

### Shadow

```csharp
public interface IEffectComponent
{
    double Opacity { get; set; }
    double Brightness { get; set; }
    double Contrast { get; set; }
    Shadow? Shadow { get; set; }
}
```

**Ограничение:** адаптер бросает `NotSupportedException` при попытке установить не-null `Shadow`:

```csharp
public Shadow? Shadow
{
    get => _shadow;
    set
    {
        if (value.HasValue)
            throw new NotSupportedException(
                "Shadow is not supported by this adapter yet.");
        _shadow = null;
    }
}
```

**Причина:** GDI+ не имеет встроенной поддержки `DropShadowEffect`. Реализация требует кастомного размытия через `ColorMatrix` + `Bitmap`-манипуляции.

**Реализация:** см. ROADMAP → «Реализация тени (Shadow) в адаптерах».

### TransformNode

**Ограничение:** `ApplyRecursive` бросает `NotSupportedException` при обнаружении `TransformNode`:

```csharp
private static void ApplyRecursive(LayoutNode root, IRenderComponent parent,
                                    ICollection<IRenderComponent> components)
{
    // ...

    if (root is TransformNode)
    {
        throw new NotSupportedException(
            "TransformNode is not supported by current adapters yet. " +
            "The node and its children were laid out, but rendering cannot be performed.");
    }

    // ...
}
```

**Причина:** адаптеры не поддерживают применение `Graphics.Transform` к компонентам. Layout для `TransformNode` работает корректно (вычисляется bounding box), но рендеринг невозможен.

**Реализация:** см. ROADMAP → «Реализация TransformNode в адаптерах».

---

<a id="resources"></a>
## Управление ресурсами

### Кто владеет ресурсами?

**Фабрики** создают новые объекты — вызывающий код владеет ими.

**Адаптеры** клонируют ресурсы при установке, чтобы защитить consumer-код:

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

Адаптер реализует `IDisposable` и освобождает **только свои клоны**:

```csharp
public void Dispose()
{
    _ownedFont?.Dispose();      // клон, созданный адаптером
    _ownedBrush?.Dispose();     // клон, созданный адаптером
    _innerControl.Dispose();    // внутренний контрол
}
```

**Что НЕ нужно диспоузить:**
- Оригинальные ресурсы consumer-кода — они владеют ими.
- Абстрактные интерфейсы (`IFont`, `IBrush`) — у них нет `Dispose`.

### Клонирование при установке

**Правило:** при установке ресурса через свойство адаптер должен:
1. Освободить предыдущий клон (если был).
2. Клонировать новый ресурс.
3. Сохранить клон в приватном поле.
4. Передать клон во внутренний компонент.

```csharp
public IFont Font
{
    get => new GdiFont(_label.Font);
    set
    {
        _ownedFont?.Dispose();              // 1
        var gdi = value.ToGdi();
        _ownedFont = gdi != null            // 2
            ? (Font)gdi.Clone()
            : null;
        _label.Font = _ownedFont;           // 3, 4
    }
}
```

---

<a id="usage"></a>
## Пример использования

```csharp
// 1. Инициализация (один раз)
YourNamespace.Adapter.Initialize();

// 2. Создание UI
var font = UI.Font("Segoe UI", 14f);
var brush = UI.SolidBrush(Color.White);

var root = UI.Column(8)
    .Add(UI.Label("Hello", font, brush))
    .Add(UI.Label("World", font, brush));

// 3. Применение к контейнеру
var factory = new RenderRootFactory(parentControl);
var displayRoot = new DisplayRoot(factory);
displayRoot.Build(root, Point.Empty, new Size(800, 600));

// 4. Очистка
displayRoot.Dispose();
```

---

<a id="checklist"></a>
## Чек-лист

- [ ] Создан статический класс `Adapter` с методом `Initialize()`.
- [ ] Реализован `IWidgetFactory` (создаёт Label, Image, Masks).
- [ ] Реализован `IRenderRootFactory` (принимает parent в конструкторе).
- [ ] Реализован `IRenderRoot` (методы `Build`, `Clear`, `Dispose`).
- [ ] Реализован `ILayoutComponent` (корневой контейнер).
- [ ] Реализован `ILabelComponent` (текст, шрифт, кисти, формат).
- [ ] Реализован `IImageComponent` (изображение, режим отображения).
- [ ] Реализованы маски (Rect, Circle, Ellipse, RoundedRect, Path).
- [ ] Реализован `IEffectComponent` (Opacity, Brightness, Contrast).
- [ ] Реализован `Shadow?` — бросает `NotSupportedException` при не-null.
- [ ] `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException` для градиентов.
- [ ] `ApplyRecursive` бросает `NotSupportedException` для `TransformNode`.
- [ ] Адаптеры клонируют ресурсы при установке.
- [ ] Адаптеры реализуют `IDisposable` и освобождают свои клоны.
- [ ] Добавлены методы конвертации (`ToYourType`, `Wrap`) при необходимости.
- [ ] Написаны тесты (CreateLabel, CreateImage, клонирование ресурсов, Dispose).
- [ ] Публичный API покрыт XML-документацией.