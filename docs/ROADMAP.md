# План развития DisplayNodes

Документ описывает **нереализованные** задачи и направления развития проекта.

Реализованные пункты перенесены в [CHANGELOG.md](CHANGELOG.md).

---

## 1. Система событий (Input)

**Приоритет:** Низкий

**Мотивация:**
Текущая архитектура не поддерживает обработку пользовательского ввода (клики, наведение, клавиатура). Это ограничивает DisplayNodes ролью статического layout-движка. Для интерактивного UI необходима система событий.

**Предполагаемый API:**

```csharp
public interface IInputComponent
{
    event Action<Point> Click;
    event Action<Point> DoubleClick;
    event Action<Point> MouseDown;
    event Action<Point> MouseUp;
    event Action<Point> MouseMove;
    event Action MouseEnter;
    event Action MouseLeave;
    event Action<KeyEventArgs> KeyDown;
    event Action<KeyEventArgs> KeyUp;
    bool HitTest(Point point);
}

// Fluent
UI.Label("Click me", font, brush)
    .OnClick(p => Console.WriteLine($"Clicked at {p}"))
    .OnMouseEnter(() => Console.WriteLine("Hover"));
```

**Архитектурные решения:**
1. События должны всплывать от ребёнка к родителю (bubbling), как в DOM/WPF.
2. `HitTest` определяет, попадает ли точка в границы компонента (с учётом масок и трансформаций).
3. Адаптеры транслируют нативные события в абстрактные `IInputComponent` события.

**Сложность:** Высокая. Требует нового интерфейса, модификации `WidgetNode` и `LayoutNode`, реализации hit-testing в адаптерах, и механизма bubbling.

---

## 2. ButtonNode

**Приоритет:** Низкий (зависит от системы событий)

**Мотивация:**
Интерактивная кнопка с состояниями (normal, hover, pressed, disabled). Базовый элемент любого UI. В текущей реализации кнопки имитируются через `LabelNode` с фоном, но не реагируют на клики.

**Предполагаемый API:**

```csharp
public class ButtonNode : WidgetNode
{
    public string Text { get; set; }
    public IFont Font { get; set; }
    public IBrush ForegroundBrush { get; set; }
    public IBrush BackgroundBrush { get; set; }
    public IBrush HoverBrush { get; set; }
    public IBrush PressedBrush { get; set; }
    public bool IsEnabled { get; set; }

    public event Action Click;
}

// Fluent
UI.Button("Submit", font, whiteBrush)
    .Background(Color.FromArgb(0, 120, 215))
    .HoverBackground(Color.FromArgb(0, 100, 195))
    .OnClick(() => Console.WriteLine("Submitted!"));
```

**Зависимости:** Требует систему событий.

**Сложность:** Средняя (при наличии системы событий). Основная работа — управление визуальными состояниями и транслирование кликов через адаптер.

---

## 3. TextInputNode

**Приоритет:** Низкий (зависит от системы событий)

**Мотивация:**
Поле ввода текста с поддержкой каретки, выделения, копирования/вставки. Критически важный элемент для форм и интерактивных приложений. В текущей реализации ввод текста невозможен.

**Предполагаемый API:**

```csharp
public class TextInputNode : WidgetNode
{
    public string Text { get; set; }
    public string Placeholder { get; set; }
    public IFont Font { get; set; }
    public IBrush ForegroundBrush { get; set; }
    public IBrush PlaceholderBrush { get; set; }
    public bool IsReadOnly { get; set; }
    public int MaxLength { get; set; }
    public char PasswordChar { get; set; }  // '\0' = обычный режим

    public event Action<string> TextChanged;
    public event Action Enter;
}

// Fluent
var input = UI.TextInput(font, brush)
    .Placeholder("Enter your name...")
    .MaxLength(100)
    .OnTextChanged(text => Console.WriteLine($"Input: {text}"))
    .OnEnter(() => Console.WriteLine("Submitted"));

// Двусторонняя привязка
var name = new Observable<string>("");
input.BindText(name);
```

**Зависимости:** Требует систему событий и, желательно, `Observable<string>` для двусторонней привязки.

**Сложность:** Очень высокая. Реализация текстового ввода с кареткой, выделением, скроллингом, IME-поддержкой и clipboard — одна из самых сложных задач в UI-фреймворках. Альтернативный подход для первого этапа: делегирование нативному контролу адаптера (WinForms `TextBox`, GDI+ кастомный рендеринг).

---

## 4. Реализация градиентов в адаптерах

**Приоритет:** Средний

**Мотивация:**
API градиентных кистей (`GdiLinearGradientBrush`, `GdiRadialGradientBrush`, `IBrushFactory.CreateLinearGradient`/`CreateRadialGradient`) реализован, но адаптеры **не поддерживают** их — `GdiConversions.ToGdi(IBrush)` бросает `NotSupportedException`.

**Задача:**
1. В `WinFormsAdapter` — переопределить `OnPaint` в `Label`/`Image` для отрисовки градиента под текстом.
2. Учесть маски (`ClipNode`) — градиент должен обрезаться.

**Сложность:** Средняя. Требует изменения логики отрисовки в компонентах адаптеров.

---

## 5. Реализация тени (Shadow) в адаптерах

**Приоритет:** Низкий

