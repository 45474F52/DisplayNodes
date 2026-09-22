# План развития DisplayNodes

Элементы сгруппированы по приоритету и тематике. Каждый пункт содержит описание задачи, мотивацию, предполагаемый API и оценку сложности.

---

## 1. Ограничения размеров (MinWidth, MaxWidth, MinHeight, MaxHeight)

**Приоритет:** Высокий

**Мотивация:**
В текущей реализации `LayoutNode` не поддерживает ограничения минимального и максимального размера. Это приводит к тому, что виджеты и контейнеры могут сжиматься до нуля или растягиваться до бесконечности при экстремальных значениях `available`. WPF, Flutter и CSS предоставляют `MinWidth`/`MaxWidth`/`MinHeight`/`MaxHeight` как базовые свойства любого элемента.

**Предполагаемый API:**

```csharp
public abstract class LayoutNode
{
    public int? MinWidth { get; set; }
    public int? MaxWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxHeight { get; set; }
}
```

**Fluent-расширения:**

```csharp
UI.Label("Text", font, brush)
    .MinWidth(100)
    .MaxWidth(400)
    .MinHeight(30)
    .MaxHeight(200);
```

**Влияние на алгоритм:**
Ограничения применяются в базовом методе `Measure` после вызова `MeasureOverride`:

```csharp
public Size Measure(Size available)
{
    var inner = new Size(
        Math.Max(0, available.Width - Margin.Horizontal),
        Math.Max(0, available.Height - Margin.Vertical)
    );
    DesiredSize = MeasureOverride(inner);

    // Применение ограничений
    int w = DesiredSize.Width;
    int h = DesiredSize.Height;
    if (MinWidth.HasValue)  w = Math.Max(w, MinWidth.Value);
    if (MaxWidth.HasValue)  w = Math.Min(w, MaxWidth.Value);
    if (MinHeight.HasValue) h = Math.Max(h, MinHeight.Value);
    if (MaxHeight.HasValue) h = Math.Min(h, MaxHeight.Value);
    DesiredSize = new Size(w, h);

    return new Size(DesiredSize.Width + Margin.Horizontal, DesiredSize.Height + Margin.Vertical);
}
```

**Сложность:** Низкая. Изменения локализованы в `LayoutNode.Measure`. Не требует правок в адаптерах.

---

## 2. Flex/Weight в StackLayoutNode

**Приоритет:** Высокий

**Мотивация:**
Текущий `StackLayoutNode` распределяет свободное пространство только через `MainAxisAlignment.SpaceBetween`, что добавляет равные промежутки между детьми. Нет возможности задать пропорциональное распределение свободного пространства между отдельными детьми (аналог `flex` в CSS или `Expanded`/`Flexible` во Flutter).

**Предполагаемый API:**

```csharp
// Свойство на LayoutNode
public double FlexWeight { get; set; } = 0;

// Fluent
UI.Column(8)
    .Add(UI.Label("Fixed", font, brush))               // FlexWeight = 0, обычный размер
    .Add(UI.Label("Flexible", font, brush).Flex(1))     // забирает 1 долю свободного места
    .Add(UI.Label("Double", font, brush).Flex(2));      // забирает 2 доли
```

**Алгоритм:**
1. Первый проход Measure: измерить всех детей с `FlexWeight == 0` как обычно. Детей с `FlexWeight > 0` измерить с `Size(0, 0)` по главной оси (они получат размер позже).
2. Вычислить свободное пространство: `free = available - sum(fixed children) - spacing`.
3. Второй проход: распределить `free` пропорционально `FlexWeight`. Каждый flex-ребёнок получает `free * (weight / totalWeight)`.
4. Переизмерить flex-детей с выделенным пространством.

**Сложность:** Средняя. Требует двухпроходного Measure внутри `StackLayoutNode.MeasureOverride`. Необходимо корректно обработать случай, когда `free < 0` (дети не помещаются).

