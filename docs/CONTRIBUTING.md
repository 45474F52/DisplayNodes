# РУКОВОДСТВО

Руководство для разработчиков, вносящих изменения в DisplayNodes.

## Общие принципы

- Ядро (`Kernel`) не должно знать о конкретных бэкендах рендеринга.
- Все ресурсы (шрифты, кисти, изображения) представляются интерфейсами из `Core.Rendering`.
- Адаптеры клонируют ресурсы при установке, чтобы consumer-код сохранял владение оригиналами.
- Публичный API должен быть покрыт XML-документацией на русском языке.
- Breaking changes требуют обсуждения. Добавление функционала — обратно совместимо.

## Структура проекта

```
DisplayNodes/
├── Kernel/
│   ├── Core/              # Базовые типы, LayoutNode, контейнеры, Observable
│   ├── Core/Rendering/    # Интерфейсы бэкенда (IFont, IBrush, IRenderComponent, ...)
│   ├── Widgets/           # LabelNode, ImageNode, ClipNode, BackgroundNode
│   ├── Fluent/            # UI-фабрика, DisplayRoot, fluent-расширения
│   └── Helpers/           # Методы расширения для обхода дерева
├── Gdi/                   # GDI+-реализации (GdiFont, GdiBrush, GdiTextMeasurer)
├── WinFormsAdapter/       # Бэкенд на WinForms
├── Tests/                 # NUnit-тесты
└── Playground/            # IDE для скриптов
```

## Целевой фреймворк

- **Ядро и адаптеры:** .NET Framework 3.5 (обусловлено средой применения).
- **Playground:** .NET Framework 4.x (для поддержки `csc.exe 4.0+`).
- **Тесты:** NUnit 3.x, запуск через `nunit3-console`.

Не используйте API из .NET 4.0+ в ядре (`Lazy<T>`, `Tuple`, `dynamic`, `ArrayPool`, `Task`, async/await).

## Стиль кода

### Именование

| Элемент | Стиль | Пример |
|---|---|---|
| Namespace | PascalCase | `DisplayNodes.Core.Rendering` |
| Публичный класс/структура | PascalCase | `LayoutNode`, `GridLength` |
| Интерфейс | I + PascalCase | `IRenderComponent`, `IFont` |
| Публичный метод | PascalCase | `MeasureOverride`, `CreateSolidBrush` |
| Публичное свойство | PascalCase | `DesiredSize`, `Bounds` |
| Приватное поле | `_camelCase` | `_parentAdapter`, `_ownedFont` |
| Локальная переменная | camelCase | `maxWidth`, `childSize` |
| Параметр | camelCase | `available`, `finalRect` |
| Константа | PascalCase | `HEADER_LINE_COUNT`, `WIDTH_OFFSET` |

### Разрешение конфликтов имён

При использовании типов, совпадающих с `System.Drawing`, применяйте алиасы:

```csharp
using System.Drawing;
using DisplayNodes.Core;
using Color = DisplayNodes.Core.Color;  // приоритет ядру
using Size = DisplayNodes.Core.Size;
using Point = DisplayNodes.Core.Point;
```

В адаптерах, где нужен GDI-тип, используйте полное имя:

```csharp
var gdiColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
```

### Структуры (value types)

- Все базовые типы (`Point`, `Size`, `Rect`, `Thickness`, `Color`, `GridLength`) — `readonly struct`.
- Реализуйте `IEquatable<T>`, операторы `==`/`!=`, переопределяйте `Equals`/`GetHashCode`/`ToString`.
- Добавляйте `[DebuggerDisplay("...")]` для удобства отладки.
- Избегайте мутабельных структур.

```csharp
[Serializable]
[DebuggerDisplay("({X}:{Y})")]
public readonly struct Point : IEquatable<Point>
{
    public readonly int X;
    public readonly int Y;
    // ...
}
```

### Классы (reference types)

- `LayoutNode` — абстрактный базовый класс.
- Контейнеры (`StackLayoutNode`, `GridNode`, ...) — наследники `LayoutNode`.
- Виджеты (`LabelNode`, `ImageNode`) — наследники `WidgetNode`.
- Компоненты рендерера — реализуют `IRenderComponent` + специализированные интерфейсы.

### XML-документация

Обязательна для всех публичных членов:

```csharp
/// <summary>
/// Вычисляет <see cref="DesiredSize"/> на основе доступного пространства.
/// </summary>
/// <param name="available">Доступное пространство от родителя.</param>
/// <returns>Возвращает <see cref="DesiredSize"/> с учётом <see cref="Margin"/>.</returns>
public Size Measure(Size available) { ... }
```