**Мотивация:**
`Shadow` описан в API (`IEffectComponent.Shadow?`), но адаптеры **бросают `NotSupportedException`** при попытке установки.

**Задача:**
1. Реализовать размытие тени через `ColorMatrix` + `Bitmap`-манипуляции (GDI+ не имеет встроенной поддержки `DropShadowEffect`).
2. Отрисовывать тень под компонентом с учётом `OffsetX`, `OffsetY`, `BlurRadius`, `Color`.
3. Обработать края: если тень выходит за `Bounds` — обрезать по краю компонента.

**Сложность:** Высокая. Требует кастомной реализации размытия, обработки прозрачности и оптимизации (кэширование теней).

---

## 6. Реализация TransformNode в адаптерах

**Приоритет:** Низкий

**Мотивация:**
`TransformNode` корректно работает в layout (вычисляет bounding box), но `ApplyRecursive` **бросает `NotSupportedException`** при обнаружении `TransformNode` — адаптеры не поддерживают трансформации.

**Задача:**
1. Передавать `Transform` от `TransformNode` в `IRenderComponent` (расширить интерфейс свойством `Transform?`).
2. В адаптерах применять `Graphics.Transform` при отрисовке компонента.
3. Для WinForms — применять трансформацию в `OnPaint`.
4. Трансформировать точку `HitTest` (когда появится система событий).

**Сложность:** Высокая. Требует изменения `IRenderComponent`, реализации трансформаций во всех адаптерах, обработки `Graphics.Save`/`Restore`.

---

## 7. Hot-swap поддеревьев (для `ConditionalNode` и `RepeaterNode`)

**Приоритет:** Средний

**Мотивация:**
`ConditionalNode` **не пересчитывает layout автоматически** при переключении `Observable<bool>`. Пользователь должен вызвать `Measure`+`Arrange`+`Refresh` вручную или перестроить дерево целиком через `DisplayRoot.Build`.

Аналогично — `ObservableList<T>` не имеет UI-интеграции (`RepeaterNode` отложен).

**Задача:**
1. Разработать механизм перестроения **части** дерева в runtime.
2. `ConditionalNode` при `ConditionChanged` должен пересчитать `Measure`/`Arrange` для себя и уведомить родителя.
3. `RepeaterNode` — новый контейнер, подписанный на `ObservableList<T>`, автоматически создающий/удаляющий дочерние `LayoutNode` при изменениях.

**Подводные камни:**
- Компоненты неактивного поддерева в `ConditionalNode` существуют, но не рендерятся.
- `RepeaterNode` требует разделения «логического» (создание `LayoutNode`) и «физического» (создание `IRenderComponent`) слоёв.
- Hot-swap затронет `Apply`/`Rebuild`/`Dispose` в `LayoutNodeFluent`.

**Сложность:** Высокая. Существенное изменение архитектуры.

---

## 8. Batch-обновления `ComputedObservable`

**Приоритет:** Низкий

**Мотивация:**
При изменении нескольких зависимостей в одном «тике» `ComputedObservable<T>` пересчитывается **несколько раз**. Пример:

```csharp
a.Value = 1;  // → compute() вызван
b.Value = 2;  // → compute() вызван снова
```

Оба пересчёта дадут корректный результат, но второй — избыточен.

**Задача:**
1. Ввести механизм отложенного пересчёта через `Dispatcher`/`Scheduler`.
2. `ComputedObservable<T>` при изменении зависимостей помечает себя «грязным», но `compute()` вызывается в конце тика.
3. API: `ComputedObservable.BatchMode` (глобальный) или `Observable.BeginUpdate()`/`EndUpdate()`.

**Сложность:** Средняя. Основная работа — координация нескольких `Observable` в одном «тике».

---

## Сводная таблица

| N  | Пункт | Приоритет | Сложность | Зависимости |
|----|---|---|---|---|
| 1  | Система событий (Input) | Низкий | Высокая | Нет |
| 2  | ButtonNode | Низкий | Средняя | 1 |
| 3  | TextInputNode | Низкий | Очень высокая | 1 |
| 4  | Градиенты в адаптерах | Средний | Средняя | Нет |
| 5  | Shadow в адаптерах | Низкий | Высокая | Нет |
| 6  | TransformNode в адаптерах | Низкий | Высокая | Нет |
| 7  | Hot-swap поддеревьев | Средний | Высокая | Нет |
| 8  | Batch-обновления ComputedObservable | Низкий | Средняя | Нет |

## Рекомендуемый порядок реализации

**Фаза A — доработки адаптеров (без изменения ядра):**
1. Градиенты в адаптерах
2. Shadow в адаптерах
3. TransformNode в адаптерах

**Фаза B — реактивность и layout:**
4. Hot-swap поддеревьев (для `ConditionalNode` и `RepeaterNode`)
5. Batch-обновления `ComputedObservable`

**Фаза C — интерактивность (отложена на поздний срок):**
6. Система событий (Input)
7. ButtonNode
8. TextInputNode

Интерактивность перенесена в последнюю фазу, поскольку текущая архитектура DisplayNodes ориентирована на
декларативное описание статического UI. Переход к интерактивным элементам потребует существенного
пересмотра ядра: добавления системы событий, механизма hit-testing, обработки фокуса и клавиатурного ввода.