---

## 3. WrapPanelNode

**Приоритет:** Средний

**Мотивация:**
Контейнер с автоматическим переносом детей на следующую строку при нехватке места по горизонтали. Аналог `WrapPanel` в WPF и `Wrap` во Flutter. Необходим для отображения тегов, чипсов, галерей миниатюр.

**Предполагаемый API:**

```csharp
public class WrapPanelNode : LayoutNode
{
    public int HorizontalSpacing { get; set; }
    public int VerticalSpacing { get; set; }
    public WrapDirection Direction { get; set; } = WrapDirection.Horizontal;
}

public enum WrapDirection
{
    Horizontal,  // строки слева направо, перенос вниз
    Vertical     // столбцы сверху вниз, перенос вправо
}

// Fluent
UI.WrapPanel(horizontalSpacing: 8, verticalSpacing: 8)
    .Add(UI.Label("Tag 1", font, brush))
    .Add(UI.Label("Tag 2", font, brush))
    .Add(UI.Label("Tag 3", font, brush));
```

**Алгоритм Measure:**
1. Итерация по детям. Для каждого ребёнка вызывается `Measure(available)`.
2. Если ребёнок помещается в текущую строку (`currentRowWidth + child.Width + spacing <= available.Width`), добавляется в строку.
3. Если не помещается, начинается новая строка: `totalHeight += rowHeight + verticalSpacing`, `currentRowWidth = child.Width`.
4. Итоговый размер: `max(rowWidths) x totalHeight`.

**Сложность:** Средняя. Алгоритм аналогичен `StackLayoutNode`, но с дополнительной логикой переноса. Требует хранения промежуточных результатов (позиции строк) для Arrange.

---

## 4. BorderNode

**Приоритет:** Средний

**Мотивация:**
Контейнер, добавляющий визуальную рамку и фон вокруг единственного дочернего элемента. Аналог `Border` в WPF. В текущей реализации для рамки приходится использовать вложенные `OverlayNode` + `BackgroundNode`, что громоздко.

**Предполагаемый API:**

```csharp
public class BorderNode : LayoutNode
{
    public Thickness BorderThickness { get; set; }
    public Color BorderColor { get; set; }
    public Color BackgroundColor { get; set; }
    public float CornerRadius { get; set; }
}

// Fluent
UI.Border(borderThickness: new Thickness(2), borderColor: Color.Gray, cornerRadius: 8f)
    .Add(UI.Label("Content", font, brush).Padding(12));
```

**Требования к адаптерам:**
`BorderNode` требует нового компонента рендерера (`IBorderComponent`) или использования существующего `ILabelComponent` с расширенными свойствами. Альтернативный подход — декомпозиция в `OverlayNode` + `BackgroundNode` + `ClipNode` на уровне ядра, без новых интерфейсов.

**Сложность:** Средняя. Если реализовывать через декомпозицию — низкая. Если через новый компонент — высокая (требует правок во всех адаптерах).

---

## 5. ComputedObservable

**Приоритет:** Средний

**Мотивация:**
Реактивное свойство, значение которого вычисляется на основе одного или нескольких других `Observable`. Избавляет от ручного управления подписками при создании производных значений. Аналог `computed` в Vue.js, `derived` в Svelte, `Select` в Rx.NET.

**Предполагаемый API:**

```csharp
public class ComputedObservable<T> : Observable<T>
{
    public ComputedObservable(Func<T> compute, params Observable<object>[] dependencies);
}

// Использование
var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName, lastName
);

// fullName.Value == "Ivan Petrov"
firstName.Value = "Petr";
// fullName.Value == "Petr Petrov" (автоматически пересчитано)
```

**Алгоритм:**
1. При создании подписывается на все зависимости.
2. При изменении любой зависимости пересчитывает значение через `compute()`.
3. Если новое значение отличается от текущего, уведомляет своих подписчиков.
4. При `Dispose` отписывается от всех зависимостей.

