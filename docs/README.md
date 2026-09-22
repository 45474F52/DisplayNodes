# DisplayNodes

Декларативный движок компоновки UI на C# в стиле WPF / Flutter / React. Описывайте интерфейс как дерево узлов — движок сам посчитает размеры, расставит элементы и отрисует через выбранный бэкенд.

```
UI.Column(spacing: 8)
  .Add(UI.Label("Hello", font, brush))
  .Add(UI.Row(12)
    .Add(UI.Label("Status:", font, gray))
    .Add(UI.Label("OK", font, green)));
```

## Возможности

- **Двухпроходный layout** — `Measure` → `Arrange`, как в WPF/Flutter
- **Контейнеры**: `Stack` (Row/Column), `Grid` (Pixel/Auto/Star), `UniformGrid`, `Overlay`, `Fixed`, `Clip` (маски: прямоугольник, круг, эллипс, скруглённый, произвольный путь)
- **Реактивность** — `Observable<T>` с автоматической подпиской виджетов
- **Fluent API** — цепочки `.Add(...).Padding(...).Margin(...)`
- **Бэкенд-агностичность** — ядро не знает про GDI/WinForms и т.д.
- **Playground** — IDE с подсветкой синтаксиса, автодополнением, undo/redo, find/replace, хот-релоадом UI

## Архитектура

```
┌──────────────────────────────────────────────────────────┐
│                      Playground                          │  ← IDE: редактор, компилятор, preview
├──────────────────────────────────────────────────────────┤
│                        Fluent                            │  ← UI-фабрика, DisplayRoot, расширения
├──────────────────────────────────────────────────────────┤
│   Kernel   │   Widgets   │   Core (LayoutNode, ...)      │  ← Ядро: layout, Observable, типы
├────────────┴─────────────┴───────────────────────────────┤
│         Rendering (IRenderComponent, IFont, ...)         │  ← Абстракции
├──────────────────────────────────────────────────────────┤
│      Gdi     │   MyCustomAdapter   │   WinFormsAdapter   │  ← Реализации
└──────────────────────────────────────────────────────────┘
```

## Модули

| Модуль | Назначение |
|---|---|
| `Kernel/Core` | Базовые типы (`Point`, `Size`, `Rect`, `Thickness`, `Color`), `LayoutNode`, контейнеры, `Observable<T>` |
| `Kernel/Rendering` | Интерфейсы бэкенда: `IRenderComponent`, `IFont`, `IBrush`, `IImage`, `IMaskComponent`, фабрики |
| `Kernel/Widgets` | `LabelNode`, `ImageNode`, `BackgroundNode`, `ClipNode` — листовые узлы дерева |
| `Kernel/Fluent` | Статический класс `UI`, `DisplayRoot`, fluent-расширения |
| `Gdi` | Обёртки над `System.Drawing` (`GdiFont`, `GdiBrush`, `GdiTextMeasurer`, конвертеры) |
| `WinFormsAdapter` | Адаптер на базе стандартных WinForms-контролов |
| `Tests` | NUnit-тесты ядра и адаптеров |
| `Playground` | Десктопное приложение — IDE для написания скриптов с компиляцией на лету |

## Быстрый старт

```csharp
// 1. Инициализация бэкенда (один раз)
DisplayNodes.WinFormsAdapter.Adapter.Initialize();

// 2. Построение дерева
IFont font = UI.Font("Segoe UI", 14f);
IBrush white = UI.Brush(Color.White.FromGdi());

LayoutNode root = UI.Column(8)
    .Padding(20)
    .Add(UI.Label("Hello, DisplayNodes!", font, white))
    .Add(UI.Row(12)
        .Add(UI.Label("Status:", font, UI.Brush(Color.Gray.FromGdi())))
        .Add(UI.Label("OK", font, UI.Brush(Color.LimeGreen.FromGdi()))));

// 3. Применение к корневому контейнеру
var displayRoot = new DisplayRoot(new RenderRootFactory(parentComponent));
displayRoot.Build(root, Core.Point.Empty, new Core.Size(800, 600));
```

## Реактивность

```csharp
var counter = new Observable<int>(0);

var node = UI.Column(8)
    .Add(UI.Label("0", font, brush).BindText(
        new Observable<string>("0")))  // пример
    .Add(UI.Button("+"));              // по клику: counter.Value++;
```

**Важно**: Подписки автоматически отписываются при `Dispose` виджета.

## Playground

Десктопное приложение для интерактивной разработки UI-скриптов.

**Возможности редактора:**
- Подсветка синтаксиса C# (свой лексер + WinAPI `EM_SETCHARFORMAT`)
- Автодополнение по `.` (рефлексия по загруженным сборкам)
- Undo/Redo с группировкой набора
- Find/Replace (plain + regex)
- Номера строк, направляющие отступа, подсветка текущей строки
- Волнистые подчёркивания строк с ошибками компиляции
- Горячие клавиши: `Ctrl+/`, `Ctrl+D`, `Ctrl+Shift+K`, `Alt+↑/↓`, `F5`...

**Компиляция:**
- Автоматически ищет `csc.exe 4.0+` (поддержка именованных аргументов)
- Если не нашёл — fallback на встроенный `CSharpCodeProvider` (C# 3.0)
- Скрипт оборачивается в шаблон `UserScript.Build()`, ошибки парсятся и показываются с корректными номерами строк

## Бэкенды

| Бэкенд | Особенности |
|---|---|
| **WinForms** | Обёртки над стандартными контролами. `Opacity` — только для фона; `Brightness`/`Contrast` не поддерживаются |
| **GDI** | Низкоуровневые обёртки (`GdiFont`, `GdiBrush`, ...). Используется обоими адаптерами для работы с ресурсами |

Свой бэкенд — реализуйте `IWidgetFactory`, `IRenderRootFactory`, `ITextMeasurer` и вызовите `UI.Factory = ...` и т.д.

## Сборка и тесты

```bash
# Сборка (требуется .NET Framework 3.5 SDK для ядра)
msbuild DisplayNodes.sln /p:Configuration=Release

# Тесты
nunit3-console Tests/bin/Release/Tests.dll
```

**Требования:**
- .NET Framework 3.5 (ядро, адаптеры)
- Windows (WinForms, GDI+)

## Структура репозитория

```
DisplayNodes/
├── Kernel/
│   ├── Core/            # LayoutNode, контейнеры, типы, Observable
│   ├── Rendering/       # Интерфейсы бэкенда
│   ├── Widgets/         # LabelNode, ImageNode, ClipNode, ...
│   ├── Fluent/          # UI-фабрика, DisplayRoot
│   └── Helpers/         # Расширения для обхода дерева
├── Gdi/                 # GDI+ реализации
├── WinFormsAdapter/
├── Tests/               # NUnit
└── Playground/          # IDE
    ├── Compilation/     # Компилятор скриптов
    ├── Editor/          # CodeEditor, подсветка, автодополнение
    └── Infrastructure/  # Настройки, логирование, состояние
```
