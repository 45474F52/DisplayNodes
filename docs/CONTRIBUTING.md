# Руководство

Руководство для разработчиков, вносящих изменения в DisplayNodes.

## Содержание

1. [Общие принципы](#principles)
2. [Структура проекта](#structure)
3. [Целевой фреймворк](#target)
4. [Стиль кода](#style)
5. [Стиль документации](#docs)
6. [Reactive-примитивы](#reactive)
7. [Визуальные эффекты и NotSupportedException](#effects)
8. [Добавление нового контейнера](#new-container)
9. [Добавление нового виджета](#new-widget)
10. [Добавление нового бэкенда](#new-backend)
11. [Тестирование](#testing)
12. [Коммиты](#commits)
13. [Pull Requests](#pull-requests)
14. [Чек-лист перед отправкой](#checklist)

---

<a id="principles"></a>
## Общие принципы

- Ядро (`Kernel`) не должно знать о конкретных бэкендах рендеринга.
- Все ресурсы (шрифты, кисти, изображения) представляются интерфейсами из `Core.Rendering`.
- Адаптеры клонируют ресурсы при установке, чтобы consumer-код сохранял владение оригиналами.
- Публичный API должен быть покрыт XML-документацией на русском языке.
- Breaking changes требуют обсуждения. Добавление функционала — обратно совместимо.

---

<a id="structure"></a>
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
├── Tests/                 # NUnit-тесты
└── Playground/            # IDE для скриптов
    ├── Compilation/       # Компилятор скриптов
    ├── Editor/            # CodeEditor, подсветка, автодополнение
    └── Infrastructure/    # Настройки, логирование, состояние
```

---

<a id="target"></a>
## Целевой фреймворк

- **Ядро и адаптеры:** .NET Framework 3.5 (обусловлено средой применения).
- **Playground:** .NET Framework 4.x (для поддержки `csc.exe 4.0+`).
- **Тесты:** NUnit 3.x, запуск через `nunit3-console`.

Не используйте API из .NET 4.0+ в ядре (`Lazy<T>`, `Tuple`, `dynamic`, `ArrayPool`, `Task`, async/await).

---

<a id="style"></a>
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

- Все базовые типы (`Point`, `Size`, `Rect`, `Thickness`, `Color`, `GridLength`, `Percent`, `GradientStop`, `Shadow`, `Transform`) — `readonly struct`.
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
- Контейнеры (`StackLayoutNode`, `GridNode`, `WrapPanelNode`, `ConditionalNode`, `TransformNode`, `OverlayNode`) — наследники `LayoutNode`.
- Виджеты (`LabelNode`, `ImageNode`, `BackgroundNode`) — наследники `WidgetNode`.
- `StretchNode` — наследник `LayoutNode`, публичный, без детей.
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
- `NotSupportedException` — для функциональности, описанной в API, но не реализованной в текущей версии (см. [Визуальные эффекты](#effects)).

Всегда указывайте `nameof(param)`:

```csharp
if (child == null)
    throw new ArgumentNullException(nameof(child));
```

### Потокобезопасность

- `Observable<T>` использует `lock` для защиты подписчиков и значения.
- `ObservableList<T>` использует `lock` для защиты коллекции.
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

---

<a id="docs"></a>
## Стиль документации

- Все XML-комментарии — на русском языке.
- Примеры кода в `<code>` или `<example>`.
- Ссылки через `<see cref="..."/>`, `<paramref name="..."/>`, `<typeparamref name="..."/>`.
- Разделы `<remarks>` для дополнительного контекста.
- Документация в `docs/*.md` — обновляется при изменениях публичного API.
- При переименовании публичных API — отражается в `docs/CHANGELOG.md` (раздел `Renamed`).
- **Не упоминайте закрытые легаси-проекты** в документации. Используйте нейтральные формулировки («адаптер», «бэкенд»).

---

<a id="reactive"></a>
## Reactive-примитивы

### Observable\<T\>

Основной реактивный примитив. Реализует `IObservableSource`.

- Потокобезопасен через `lock`.
- Уведомляет подписчиков только при изменении значения.
- Уведомления — вне `lock` (избегаем deadlock).
- Исключения в подписчиках перехватываются.

### IObservableSource

Не-generic интерфейс для унификации `Observable<T>` с разными `T`:

```csharp
public interface IObservableSource
{
    IDisposable Subscribe(Action<object> callback);
}
```

**Зачем:** из-за инвариантности generic'ов в C# нельзя передать `Observable<int>` и `Observable<string>` в один массив `Observable<object>[]`. Через `IObservableSource` — можно.

**При добавлении нового реактивного класса:** если он должен поддерживать зависимости разных типов — реализуйте `IObservableSource` явно.

### ComputedObservable\<T\>

Вычисляемое свойство на основе других источников.

- Начальное значение вычисляется в конструкторе через `ComputeInitial`.
- При изменении любой зависимости вызывается `compute()`.
- Исключения в `compute` перехватываются — старое значение остаётся, подписчики не уведомляются.
- `Dispose` отписывается от всех зависимостей.
- `Refresh()` — ручной пересчёт.

**При добавлении нового Computed-типа:** следуйте тем же принципам — начальное значение в конструкторе, отписка в `Dispose`, защита от исключений.

### ObservableList\<T\>

Реактивная коллекция с событием `Changed`.

- Потокобезопасна через `lock`.
- Событие `Changed` вызывается **вне lock'а** — подписчик может безопасно изменять коллекцию.
- Все подписчики вызываются **отдельно** через `GetInvocationList()`. Исключение в одном не ломает цепочку.
- `Dispose` делает коллекцию непригодной: любая операция бросает `ObjectDisposedException`.
- `GetEnumerator` возвращает enumerator `List<T>` — `foreach` бросает `InvalidOperationException` при модификации.
- `ToList()` — безопасный снимок.

**При добавлении новых реактивных коллекций:** вызывайте подписчиков через `GetInvocationList()` — multicast delegate прерывает цепочку при исключении в одном из подписчиков.

### ListChange\<T\> и ListChangeType

`ListChange<T>` — readonly struct, описывающий одно изменение: `Type`, `OldIndex`, `NewIndex`, `Item`.

`ListChangeType` — enum: `Add`, `Insert`, `Remove`, `Replace`, `Move`, `Reset`.

**Соглашение:** для `Add`/`Insert` — `OldIndex = -1`, для `Remove` — `NewIndex = -1`, для `Reset` — оба `-1`.

---

<a id="effects"></a>
## Визуальные эффекты и NotSupportedException

Три визуальных эффекта описаны в API ядра, но **не поддерживаются** текущими адаптерами:

| Эффект | API | Поведение в адаптере |
|---|---|---|
| Градиенты | `IBrushFactory.CreateLinearGradient`/`CreateRadialGradient` | `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException` для градиентных кистей |
| Shadow | `IEffectComponent.Shadow?` | Setter бросает `NotSupportedException` при `value.HasValue` |
| TransformNode | `TransformNode` в дереве | `ApplyRecursive` бросает `NotSupportedException` при обнаружении |

### Паттерн

**API готовится заранее, реализация в адаптерах — отдельная задача.** Это позволяет:

1. Ядро и адаптеры не блокируют друг друга.
2. Потребители видят в API все возможности сразу.
3. `NotSupportedException` с внятным сообщением показывает, что функциональность есть, но не реализована в конкретном бэкенде.

### Как добавлять новые эффекты

1. Описать API в ядре (интерфейс, структура, свойство).
2. Добавить Fluent-метод.
3. Реализовать setter в адаптерах с `NotSupportedException` при не-null/не-default значении.
4. Задокументировать ограничение в `<remarks>`.
5. В `docs/CHANGELOG.md` — раздел `Added` с пометкой «API + `NotSupportedException` в адаптерах».
6. В `docs/ROADMAP.md` — пункт «Реализация X в адаптерах».

**Правило:** сообщение `NotSupportedException` должно содержать имя бэкенда и явное «not supported by this adapter yet».

---

<a id="new-container"></a>
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

**Примеры:** см. `StackLayoutNode`, `GridNode`, `UniformGridNode`, `OverlayNode`, `WrapPanelNode`, `ConditionalNode`, `TransformNode`.

### Особые случаи

- **`ConditionalNode`** — хранит оба поддерева в `Children` всегда, неактивное пропускает в Measure/Arrange. Реализует `IDisposable` для отписки от `Observable<bool>`.
- **`TransformNode`** — `Measure` возвращает bounding box трансформированного ребёнка. В `ApplyRecursive` бросает `NotSupportedException`.
- **`WrapPanelNode`** — Stateless Measure/Arrange (пересчёт при каждом Arrange).

---

<a id="new-widget"></a>
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

**Примеры:** см. `LabelNode`, `ImageNode`, `BackgroundNode`, `ClipNode`.

### Соглашения об именовании методов привязки

| Свойство компонента | Метод привязки |
|---|---|
| `ILabelComponent.Text` | `BindText` |
| `ILabelComponent.Font` | `BindFont` |
| `ILabelComponent.ForegroundBrush` | `BindForegroundBrush` |
| `ILabelComponent.BackgroundBrush` | `BindBackgroundBrush` |
| `IImageComponent.Image` | `BindBitmap` |
| `IImageComponent.SizeMode` | `BindSizeMode` |
| `IRenderComponent.Visible` | `BindVisible` |
| `IEffectComponent.Opacity` | `BindOpacity` |
| `IEffectComponent.Brightness` | `BindBrightness` |
| `IEffectComponent.Contrast` | `BindContrast` |

**История переименований:**
- `BindBrush` → `BindForegroundBrush`.
- `BindFullBrush` → `BindBackgroundBrush`.
- `LabelNodeFluent.FullBrush` → `BackgroundBrush`.
- `UI.Brush(Color)` → `UI.SolidBrush(Color)`.

### `StretchNode`

Публичный узел, растягивающийся вдоль обеих осей. Полезен как flex-ребёнок в `StackLayoutNode`:

```csharp
public sealed class StretchNode : LayoutNode
{
    public int NaturalWidth { get; }
    public int NaturalHeight { get; }

    public StretchNode(int naturalWidth = 0, int naturalHeight = 0)
    {
        NaturalWidth = naturalWidth;
        NaturalHeight = naturalHeight;
        HAlignment = Alignment.Stretch;
        VAlignment = Alignment.Stretch;
    }

    protected override Size MeasureOverride(Size available)
        => new Size(NaturalWidth, NaturalHeight);

    protected override void ArrangeOverride(Rect finalRect) { }
}
```

---

<a id="new-backend"></a>
## Добавление нового бэкенда

См. подробное руководство в `CUSTOM_ADAPTERS_GUIDELINE.md`. Краткий чек-лист:

1. Создайте проект `YourAdapter`.
2. Реализуйте `IWidgetFactory` (создание Label, Image, Masks).
3. Реализуйте `IRenderRootFactory` и `IRenderRoot`.
4. Реализуйте `ILayoutComponent` (корневой контейнер).
5. Реализуйте `ILabelComponent`, `IImageComponent`, `IMaskComponent`, `IEffectComponent`.
6. Реализуйте свойство `Shadow?` — бросает `NotSupportedException` при не-null.
7. `GdiConversions.ToGdi(IBrush)` — бросает `NotSupportedException` для градиентов.
8. `ApplyRecursive` — бросает `NotSupportedException` для `TransformNode`.
9. Адаптеры клонируют ресурсы при установке.
10. Реализуйте `IDisposable` во всех компонентах.
11. Создайте статический класс `Adapter` с методом `Initialize()`.
12. Напишите тесты: `_ClonesOnSet_OriginalNotDisposedByAdapter`, проверка `NotSupportedException`, `Dispose`.

---

<a id="testing"></a>
## Тестирование

### Структура тестов

```
Tests/
├── Core/                  # Тесты контейнеров и типов
├── Gdi/                   # Тесты GDI-обёрток
├── Fluent/                # Тесты fluent-расширений
├── Adapters/              # Тесты адаптеров
└── ...
```

### Правила написания тестов

- Один тест — одна проверка. Используйте `Assert.EnterMultipleScope()` для нескольких утверждений.
- Имена тестов: `MethodName_Condition_ExpectedResult`.
- Обязательно тестируйте:
  - Measure/Arrange для контейнеров.
  - Клонирование ресурсов в адаптерах (`_ClonesOnSet_OriginalNotDisposedByAdapter`).
  - Граничные случаи (null, пустые коллекции, отрицательные размеры).
  - Реактивность: уведомление при изменении `Observable`, отсутствие уведомления при неизменном значении, отписка при `Dispose`.
  - `ObservableList`: multicast delegate не прерывает цепочку при исключении в подписчике.
  - `ComputedObservable`: chained computed, `Refresh`, `Dispose`.
  - `TransformNode`: float-шумы (`Measure_Rotation90` должен давать `50`, не `51`).
- Освобождайте ресурсы в тестах: `(component as IDisposable)?.Dispose()`.

### Пример теста

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

---

<a id="commits"></a>
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
- `style` — форматирование (без изменения логики).

### Область

- `core` — ядро (LayoutNode, контейнеры, типы).
- `rendering` — интерфейсы рендеринга.
- `widgets` — виджеты (Label, Image, Clip, Background).
- `fluent` — UI-фабрика, fluent-расширения.
- `gdi` — GDI+-реализации.
- `adapter` — адаптеры (укажите конкретный: `winforms`, ...).
- `reactive` — `Observable`, `ComputedObservable`, `ObservableList`.
- `effects` — градиенты, Shadow, Transform.
- `playground` — IDE.
- `tests` — тесты.
- `docs` — документация.

### Примеры

```
feat(core): добавить WrapPanelNode для переноса строк

fix(gdi): исправить утечку GDI-handles в GdiTextMeasurer

feat(reactive): добавить ObservableList<T> с событием Changed

docs(kernel): обновить README с примерами GridNode

test(core): добавить тесты для UniformGridNode с Padding

refactor(fluent): вынести BindText в отдельный метод LabelNode

feat(effects): добавить API Shadow (NotSupportedException в адаптерах)

chore(docs): исправить опечатки в EXAMPLES.md
```

---

<a id="pull-requests"></a>
## Pull Requests

### Перед созданием PR

1. Убедитесь, что все тесты проходят.
2. Проверьте, что новый код соответствует стилю проекта.
3. Обновите документацию, если изменился публичный API.
4. Добавьте тесты для новой функциональности.
5. Проверьте, что изменения обратно совместимы (если это не breaking change).
6. Отразите переименования в `docs/CHANGELOG.md` (раздел `Renamed`).

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

---

<a id="checklist"></a>
## Чек-лист перед отправкой

- [ ] Все тесты проходят.
- [ ] Код компилируется без предупреждений.
- [ ] Новый публичный API покрыт XML-документацией.
- [ ] Изменения обратно совместимы (или описаны breaking changes).
- [ ] Ресурсы освобождаются корректно (нет утечек).
- [ ] Потокобезопасность соблюдена (если применимо).
- [ ] Коммиты следуют формату Conventional Commits.
- [ ] Документация обновлена (README, примеры, CHANGELOG).
- [ ] Переименования отражены в `docs/CHANGELOG.md`.
- [ ] Функциональность с `NotSupportedException` задокументирована.
- [ ] В документации не упоминаются закрытые легаси-проекты.
- [ ] Новые контейнеры/виджеты имеют `sealed override Arrange` (для контейнеров).
- [ ] Новые реактивные классы вызывают подписчиков через `GetInvocationList()`.