**Сложность:** Средняя. Основная сложность — корректная обработка циклических зависимостей и множественных обновлений в одном тике.

---

## 6. ObservableList<T>

**Приоритет:** Средний

**Мотивация:**
Реактивная коллекция, уведомляющая об изменениях (добавление, удаление, замена, перемещение, полная очистка). Необходима для динамических списков, где количество элементов меняется в runtime. Текущий `Observable<T>` работает только со скалярными значениями.

**Предполагаемый API:**

```csharp
public class ObservableList<T> : IList<T>, IDisposable
{
    public event Action<ListChangeType, int, T> Changed;
    
    public void Add(T item);
    public void RemoveAt(int index);
    public void Insert(int index, T item);
    public void Move(int oldIndex, int newIndex);
    public void Clear();
}

public enum ListChangeType
{
    Add,
    Remove,
    Replace,
    Move,
    Reset
}

// Использование
var items = new ObservableList<string>();
items.Add("Item 1");  // уведомит: Add, 0, "Item 1"
items.RemoveAt(0);    // уведомит: Remove, 0, "Item 1"
```

**Интеграция с UI:**
Потребует нового контейнера `RepeaterNode` или `ListViewNode`, который подписывается на `ObservableList` и автоматически создаёт/удаляет дочерние `LayoutNode` при изменениях.

**Сложность:** Высокая. Требует реализации `IList<T>`, потокобезопасности, и нового механизма перестроения поддеревьев.

---

## 7. ConditionalNode / When

**Приоритет:** Средний

**Мотивация:**
Условное отображение узлов в зависимости от значения `Observable<bool>`. Позволяет декларативно описывать логику "показать/скрыть" без ручного управления `Visible`. Аналог `v-if` в Vue, `{#if}` в Svelte, `Conditional` в SwiftUI.

**Предполагаемый API:**

```csharp
public class ConditionalNode : LayoutNode
{
    public ConditionalNode(Observable<bool> condition, LayoutNode trueNode, LayoutNode falseNode = null);
}

// Fluent
var isLoggedIn = new Observable<bool>(false);

UI.Column(8)
    .Add(UI.When(isLoggedIn,
        trueNode: UI.Label("Welcome!", font, brush),
        falseNode: UI.Label("Please log in", font, grayBrush)));
```

**Алгоритм:**
1. Подписывается на `condition`.
2. При `true` — Measure/Arrange для `trueNode`, `falseNode` скрыт.
3. При `false` — Measure/Arrange для `falseNode` (если задан), `trueNode` скрыт.
4. При изменении условия — перестроение дерева.

**Сложность:** Средняя. Основная сложность — корректное управление жизненным циклом компонентов (создание/уничтожение при переключении).

---

## 8. Градиентные кисти

**Приоритет:** Средний

**Мотивация:**
Текущая реализация поддерживает только сплошные кисти (`SolidBrush`). Градиенты необходимы для современного UI: фоны кнопок, карточек, заголовков.

**Предполагаемый API:**

```csharp
public interface IBrushFactory
{
    IBrush CreateSolidBrush(Color color);
    IBrush CreateLinearGradient(Point start, Point end, Color startColor, Color endColor);
    IBrush CreateRadialGradient(Point center, float radius, Color innerColor, Color outerColor);
}

// Fluent
UI.Background(UI.BrushFactory.CreateLinearGradient(
    new Point(0, 0), new Point(0, 100),
    Color.Blue, Color.White));
```

**Требования к адаптерам:**
Каждый адаптер должен реализовать создание градиентных кистей через свой бэкенд (GDI+ `LinearGradientBrush`, `PathGradientBrush` и т.д.).

**Сложность:** Средняя. Ядро — низкая (новые методы в `IBrushFactory`). Адаптеры — средняя (реализация градиентов через GDI+).

---

## 9. Tooltip с сигнатурой в CompletionPanel