Для внутренних классов документация опциональна, но приветствуется.

### Исключения

- `ArgumentNullException` — для null-параметров, где null недопустим.
- `ArgumentOutOfRangeException` — для некорректных значений (отрицательные размеры, индексы вне диапазона).
- `InvalidOperationException` — для вызовов в некорректном состоянии (например, `UI.Font` до инициализации).
- `ArgumentException` — для общих ошибок аргументов.

Всегда указывайте `nameof(param)`:

```csharp
if (child == null)
    throw new ArgumentNullException(nameof(child));
```

### Потокобезопасность

- `Observable<T>` использует `lock` для защиты подписчиков.
- `ApiIndex` в Playground — `lock` для словарей.
- `[ThreadStatic]` для кэшей GDI-объектов (`GdiTextMeasurer`).
- UI-компоненты (`IRenderComponent`) должны использоваться только из потока, в котором созданы.

### Управление ресурсами

- Адаптеры реализуют `IDisposable`.
- Адаптеры клонируют ресурсы (`Font`, `SolidBrush`, `StringFormat`, `Bitmap`) при установке.
- Consumer-код владеет оригиналами, адаптер владеет клонами.
- Используйте `using` для локальных GDI-объектов.

```csharp
// Правильно: consumer владеет ресурсом
using (var font = new Font("Arial", 12f))
{
    var wrapped = font.Wrap();
    label.Font = wrapped;  // адаптер клонирует
}

// Неправильно: передача владения
var font = new Font("Arial", 12f);
label.Font = font.Wrap();
font.Dispose();  // адаптер может использовать освобождённый ресурс
```

## Добавление нового контейнера

1. Создайте класс в `Kernel/Core`, наследник `LayoutNode`.
2. Переопределите `MeasureOverride(Size available)` — вычислите `DesiredSize`.
3. Переопределите `ArrangeOverride(Rect finalRect)` — разместите детей через `child.Arrange(rect)`.
4. Контейнеры должны занимать весь предоставленный слот. Переопределите `Arrange(Rect)` как `sealed`:

```csharp
public sealed override void Arrange(Rect finalRect)
{
    Rect inner = finalRect.Deflate(Margin);
    Bounds = inner;
    ArrangeOverride(inner);
}
```

5. Добавьте метод-фабрику в `UI`:

```csharp
public static MyContainerNode MyContainer(int param)
    => new MyContainerNode(param);
```

6. Добавьте fluent-расширения в `LayoutNodeFluent` или отдельный файл.
7. Напишите NUnit-тесты в `Tests/Core/`.

Пример: см. `StackLayoutNode`, `GridNode`, `UniformGridNode`, `OverlayNode`.

## Добавление нового виджета

1. Создайте класс в `Kernel/Widgets`, наследник `WidgetNode`.
2. Переопределите `MeasureOverride(Size available)` — верните желаемый размер.
3. Переопределите `ApplyBounds()` — запишите `Bounds` в свойства компонента.
4. Если виджет поддерживает реактивные привязки, используйте `AddSubscription`:

```csharp
public MyWidget BindValue(Observable<string> source)
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));
    Component.Value = source.Value;
    _ = AddSubscription(source.Subscribe(v => Component.Value = v));
    return this;
}
```

5. Добавьте fluent-расширения.
6. Напишите тесты.

Пример: см. `LabelNode`, `ImageNode`, `BackgroundNode`, `ClipNode`.

## Добавление нового бэкенда (адаптера)

См. подробное руководство в `CUSTOM_ADAPTERS_GUIDELINE.md`. Краткий чек-лист:

1. Создайте проект `YourAdapter`.
2. Реализуйте `IWidgetFactory` (создание Label, Image, Masks).
3. Реализуйте `IRenderRootFactory` и `IRenderRoot`.
4. Реализуйте `ILayoutComponent` (корневой контейнер).
5. Реализуйте `ILabelComponent`, `IImageComponent`, `IMaskComponent`.
6. Адаптеры должны клонировать ресурсы при установке.
7. Реализуйте `IDisposable` во всех компонентах.
8. Создайте статический класс `Adapter` с методом `Initialize()`.

## Тестирование

### Структура тестов

