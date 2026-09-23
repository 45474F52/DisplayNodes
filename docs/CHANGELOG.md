# DisplayNodes Changelog

Все значимые изменения проекта DisplayNodes документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/),
проект придерживается [Semantic Versioning](https://semver.org/lang/ru/).

---

## [Unreleased]

Реализация ROADMAP.md — фазы 1–4 (без системы событий).

### Added

#### Layout (ограничения размеров)

- **`LayoutNode.MinWidth`** (`int?`) — минимальная ширина. Применяется в `Measure` после `MeasureOverride`.
- **`LayoutNode.MaxWidth`** (`int?`) — максимальная ширина.
- **`LayoutNode.MinHeight`** (`int?`) — минимальная высота.
- **`LayoutNode.MaxHeight`** (`int?`) — максимальная высота.
- **Fluent-расширения** в `LayoutNodeFluent`:
  - `.MinWidth(int)`, `.MaxWidth(int)`, `.MinHeight(int)`, `.MaxHeight(int)`
  - `.WidthRange(min, max)`, `.HeightRange(min, max)`
- Тесты: `Tests/Core/LayoutNodeConstraintsTests.cs`.

#### Layout (Flex/Weight)

- **`LayoutNode.FlexWeight`** (`double`, default `0`) — коэффициент гибкости вдоль главной оси `StackLayoutNode`.
- **`StackLayoutNode.MeasureOverride`/`ArrangeOverride`** переработаны: поддержка flex-детей. При `free > 0` flex-дети растягиваются пропорционально весам, при `free < 0` — сжимаются. Fixed-дети не сжимаются.
- **Fluent-расширение** `.Flex(double weight = 1)` в `LayoutNodeFluent`.
- **`StretchNode`** — публичный узел, растягивающийся вдоль обеих осей. Полезен как flex-ребёнок.
- Тесты: `Tests/Core/StackLayoutFlexTests.cs`.

#### Layout (WrapPanel)

- **`WrapPanelNode`** — новый контейнер с автоматическим переносом детей на следующую строку/столбец.
  - Свойства: `Direction` (`WrapDirection.Horizontal`/`Vertical`), `Spacing`, `LineSpacing`.
  - `sealed override Arrange` — растягивается на весь слот.
  - Stateless Measure/Arrange (пересчёт при каждом Arrange).
- **`WrapDirection`** — enum направлений переноса.
- **Fluent:** `UI.WrapPanel(direction, spacing, lineSpacing)`, `.Direction(WrapDirection)`.
- Тесты: `Tests/Core/WrapPanelNodeTests.cs`.

#### Layout (Border)

- **`UI.Border(IBrush, int cornerRadius = 0)`** — композиция `OverlayNode` + `ClipNode` + `BackgroundNode`.
- **`UI.Border(Color, int cornerRadius = 0)`** — перегрузка для удобства.
- **`BackgroundNode(IBrush, ILabelComponent)`** — новый конструктор.
- **`UI.Background(IBrush)`** — новая перегрузка.
- Тесты: `Tests/Fluent/BorderTests.cs`.

#### Реактивность (ComputedObservable)

- **`IObservableSource`** — не-generic интерфейс для подписки на `Observable<T>` с разными `T`.
- **`Observable<T>`** реализует `IObservableSource` (явная реализация через `Action<object>`).
- **`ComputedObservable<T>`** — реактивное свойство, значение которого вычисляется из других источников.
  - Конструктор: `ComputedObservable(Func<T> compute, params IObservableSource[] deps)`.
  - Метод `Refresh()` для ручного пересчёта.
  - Реализует `IDisposable` — отписывается от зависимостей.
- Тесты: `Tests/Core/ComputedObservableTests.cs`.

#### Реактивность (ObservableList)

- **`ObservableList<T>`** — реактивная коллекция, реализующая `IList<T>`.
  - Потокобезопасна (внутренний `lock`).
  - Событие `Changed` вызывается **вне lock'а**.
  - Все подписчики вызываются **отдельно** через `GetInvocationList()` — исключение в одном не ломает цепочку.
  - `Dispose` делает коллекцию непригодной: любая операция бросает `ObjectDisposedException`.
  - `GetEnumerator` возвращает enumerator `List<T>` (`foreach` бросает `InvalidOperationException` при модификации, как в `List<T>`).
  - `ToList()` — безопасный снимок для перебора с модификацией.
  - Дополнительно к `IList<T>`: `Move(oldIndex, newIndex)`.
- **`ListChangeType`** — enum типов изменений: `Add`, `Insert`, `Remove`, `Replace`, `Move`, `Reset`.
- **`ListChange<T>`** — readonly struct с полями `Type`, `OldIndex`, `NewIndex`, `Item`.
- Тесты: `Tests/Core/ObservableListTests.cs`.

#### Реактивность (ConditionalNode)

- **`ConditionalNode`** — контейнер, отображающий одно из двух поддеревьев по `Observable<bool>`.
  - Оба поддерева (`TrueNode`, `FalseNode`) хранятся в `Children` всегда.
  - Неактивное поддерево пропускается в `Measure`/`Arrange`.
  - `ActiveNode` — публичное свойство для чтения текущего активного поддерева.
  - `sealed override Arrange` — растягивается на весь слот.
  - `IDisposable` — отписывается от `Observable<bool>`.
- **Fluent:** `UI.When(condition, trueNode, falseNode = null)`, `.OnChanged(Action<bool>)`.
- Тесты: `Tests/Core/ConditionalNodeTests.cs`.

#### Визуальные эффекты (градиенты)

- **`Percent`** — readonly struct для процентных значений (0..100) с валидацией.
- **`GradientStop`** — readonly struct (Color + Percent Offset).
- **`IBrushFactory.CreateLinearGradient(Point start, Point end, params GradientStop[] stops)`** — линейный градиент.
- **`IBrushFactory.CreateRadialGradient(Point center, Percent radius, params GradientStop[] stops)`** — радиальный градиент.
- **`GdiLinearGradientBrush`** — описание линейного градиента (нормализованные координаты 0..100).
- **`GdiRadialGradientBrush`** — описание радиального градиента.
- **`GdiBrushFactory`** реализует новые методы.
- **`GdiConversions.CreateGdiBrush(...)`** — extension'ы для создания конкретных GDI+ кистей из описания под заданный `RectangleF`.
- **Fluent:** `UI.LinearGradient(...)`, `UI.RadialGradient(...)`.
- **`GdiConversions.ToGdi(IBrush)`** теперь бросает `NotSupportedException` для градиентных кистей — адаптеры не поддерживают градиенты.
- Тесты: `Tests/Core/PercentTests.cs`, `Tests/Core/GradientStopTests.cs`, `Tests/Gdi/GdiBrushFactoryGradientTests.cs`, `Tests/Gdi/GdiConversionsTests.cs`.

#### Визуальные эффекты (Shadow)

- **`Shadow`** — readonly struct (OffsetX, OffsetY, BlurRadius, Color).
- **`IEffectComponent.Shadow?`** — nullable свойство тени.
- **Fluent:** `.Shadow(Shadow)`, `.Shadow(offsetX, offsetY, blurRadius, color)`, `.Shadow(offsetX, offsetY, blurRadius)` в `LayoutNodeFluent`.
- Адаптеры **бросают `NotSupportedException`** при попытке установить тень.
- Тесты: `Tests/Core/ShadowTests.cs`, `Tests/Fluent/ShadowFluentTests.cs`.

#### Визуальные эффекты (TransformNode)

- **`Transform`** — readonly struct (ScaleX, ScaleY, Rotation, SkewX, SkewY, Origin).
- **`TransformNode`** — контейнер, применяющий аффинное преобразование к единственному ребёнку.
  - **Учитывается в layout**: `Measure` возвращает bounding box трансформированного ребёнка.
  - Порядок трансформации: scale → skew → rotate, относительно `Origin`.
  - `ComputeBoundingBox` — полная формула с учётом origin и skew.
  - `RoundNearInteger` — нейтрализует микроскопические шумы `float`-математики (например, `cos(π/2) ≈ -4.37e-8` вместо `0`).
  - `sealed override Arrange`.
  - Свойство `Child` (get/set) — единственный дочерний узел.
- **`UI.Transform(scaleX, scaleY, rotation)`** — fluent-конструктор.
- **`UI.Transform(scaleX, scaleY, rotation, origin)`** — с явным origin.
- **`ApplyRecursive`** в `LayoutNodeFluent` бросает `NotSupportedException` при обнаружении `TransformNode` — адаптеры не поддерживают трансформации.
- Тесты: `Tests/Core/TransformTests.cs`, `Tests/Core/TransformNodeTests.cs`, `Tests/Fluent/TransformNodeApplyTests.cs`.

#### Playground (Tooltip с сигнатурой в автодополнении)

- **`CompletionItem`** — расширенная модель элемента автодополнения (Name, Signature, ReturnType, Documentation, Kind).
- **`CompletionItemKind`** — enum: `Method`, `Property`, `Field`, `Type`.
- **`XmlDocProvider`** — загрузка XML-документации из `.xml`-файлов рядом со сборками. Поддерживает `<see cref>`, `<paramref>`, нормализацию whitespace.
- **`ApiIndex`** переработан:
  - `Initialize(XmlDocProvider, params Assembly[])` — новый параметр.
  - `GetMembers` возвращает `List<CompletionItem>` вместо `List<string>`.
  - Собирает поля (`FieldInfo`) в дополнение к методам и свойствам.
  - Строит сигнатуры методов с параметрами и типами.
  - Форматирует типы по правилам C# (`int`, `string`, `List<T>`, `T[]`).
- **`CompletionTooltip`** — Popup-окно с сигнатурой, типом возврата и XML-summary. Позиционируется с учётом краёв экрана (рабочая область монитора).
- **`CompletionPanel`** — интеграция tooltip'а, `HideSelection = false` (видимое выделение без фокуса), `Measure(item)` для позиционирования.

#### Playground (Экспорт preview)

- **`AppForm.ExportPreview()`** — сохранение текущего preview в файл (PNG/JPEG/BMP).
- **Пункт меню** «Экспорт preview...» в `BtnFile` с горячей клавишей **`Ctrl+E`**.
- **`GetImageFormatFromPath`** — определение `ImageFormat` по расширению.

#### Playground (Абстракция preview)

- **`IPreviewHost`** — абстракция области предпросмотра, инкапсулирующая работу с конкретным адаптером.
  - `CurrentSize`, `Build(LayoutNode)`, `Resize(Size)`, `Clear()`, `ExportToBitmap()`.
- **`WinFormsPreviewHost`** — реализация для `WinFormsAdapter` (рендерит дерево как WinForms-контролы).

### Changed

- **`LayoutNode.Measure`** — clamp `DesiredSize` по `MinWidth`/`MaxWidth`/`MinHeight`/`MaxHeight` после `MeasureOverride`.
- **`StackLayoutNode`** — переработаны `MeasureOverride`/`ArrangeOverride` для поддержки flex. Удалён вспомогательный метод `ComputeChildMainSizes`.
- **`StackLayoutNode.Arrange`** — восстановлен `sealed override`, потерянный при переписывании. Без него `MainAxisAlignment` работал в уменьшенном слоте.
- **`FixedNode`** — `HAlignment = Start`, `VAlignment = Start` в конструкторе. Теперь `FixedNode` **не растягивается** по умолчанию.
- **`GridNodeTests`** — заменены `FixedNode` на `StretchNode` в тестах распределения колонок.
- **`IEffectComponent`** — добавлено свойство `Shadow? Shadow { get; set; }`.
- **`BackgroundNode`** — конструктор `(Color, ILabelComponent, Func<Color, IBrush>)` делегирует в новый `(IBrush, ILabelComponent)`.
- **`UI.Background(Color)`** — использует `UI.SolidBrush(color)` вместо передачи фабрики.
- **`AppForm`** — рефакторинг под `IPreviewHost`: убраны прямые зависимости от `DisplayRoot`, `RenderRootFactory`, `ManagerDisplays`, `ManagerTimers`.

### Renamed

- **`UI.Brush(Color)` → `UI.SolidBrush(Color)`** — метод `UI` для создания сплошной кисти переименован для однозначности (отличать от `LinearGradient`/`RadialGradient`).
- **`LabelNodeFluent.FullBrush(...)` → `LabelNodeFluent.BackgroundBrush(...)`** — метод переименован в соответствии с названием свойства `ILabelComponent.BackgroundBrush`.
- **`LabelNode.BindBrush(...)` → `LabelNode.BindForegroundBrush(...)`** — метод переименован в соответствии с названием свойства `ILabelComponent.ForegroundBrush`.
- **`LabelNode.BindFullBrush(...)` → `LabelNode.BindBackgroundBrush(...)`** — метод переименован в соответствии с названием свойства `ILabelComponent.BackgroundBrush`.

### Fixed

- **`ObservableList<T>.RaiseChanged`** — вызов подписчиков через `GetInvocationList()` вместо прямого multicast-вызова. Исключение в одном подписчике больше не прерывает остальных.
- **`TransformNode.ComputeBoundingBox`** — `RoundNearInteger` для нейтрализации float-шумов (`Measure_Rotation90` давал `51` вместо `50`).
- **`CompletionTooltip`** — заменён `ToolStripDropDown` на `Form` с `WS_EX_NOACTIVATE`: tooltip не забирает фокус, ввод текста и навигация стрелками в редакторе работают при открытом tooltip'е.

### Security

Нет.

---

## Итоги ROADMAP

| #  | Пункт                                | Статус | Примечание |
|----|--------------------------------------|--------|------------|
| 1  | Ограничения размеров                 | ✅     | — |
| 2  | Flex/Weight в StackLayoutNode        | ✅     | — |
| 3  | WrapPanelNode                        | ✅     | — |
| 4  | BorderNode                           | ✅     | Через `UI.Border` (композиция) |
| 5  | ComputedObservable                   | ✅     | — |
| 6  | ObservableList\<T\>                  | ✅     | — |
| 7  | ConditionalNode / When               | ✅     | Без hot-swap поддеревьев |
| 8  | Градиентные кисти                    | ✅     | API + `NotSupportedException` в адаптерах |
| 9  | Tooltip с сигнатурой                 | ✅     | Popup с учётом краёв экрана |
| 10 | Тень (Shadow)                        | ✅     | API + `NotSupportedException` |
| 11 | TransformNode                        | ✅     | Layout работает; `NotSupportedException` при Apply |
| 12 | Экспорт preview                      | ✅     | `Ctrl+E`, PNG/JPEG/BMP |
| 13 | Система событий (Input)              | ⏸     | Отложено (см. [ROADMAP.md](ROADMAP.md)) |
| 14 | ButtonNode                           | ⏸     | Отложено (зависит от 13) |
| 15 | TextInputNode                        | ⏸     | Отложено (зависит от 13) |

Актуальный план развития — в [ROADMAP.md](ROADMAP.md).

---

## Известные ограничения

### Адаптеры не поддерживают

- **Градиентные кисти** — бросается `NotSupportedException` при установке в компонент.
- **Тень** — бросается `NotSupportedException`.
- **Трансформации** — `ApplyRecursive` бросает `NotSupportedException` при обнаружении `TransformNode`.

### Layout

- **`ConditionalNode`** не пересчитывает layout автоматически при переключении условия. Пользователь должен вызвать `Measure`+`Arrange`+`Refresh` вручную или перестроить дерево.
- **`ObservableList<T>`** — UI-интеграция (`RepeaterNode`) **отложена** до реализации hot-swap поддеревьев.

### Playground

- **Экспорт preview** сохраняет в размере `picPreview`, другой размер не поддерживается.
- **XML-doc** читается только из своих сборок (BCL не поддерживается).

---

## Внутренние улучшения

- **Тесты:** десятки новых NUnit-тестов на все новые фичи.
- **`StretchNode`** — публичный класс в `Kernel/Core/StretchNode.cs` для тестов flex-механики.
- **`IPreviewHost`** — абстракция preview в Playground, упрощающая переключение между адаптерами.
- **`CompletionTooltip`** использует `Form` с `WS_EX_NOACTIVATE` — не крадёт фокус, не закрывает `CompletionPanel`.