**Приоритет:** Средний (для Playground)

**Мотивация:**
Текущий `CompletionPanel` показывает только имена методов и свойств. При выборе элемента из списка пользователь не видит сигнатуру метода, тип возвращаемого значения или XML-документацию. Это затрудняет работу с API DisplayNodes.

**Предполагаемый API:**

```csharp
// CompletionPanel получает расширенные данные
internal sealed class CompletionItem
{
    public string Name { get; set; }
    public string Signature { get; set; }      // "Measure(Size available) : Size"
    public string ReturnType { get; set; }      // "Size"
    public string Documentation { get; set; }   // XML-doc summary
    public CompletionItemKind Kind { get; set; } // Method, Property, Field, Type
}
```

**Реализация:**
1. Расширить `ApiIndex` для сбора сигнатур и XML-документации через рефлексию (`MethodInfo.GetParameters()`, `MemberInfo.GetCustomAttributes()`).
2. Добавить панель документации справа от `CompletionPanel` (как в VS Code).
3. Отображать сигнатуру при выделении элемента в списке.

**Сложность:** Средняя. Основная работа — в `ApiIndex` и UI `CompletionPanel`. Не затрагивает ядро.

---

## 10. Тень (Shadow)

**Приоритет:** Низкий

**Мотивация:**
Визуальный эффект тени для контейнеров и виджетов. Необходим для создания глубины и иерархии в UI (карточки, выпадающие меню, модальные окна).

**Предполагаемый API:**

```csharp
public class Shadow
{
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public int BlurRadius { get; set; }
    public Color Color { get; set; }
}

// Fluent
UI.Column(8)
    .Shadow(new Shadow { OffsetX = 2, OffsetY = 4, BlurRadius = 8, Color = Color.FromArgb(80, 0, 0, 0) })
    .Add(UI.Label("Card", font, brush));
```

**Требования к адаптерам:**
Реализация через `DropShadowEffect` или ручной рендеринг размытого прямоугольника под компонентом. GDI+ не имеет встроенной поддержки теней, потребуется кастомная реализация через `BlurBitmap` или `ColorMatrix`.

**Сложность:** Высокая. Требует нового интерфейса (`IEffectComponent.Shadow` или отдельный `IShadowComponent`), реализации размытия во всех адаптерах, и учёта тени в Measure (тень выходит за границы Bounds).

---

## 11. TransformNode

**Приоритет:** Низкий

**Мотивация:**
Контейнер, применяющий аффинные преобразования (масштабирование, поворот, сдвиг) к дочерним элементам. Аналог `RenderTransform` в WPF, `Transform` во Flutter.

**Предполагаемый API:**

```csharp
public class TransformNode : LayoutNode
{
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;
    public float Rotation { get; set; }  // градусы
    public float SkewX { get; set; }
    public float SkewY { get; set; }
    public Point TransformOrigin { get; set; }  // (0.5, 0.5) = центр
}

// Fluent
UI.Transform(scaleX: 1.5f, rotation: 45f)
    .Add(UI.Label("Rotated", font, brush));
```

**Требования к адаптерам:**
Применение `Matrix` или `Graphics.Transform` при отрисовке. Влияет на Measure: трансформированный элемент может занимать больше или меньше места.

**Сложность:** Высокая. Корректный Measure с учётом трансформаций требует вычисления bounding box повёрнутого прямоугольника. Не все адаптеры могут поддерживать произвольные трансформации.

---

## 12. Экспорт preview

**Приоритет:** Низкий (для Playground)

**Мотивация:**
Возможность сохранить текущий preview в виде растрового изображения (PNG, BMP). Полезно для документирования UI, создания скриншотов и отладки.

**Предполагаемый API:**

