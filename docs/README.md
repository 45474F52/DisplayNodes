# DisplayNodes

Декларативный движок компоновки UI на C# в стиле WPF / Flutter / React. Описывайте интерфейс как дерево узлов — движок сам посчитает размеры, расставит элементы и отрисует через выбранный бэкенд.

```
UI.Column(spacing: 8)
  .Add(UI.Label("Hello", font, brush))
  .Add(UI.Row(12)
    .Add(UI.Label("Status:", font, gray))
    .Add(UI.Label("OK", font, green)));
```

## Содержание

1. [Возможности](#features)
2. [Архитектура](#architecture)
3. [Модули](#modules)
4. [Быстрый старт](#quickstart)
5. [Реактивность](#reactive)
6. [Layout](#layout)
7. [Визуальные эффекты](#effects)
8. [Playground](#playground)
9. [Бэкенды](#backends)
10. [Сборка и тесты](#build)
11. [Структура репозитория](#structure)
12. [Документация](#documentation)
13. [Лицензия](#license)

---

<a id="features"></a>
## Возможности

- **Двухпроходный layout** — `Measure` → `Arrange`, как в WPF/Flutter
- **Контейнеры**: `Stack` (Row/Column), `Grid` (Pixel/Auto/Star), `UniformGrid`, `Overlay`, `WrapPanel`, `Conditional`, `Transform`, `Fixed`, `Stretch`, `Clip` (маски: прямоугольник, круг, эллипс, скруглённый, произвольный путь)
- **Ограничения размеров** — `MinWidth` / `MaxWidth` / `MinHeight` / `MaxHeight`
- **Flex-механика** — `FlexWeight` для пропорционального распределения свободного места
- **Border через композицию** — `UI.Border(...)` (`ClipNode` + `BackgroundNode` + `OverlayNode`)
- **Реактивность** — `Observable<T>`, `ComputedObservable<T>`, `ObservableList<T>`
- **Условный рендеринг** — `ConditionalNode` (`UI.When(...)`) по `Observable<bool>`
- **Fluent API** — цепочки `.Add(...).Padding(...).Margin(...)`
- **Бэкенд-агностичность** — ядро не знает про GDI/WinForms и т.д.
- **Визуальные эффекты** — градиенты, тень, трансформации (API подготовлен, реализация в адаптерах — отдельная задача)
- **Playground** — IDE с подсветкой синтаксиса, автодополнением, undo/redo, find/replace, хот-релоадом UI

---

<a id="architecture"></a>
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

---

<a id="modules"></a>
## Модули

| Модуль | Назначение |
|---|---|
| `Kernel/Core` | Базовые типы (`Point`, `Size`, `Rect`, `Thickness`, `Color`, `Percent`, `GradientStop`, `Shadow`, `Transform`), `LayoutNode`, контейнеры, `Observable<T>`, `ComputedObservable<T>`, `ObservableList<T>` |
| `Kernel/Rendering` | Интерфейсы бэкенда: `IRenderComponent`, `IFont`, `IBrush`, `IImage`, `IMaskComponent`, фабрики |
| `Kernel/Widgets` | `LabelNode`, `ImageNode`, `BackgroundNode`, `ClipNode` — листовые узлы дерева |
| `Kernel/Fluent` | Статический класс `UI`, `DisplayRoot`, fluent-расширения |
| `Gdi` | Обёртки над `System.Drawing` (`GdiFont`, `GdiBrush`, `GdiTextMeasurer`, конвертеры) |
| `WinFormsAdapter` | Адаптер на базе стандартных WinForms-контролов |
| `Tests` | NUnit-тесты ядра и адаптеров |
| `Playground` | Десктопное приложение — IDE для написания скриптов с компиляцией на лету |

---

<a id="quickstart"></a>
## Быстрый старт

```csharp
// 1. Инициализация бэкенда (один раз)
DisplayNodes.WinFormsAdapter.Adapter.Initialize();

// 2. Построение дерева
IFont font = UI.Font("Segoe UI", 14f);
IBrush white = UI.SolidBrush(Color.White.FromGdi());

LayoutNode root = UI.Column(8)
    .Padding(20)
    .Add(UI.Label("Hello, DisplayNodes!", font, white))
    .Add(UI.Row(12)
        .Add(UI.Label("Status:", font, UI.SolidBrush(Color.Gray.FromGdi())))
        .Add(UI.Label("OK", font, UI.SolidBrush(Color.LimeGreen.FromGdi()))));

// 3. Применение к корневому контейнеру
var displayRoot = new DisplayRoot(new RenderRootFactory(parentComponent));
displayRoot.Build(root, Core.Point.Empty, new Core.Size(800, 600));
```

### Border с скруглёнными углами

```csharp
UI.Border(UI.SolidBrush(Color.FromArgb(45, 45, 48).FromGdi()), cornerRadius: 8)
    .Padding(16)
    .Add(UI.Label("Card content", font, white));
```

### Ограничения размеров

```csharp
UI.Label("Text", font, white)
    .MinWidth(100)
    .MaxWidth(400)
    .MinHeight(30);
```

---

<a id="reactive"></a>
## Реактивность

### Observable\<T\>

```csharp
var counter = new Observable<int>(0);

var node = UI.Column(8)
    .Add(UI.Label("0", font, brush)
        .BindText(new ComputedObservable<string>(
            () => counter.Value.ToString(), counter)));

counter.Value = 10;  // UI обновится автоматически
```

**Важно**: подписки автоматически отписываются при `Dispose` виджета.

### ComputedObservable\<T\>

Вычисляемое свойство на основе других источников:

```csharp
var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName, lastName);

firstName.Value = "Petr";
// fullName.Value == "Petr Petrov" (автоматически)
```

### ObservableList\<T\>

Реактивная коллекция:

```csharp
var items = new ObservableList<string>();
items.Changed += change => Console.WriteLine($"{change.Type}: {change.Item}");
items.Add("Item 1");
```

### ConditionalNode

Условный рендеринг:

```csharp
var isLoggedIn = new Observable<bool>(false);

var node = UI.When(
    isLoggedIn,
    trueNode: UI.Label("Welcome!", font, green),
    falseNode: UI.Label("Please log in", font, gray));
```

---

<a id="layout"></a>
## Layout

### Flex-механика

Пропорциональное распределение свободного пространства в `StackLayoutNode`:

```csharp
UI.Column(8)
    .Add(UI.Label("Fixed", font, brush))               // natural size
    .Add(UI.Label("Flexible", font, brush).Flex(1))     // 1 доля свободного места
    .Add(UI.Label("Double", font, brush).Flex(2));      // 2 доли
```

### StretchNode

Узел, растягивающийся вдоль обеих осей:

```csharp
UI.Column(0)
    .Add(UI.Label("Header", font, brush).Padding(12))
    .Add(new StretchNode(0, 0).Flex(1))
    .Add(UI.Label("Footer", font, brush).Padding(12));
```

### WrapPanel

Перенос детей на следующую строку/столбец:

```csharp
UI.WrapPanel(direction: WrapDirection.Horizontal, spacing: 8, lineSpacing: 8)
    .Add(UI.Label("Tag 1", font, brush))
    .Add(UI.Label("Tag 2", font, brush))
    .Add(UI.Label("Tag 3", font, brush));
```

---

<a id="effects"></a>
## Визуальные эффекты

Три эффекта описаны в API, но **не поддерживаются** текущими адаптерами. При попытке использования бросают `NotSupportedException`. API подготовлен для будущей реализации (см. [ROADMAP.md](ROADMAP.md)).

| Эффект | API |
|---|---|
| Градиенты | `UI.LinearGradient(...)`, `UI.RadialGradient(...)` |
| Тень | `.Shadow(offsetX, offsetY, blurRadius)` |
| Трансформации | `UI.Transform(scaleX, scaleY, rotation)` |

Пример:

```csharp
// API готов, но текущий адаптер бросит NotSupportedException
var bg = UI.Background(UI.LinearGradient(
    new Point(0, 0), new Point(100, 0),
    new GradientStop(Color.Red, 0),
    new GradientStop(Color.Blue, 100)));
```

---

<a id="playground"></a>
## Playground

Десктопное приложение для интерактивной разработки UI-скриптов.

**Возможности редактора:**
- Подсветка синтаксиса C# (свой лексер + WinAPI `EM_SETCHARFORMAT`)
- Автодополнение по `.` (рефлексия по загруженным сборкам)
- Tooltip с сигнатурой, типом возврата и XML-документацией
- Undo/Redo с группировкой набора
- Find/Replace (plain + regex)
- Номера строк, направляющие отступа, подсветка текущей строки
- Волнистые подчёркивания строк с ошибками компиляции
- Горячие клавиши: `Ctrl+/`, `Ctrl+D`, `Ctrl+Shift+K`, `Alt+↑/↓`, `F5`...

**Компиляция:**
- Автоматически ищет `csc.exe 4.0+` (поддержка именованных аргументов)
- Если не нашёл — fallback на встроенный `CSharpCodeProvider` (C# 3.0)
- Скрипт оборачивается в шаблон `UserScript.Build()`, ошибки парсятся и показываются с корректными номерами строк

**Экспорт preview:**
- Пункт меню `Файл` → `Экспорт изображения` (или `Ctrl+E`)
- Сохранение в PNG / JPEG / BMP

**Абстракция preview:**
- `IPreviewHost` — интерфейс, инкапсулирующий работу с конкретным адаптером
- `WinFormsPreviewHost` — реализация для WinFormsAdapter

---

<a id="backends"></a>
## Бэкенды

| Бэкенд | Особенности |
|---|---|
| **WinForms** | Обёртки над стандартными контролами. `Opacity` — только для фона; `Brightness`/`Contrast` не поддерживаются |
| **GDI** | Низкоуровневые обёртки (`GdiFont`, `GdiBrush`, ...). Используется обоими адаптерами для работы с ресурсами |

Свой бэкенд — реализуйте `IWidgetFactory`, `IRenderRootFactory`, `ITextMeasurer` и вызовите `UI.Factory = ...` и т.д.

**Не поддерживается текущими адаптерами:**
- Градиентные кисти — `NotSupportedException`
- Тень — `NotSupportedException`
- Трансформации (`TransformNode`) — `NotSupportedException`

---

<a id="build"></a>
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

---

<a id="structure"></a>
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
├── Playground/          # IDE
│   ├── Compilation/     # Компилятор скриптов
│   ├── Editor/          # CodeEditor, подсветка, автодополнение
│   └── Infrastructure/  # Настройки, логирование, состояние
└── docs/                # Документация
```

---

<a id="documentation"></a>
## Документация

- [EXAMPLES.md](EXAMPLES.md) — примеры использования всех фич
- [ARCHITECTURE.md](ARCHITECTURE.md) — архитектура и алгоритмы
- [CUSTOM_CONTAINERS.md](CUSTOM_CONTAINERS.md) — создание своих контейнеров
- [CUSTOM_WIDGETS.md](CUSTOM_WIDGETS.md) — создание своих виджетов
- [CUSTOM_ADAPTERS_GUIDELINE.md](CUSTOM_ADAPTERS_GUIDELINE.md) — создание адаптера бэкенда
- [OBSERVABLE_DEEP_DIVE.md](OBSERVABLE_DEEP_DIVE.md) — глубокое руководство по `Observable<T>`
- [DNP_TEMPLATES.md](DNP_TEMPLATES.md) — шаблоны скриптов Playground
- [CONTRIBUTING.md](CONTRIBUTING.md) — руководство для разработчиков
- [ROADMAP.md](ROADMAP.md) — план развития
- [CHANGELOG.md](CHANGELOG.md) — история изменений


<a id="license"></a>
## Лицензия

Copyright 2026 AES

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.