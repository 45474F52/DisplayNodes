# Создание адаптера для DisplayNodes

Руководство по созданию собственного бэкенда рендеринга для DisplayNodes.

## Архитектура адаптера

Адаптер состоит из следующих компонентов:

```
YourAdapter/
├── Adapter.cs                    # Точка входа (статический класс)
├── WidgetFactory.cs              # Фабрика виджетов (IWidgetFactory)
├── RenderRootFactory.cs          # Фабрика корневого контейнера (IRenderRootFactory)
├── RenderRoot.cs                 # Корневой контейнер (IRenderRoot)
└── Components/
    ├── ComponentBase.cs          # Базовый класс для всех компонентов
    ├── Layout.cs                 # Корневой layout-контейнер (ILayoutComponent)
    ├── Label.cs                  # Текстовая метка (ILabelComponent)
    ├── Image.cs                  # Изображение (IImageComponent)
    └── Masks/
        ├── MaskBase.cs           # Базовый класс для масок
        ├── RectMask.cs           # Прямоугольная маска
        ├── CircleMask.cs         # Круговая маска
        ├── EllipseMask.cs        # Эллиптическая маска
        ├── RoundedRectMask.cs    # Скруглённые углы
        └── PathMask.cs           # Произвольная форма
```

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

**Важно:** `UI.Measurer`, `UI.BrushFactory`, `UI.FontFactory`, `UI.ImageFactory` можно переиспользовать из `DisplayNodes.Gdi`, если ваш бэкенд работает с GDI+-ресурсами.

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
                // Клонировать и установить
                // ...
            }
        }
        
        // Опционально: ITextLayoutComponent
        public TextDrawMethod DrawMethod { get; set; }
        public LabelStretch Stretch { get; set; }
        
        // Опционально: IEffectComponent
        public double Opacity { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        
        public void Dispose()
        {
            _ownedFont?.Dispose();
            _ownedForeground?.Dispose();
            _ownedBackground?.Dispose();
            _label.Dispose();
        }
    }
}
```

**Критически важно:** Адаптер **клонирует** ресурсы (Font, Brush, StringFormat) при установке, чтобы consumer-код сохранял владение оригиналами. Иначе Dispose адаптера освободит ресурсы, которые ещё используются.

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
        
        public void Dispose()
        {
            _ownedBitmap?.Dispose();
            _image.Dispose();
        }
    }
}
```

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
            // ...
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

## Шаг 8: Конвертеры (опционально)

Если ваш бэкенд использует GDI+-типы, добавьте методы расширения:

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
        
        // Обёртывание вашего типа в абстрактный интерфейс
        public static IFont Wrap(this YourFontType font)
        {
            return new GdiFont(font);
        }
        
        // Аналогично для Brush, Image, и т.д.
    }
}
```

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

## Пример использования

```csharp
// 1. Инициализация (один раз)
YourNamespace.Adapter.Initialize();

// 2. Создание UI
var font = UI.Font("Segoe UI", 14f);
var brush = UI.Brush(Color.White);

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

## Чек-лист

- [ ] Создан статический класс `Adapter` с методом `Initialize()`
- [ ] Реализован `IWidgetFactory` (создаёт Label, Image, Masks)
- [ ] Реализован `IRenderRootFactory` (принимает parent в конструкторе)
- [ ] Реализован `IRenderRoot` (методы `Build`, `Clear`, `Dispose`)
- [ ] Реализован `ILayoutComponent` (корневой контейнер)
- [ ] Реализован `ILabelComponent` (текст, шрифт, кисти)
- [ ] Реализован `IImageComponent` (изображение, режим отображения)
- [ ] Реализованы маски (Rect, Circle, Ellipse, RoundedRect, Path)
- [ ] Адаптеры клонируют ресурсы при установке
- [ ] Адаптеры реализуют `IDisposable` и освобождают свои клоны
- [ ] Добавлены методы конвертации (ToYourType, Wrap) при необходимости

## Известные ограничения

### WinForms-бэкенд

- `Opacity` работает только для фона (через `BackColor.A`)
- `Brightness` и `Contrast` не поддерживаются (помечены `[Obsolete]`)
- `TextDrawMethod` и `LabelStretch` не поддерживаются