```csharp
// В AppForm
private void ExportPreview(string path, ImageFormat format)
{
    if (_vm.CurrentRoot == null) return;
    var size = new Core.Size(picPreview.ClientSize.Width, picPreview.ClientSize.Height);
    using (var bitmap = new Bitmap(size.Width, size.Height))
    {
        // Рендеринг дерева в bitmap через GDI+
        RenderToBitmap(_vm.CurrentRoot, bitmap);
        bitmap.Save(path, format);
    }
}
```

**Реализация:**
1. Создать `Graphics` из `Bitmap`.
2. Рекурсивно отрисовать все компоненты дерева, используя GDI+-методы.
3. Альтернативный подход: захват существующего preview через `Control.DrawToBitmap`.

**Сложность:** Низкая-средняя. `Control.DrawToBitmap` работает для WinForms-адаптера. Для основного адаптера потребуется кастомный рендеринг в `Bitmap`.

---

## 13. Система событий (Input)

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

## 14. ButtonNode

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

**Зависимости:** Требует систему событий (пункт 13).

**Сложность:** Средняя (при наличии системы событий). Основная работа — управление визуальными состояниями и транслирование кликов через адаптер.

---

## 15. TextInputNode

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

**Зависимости:** Требует систему событий (пункт 13) и, желательно, `Observable<string>` для двусторонней привязки.

**Сложность:** Очень высокая. Реализация текстового ввода с кареткой, выделением, скроллингом, IME-поддержкой и clipboard — одна из самых сложных задач в UI-фреймворках. Альтернативный подход для первого этапа: делегирование нативному контролу адаптера (WinForms `TextBox`, GDI+ кастомный рендеринг).

---

## Сводная таблица

| N  | Элемент | Приоритет | Сложность | Зависимости |
|----|---|---|---|---|
| 1  | Ограничения размеров | Высокий | Низкая | Нет |
| 2  | Flex/Weight в StackLayoutNode | Высокий | Средняя | Нет |
| 3  | WrapPanelNode | Средний | Средняя | Нет |
| 4  | BorderNode | Средний | Средняя | Нет |
| 5  | ComputedObservable | Средний | Средняя | Нет |
| 6  | ObservableList<T> | Средний | Высокая | Нет |
| 7  | ConditionalNode / When | Средний | Средняя | Нет |
| 8  | Градиентные кисти | Средний | Средняя | Нет |
| 9  | Tooltip с сигнатурой | Средний | Средняя | Нет |
| 10 | Тень | Низкий | Высокая | Нет |
| 11 | TransformNode | Низкий | Высокая | Нет |
| 12 | Экспорт preview | Низкий | Низкая-Средняя | Нет |
| 13 | Система событий (Input) | Низкий | Высокая | Нет |
| 14 | ButtonNode | Низкий | Средняя | 13 |
| 15 | TextInputNode | Низкий | Очень высокая | 13 |

## Рекомендуемый порядок реализации

**Фаза 1 — Улучшение layout (без breaking changes):**
1. Ограничения размеров
2. Flex/Weight в StackLayoutNode
3. WrapPanelNode
4. BorderNode

**Фаза 2 — Реактивность:**
5. ComputedObservable
6. ObservableList<T>
7. ConditionalNode / When

**Фаза 3 — Визуальные эффекты:**
8. Градиентные кисти
9. Тень
10. TransformNode

**Фаза 4 — Инструменты (Playground):**
11. Tooltip с сигнатурой в CompletionPanel
12. Экспорт preview

**Фаза 5 — Интерактивность (отложена на поздний срок):**
13. Система событий (Input)
14. ButtonNode
15. TextInputNode

Интерактивность перенесена в последнюю фазу, поскольку текущая архитектура DisplayNodes ориентирована на декларативное описание статического UI. Переход к интерактивным элементам потребует существенного пересмотра ядра: добавления системы событий, механизма hit-testing, обработки фокуса и клавиатурного ввода. Эти изменения не блокируют развитие layout-системы, реактивности и визуальных эффектов, которые могут развиваться независимо.