```
Tests/
├── Core/                  # Тесты контейнеров и типов
├── Gdi/                   # Тесты GDI-обёрток
├── Adapters/
│   └── WinFormsAdapter/
└── Playground/            # (опционально) тесты компиляции
```

### Правила написания тестов

- Один тест — одна проверка. Используйте `Assert.EnterMultipleScope()` для нескольких утверждений.
- Имена тестов: `MethodName_Condition_ExpectedResult`.
- Обязательно тестируйте:
  - Measure/Arrange для контейнеров.
  - Клонирование ресурсов в адаптерах (`_ClonesOnSet_OriginalNotDisposedByAdapter`).
  - Граничные случаи (null, пустые коллекции, отрицательные размеры).
- Освобождайте ресурсы в тестах: `(component as IDisposable)?.Dispose()`.

```csharp
[Test]
public void Grid_StarColumns_DistributesSpaceProportionally()
{
    var grid = new GridNode();
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(2)));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
    _ = grid.Add(new FixedNode(10, 10), 0, 0);
    _ = grid.Add(new FixedNode(10, 10), 0, 1);
    _ = grid.Measure(new Size(300, 100));
    grid.Arrange(new Rect(0, 0, 300, 100));
    using (Assert.EnterMultipleScope())
    {
        Assert.That(grid.Children[0].Bounds.Width, Is.EqualTo(100));
        Assert.That(grid.Children[1].Bounds.Width, Is.EqualTo(200));
    }
}
```

### Запуск тестов

```bash
# Сборка
msbuild DisplayNodes.sln /p:Configuration=Debug

# Запуск всех тестов
nunit3-console Tests/bin/Debug/Tests.dll

# Запуск конкретного теста
nunit3-console Tests/bin/Debug/Tests.dll --where "name == GridNodeTests"
```

## Коммиты

### Формат сообщения

```
<тип>(<область>): <краткое описание>

[опциональное тело]

[опциональный футер]
```

### Типы

- `feat` — новая функциональность.
- `fix` — исправление ошибки.
- `refactor` — изменение структуры без изменения поведения.
- `docs` — только документация.
- `test` — добавление или исправление тестов.
- `chore` — инфраструктура, зависимости, конфигурация.
- `perf` — оптимизация производительности.
- `style` — форматирование, пробелы, точки с запятой (без изменения логики).

### Область

- `core` — ядро (LayoutNode, контейнеры, типы).
- `rendering` — интерфейсы рендеринга.
- `widgets` — виджеты (Label, Image, Clip).
- `fluent` — UI-фабрика, fluent-расширения.
- `gdi` — GDI+-реализации.
- `adapter` — адаптеры (укажите конкретный: `winforms`, `...`).
- `playground` — IDE.
- `tests` — тесты.

### Примеры

```
feat(core): добавить WrapPanelNode для переноса строк

fix(gdi): исправить утечку GDI-handles в GdiTextMeasurer

docs(kernel): обновить README с примерами GridNode

test(core): добавить тесты для UniformGridNode с Padding

refactor(fluent): вынести BindText в отдельный метод LabelNode
```

## Pull Requests

### Перед созданием PR

1. Убедитесь, что все тесты проходят.
2. Проверьте, что новый код соответствует стилю проекта.
3. Обновите документацию, если изменился публичный API.
4. Добавьте тесты для новой функциональности.
5. Проверьте, что изменения обратно совместимы (если это не breaking change).

### Описание PR

- Кратко опишите, что делает PR.
- Укажите мотивацию (зачем это нужно).
- Перечислите изменения.
- Если есть breaking changes, опишите миграцию.
- Приложите скриншоты, если изменился визуал.

### Ревью

- PR требует минимум одного одобрения.
- Автор PR отвечает на комментарии и вносит правки.
- После одобрения автор сливает PR (squash merge предпочтителен).

## Проверка перед отправкой

- [ ] Все тесты проходят.
- [ ] Код компилируется без предупреждений.
- [ ] Новый публичный API покрыт XML-документацией.
- [ ] Изменения обратно совместимы (или описаны breaking changes).
- [ ] Ресурсы освобождаются корректно (нет утечек).
- [ ] Потокобезопасность соблюдена (если применимо).
- [ ] Коммиты следуют формату Conventional Commits.
- [ ] Документация обновлена (README, примеры).

## Вопросы и обсуждения

- Для вопросов по API создайте Issue с меткой `question`.
- Для предложений по функциональности — Issue с меткой `enhancement`.
- Для сообщений об ошибках — Issue с меткой `bug` и минимальным воспроизводимым примером.