# Шаблоны скриптов `.dnp`

Руководство по использованию и созданию шаблонов скриптов для DisplayNodes Playground.  
Файлы скриптов используют расширение `.dnp` (DisplayNodes Playground) и представляют собой фрагменты кода на C#, которые возвращают экземпляр `LayoutNode`.

## Содержание

1. [Формат файла .dnp](#format)
2. [Шаблон 1: Базовый (Hello World)](#template-1)
3. [Шаблон 2: Форма ввода (Grid Layout)](#template-2)
4. [Шаблон 3: Информационная панель (Dashboard)](#template-3)
5. [Шаблон 4: Минимальный (Empty)](#template-4)
6. [Шаблон 5: Flex-верстка](#template-5)
7. [Шаблон 6: WrapPanel для тегов](#template-6)
8. [Шаблон 7: Border со скруглёнными углами](#template-7)
9. [Шаблон 8: ComputedObservable](#template-8)
10. [Шаблон 9: ConditionalNode](#template-9)
11. [Рекомендации по написанию скриптов](#recommendations)
12. [Интеграция с Playground](#integration)

---

<a id="format"></a>
## Формат файла .dnp

Скрипт `.dnp` не является полноценным C#-файлом с пространством имён или классом. Playground автоматически оборачивает содержимое файла в статический класс `UserScript` и метод `Build()`.

**Требования к скрипту:**
1. Код должен возвращать экземпляр `LayoutNode` (или наследника, например, `StackLayoutNode`, `GridNode`).
2. Допускается использование локальных переменных, условных операторов и циклов внутри метода.
3. Все необходимые пространства имён (`DisplayNodes.Core`, `DisplayNodes.Fluent`, `DisplayNodes.Core.Rendering`) уже подключены в обёртке.
4. Скрипт не должен содержать побочных эффектов (ввод-вывод, сетевые запросы), так как он выполняется в песочнице Playground.

**Доступные пространства имён в обёртке:**

```csharp
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using DisplayNodes.Widgets;
using DisplayNodes.Gdi;
using Color = System.Drawing.Color;
```

---

<a id="template-1"></a>
## Шаблон 1: Базовый (Hello World)

Простейший пример, демонстрирующий создание вертикального стека, работу со шрифтами, кистями и базовыми виджетами.

```csharp
// Получаем ресурсы через статические фабрики UI
IFont titleFont = UI.Font("Segoe UI", 24f, bold: true);
IFont textFont = UI.Font("Segoe UI", 14f);

IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush grayBrush = UI.SolidBrush(Color.Gray.FromGdi());
IBrush accentBrush = UI.SolidBrush(Color.FromArgb(0, 120, 215).FromGdi());

// Строим дерево узлов с использованием Fluent API
return UI.Column(spacing: 16)
    .Padding(24)
    .HAlignment(Alignment.Center)
    .Add(UI.Label("Добро пожаловать в DisplayNodes", titleFont, whiteBrush))
    .Add(UI.Label("Это декларативная система компоновки интерфейса.", textFont, grayBrush))
    .Add(UI.Fixed(0, 16)) // Распорка
    .Add(UI.Row(spacing: 12)
        .Add(UI.Label("Подробнее", textFont, whiteBrush)
            .BackgroundBrush(accentBrush)
            .Padding(12, 6))
        .Add(UI.Label("Закрыть", textFont, whiteBrush)
            .BackgroundBrush(Color.FromArgb(80, 80, 80).FromGdi())
            .Padding(12, 6)));
```

---

<a id="template-2"></a>
## Шаблон 2: Форма ввода (Grid Layout)

Демонстрация использования `GridNode` для создания структурированных форм с фиксированными и пропорциональными (Star) размерами колонок.

```csharp
IFont labelFont = UI.Font("Segoe UI", 12f);
IFont valueFont = UI.Font("Consolas", 14f);

IBrush labelBrush = UI.SolidBrush(Color.FromArgb(180, 180, 180).FromGdi());
IBrush valueBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush bgBrush = UI.SolidBrush(Color.FromArgb(45, 45, 48).FromGdi());

// Создаём сетку
var grid = UI.Grid()
    .Padding(20);

// Определяем строки: заголовок (Auto), поля (Auto), кнопки (Auto)
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

// Определяем колонки: метка (Auto), значение (Star — занимает всё оставшееся место)
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

// Заполняем ячейки
grid.Add(UI.Label("Имя пользователя:", labelFont, labelBrush), row: 0, column: 0);
grid.Add(UI.Label("admin_user", valueFont, valueBrush)
    .BackgroundBrush(Color.FromArgb(60, 60, 60).FromGdi())
    .Padding(8, 4), row: 0, column: 1);

grid.Add(UI.Label("Статус:", labelFont, labelBrush), row: 1, column: 0);
grid.Add(UI.Label("Активен", valueFont, UI.SolidBrush(Color.LimeGreen.FromGdi()))
    .BackgroundBrush(Color.FromArgb(60, 60, 60).FromGdi())
    .Padding(8, 4), row: 1, column: 1);

grid.Add(UI.Label("Роль:", labelFont, labelBrush), row: 2, column: 0);
grid.Add(UI.Label("Administrator", valueFont, valueBrush)
    .BackgroundBrush(Color.FromArgb(60, 60, 60).FromGdi())
    .Padding(8, 4), row: 2, column: 1);

// Кнопки действий в последней строке
var actions = UI.Row(spacing: 12)
    .MainAlignment(MainAxisAlignment.End)
    .Add(UI.Label("Сохранить", labelFont, UI.SolidBrush(Color.White.FromGdi()))
        .BackgroundBrush(Color.FromArgb(0, 120, 215).FromGdi())
        .Padding(16, 8))
    .Add(UI.Label("Отмена", labelFont, UI.SolidBrush(Color.White.FromGdi()))
        .BackgroundBrush(Color.FromArgb(80, 80, 80).FromGdi())
        .Padding(16, 8));

return UI.Column(spacing: 0)
    .Add(UI.Background(Color.FromArgb(30, 30, 30).FromGdi()))
    .Add(grid)
    .Add(UI.Fixed(0, 10))
    .Add(actions.Margin(0, 0, 20, 20));
```

---

<a id="template-3"></a>
## Шаблон 3: Информационная панель (Dashboard)

Сложный пример, сочетающий `UniformGridNode`, `ClipNode` (маски), `OverlayNode` и фоновые элементы.

```csharp
IFont headerFont = UI.Font("Segoe UI", 16f, bold: true);
IFont valueFont = UI.Font("Segoe UI", 28f, bold: true);
IFont subFont = UI.Font("Segoe UI", 12f);

IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush grayBrush = UI.SolidBrush(Color.Gray.FromGdi());

// Функция-помощник для создания карточки метрики
LayoutNode CreateMetricCard(string title, string value, Color accentColor)
{
    return UI.Overlay()
        .Add(UI.Background(Color.FromArgb(45, 45, 48).FromGdi()))
        .Add(UI.ClipRoundedRect(cornerRadius: 8f)
            .Add(UI.Background(UI.SolidBrush(accentColor.FromGdi())))
            .Add(UI.Column(spacing: 8)
                .Padding(16)
                .Add(UI.Label(title, subFont, whiteBrush))
                .Add(UI.Label(value, valueFont, whiteBrush))));
}

// Основная компоновка
return UI.Column(spacing: 16)
    .Padding(20)
    .Add(UI.Background(Color.FromArgb(30, 30, 30).FromGdi()))
    .Add(UI.Label("Обзор системы", headerFont, whiteBrush))
    .Add(UI.Fixed(0, 12))
    // Равномерная сетка 2x2
    .Add(UI.UniformGrid(rows: 2, columns: 2, spacing: 16)
        .Add(CreateMetricCard("Запросов в секунду", "1,245", Color.FromArgb(0, 120, 215)))
        .Add(CreateMetricCard("Активных пользователей", "8,932", Color.FromArgb(0, 180, 100)))
        .Add(CreateMetricCard("Среднее время отклика", "42 мс", Color.FromArgb(255, 165, 0)))
        .Add(CreateMetricCard("Ошибок (24ч)", "0.01%", Color.FromArgb(220, 50, 50))));
```

---

<a id="template-4"></a>
## Шаблон 4: Минимальный (Empty)

Пустой шаблон для начала разработки с нуля. Содержит только необходимую структуру возврата.

```csharp
// Метод Build() должен возвращать экземпляр LayoutNode.
// Раскомментируйте и измените код ниже для начала работы.

/*
IFont font = UI.Font("Segoe UI", 14f);
IBrush brush = UI.SolidBrush(Color.White.FromGdi());

return UI.Column(spacing: 8)
    .Padding(16)
    .Add(UI.Label("Начните редактирование здесь", font, brush));
*/

throw new NotImplementedException("Скрипт должен возвращать LayoutNode. Удалите эту строку и раскомментируйте код выше.");
```

---

<a id="template-5"></a>
## Шаблон 5: Flex-верстка

Демонстрация `FlexWeight` в `StackLayoutNode` — пропорциональное распределение свободного пространства.

```csharp
IFont font = UI.Font("Segoe UI", 14f);
IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush headerBrush = UI.SolidBrush(Color.FromArgb(0, 120, 215).FromGdi());
IBrush footerBrush = UI.SolidBrush(Color.FromArgb(80, 80, 80).FromGdi());

return UI.Column(spacing: 0)
    // Header — фиксированный размер (FlexWeight = 0)
    .Add(UI.Label("Header", font, whiteBrush)
        .BackgroundBrush(headerBrush)
        .Padding(12))
    // Content — растягивается на всё свободное место
    .Add(new StretchNode(0, 0).Flex(1))
    // Footer — фиксированный размер
    .Add(UI.Label("Footer", font, whiteBrush)
        .BackgroundBrush(footerBrush)
        .Padding(12));
```

**Поведение:**
- `Header` и `Footer` — фиксированной высоты (по содержимому).
- `StretchNode(0, 0).Flex(1)` — занимает всё свободное место между ними.
- Если контейнер имеет ограниченную высоту, свободное место распределяется между flex-детьми пропорционально весам.

### Два flex-ребёнка

```csharp
return UI.Column(spacing: 0)
    .Add(UI.Label("Header", font, whiteBrush).Padding(12))
    .Add(UI.Label("Top (1/3)", font, whiteBrush).Flex(1).Padding(12))
    .Add(UI.Label("Bottom (2/3)", font, whiteBrush).Flex(2).Padding(12))
    .Add(UI.Label("Footer", font, whiteBrush).Padding(12));
```

---

<a id="template-6"></a>
## Шаблон 6: WrapPanel для тегов

Использование `WrapPanelNode` для размещения тегов с автоматическим переносом.

```csharp
IFont font = UI.Font("Segoe UI", 12f);
IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush tagBrush = UI.SolidBrush(Color.FromArgb(60, 60, 60).FromGdi());

string[] tags = new[]
{
    "C#", ".NET", "WPF", "Flutter", "React",
    "TypeScript", "Rust", "Go", "Python",
    "Kotlin", "Swift", "Java"
};

var panel = UI.WrapPanel(
    direction: WrapDirection.Horizontal,
    spacing: 8,        // между тегами в строке
    lineSpacing: 8)    // между строками
    .Padding(16)
    .Add(UI.Background(Color.FromArgb(30, 30, 30).FromGdi()));

foreach (var tag in tags)
{
    panel.Add(UI.Label(tag, font, whiteBrush)
        .BackgroundBrush(tagBrush)
        .Padding(10, 5));
}

return panel;
```

**Поведение:**
- Теги размещаются слева направо.
- Когда места не хватает — переносятся на новую строку.
- `spacing` — между тегами в строке.
- `lineSpacing` — между строками.

### Вертикальный WrapPanel

```csharp
var panel = UI.WrapPanel(
    direction: WrapDirection.Vertical,
    spacing: 8,
    lineSpacing: 8)
    .Padding(16);

// Столбцы сверху вниз, перенос вправо
```

---

<a id="template-7"></a>
## Шаблон 7: Border со скруглёнными углами

Использование `UI.Border` для создания карточек с фоном и скруглёнными углами.

```csharp
IFont titleFont = UI.Font("Segoe UI", 18f, bold: true);
IFont textFont = UI.Font("Segoe UI", 14f);

IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush grayBrush = UI.SolidBrush(Color.FromArgb(200, 200, 200).FromGdi());
IBrush cardBrush = UI.SolidBrush(Color.FromArgb(45, 45, 48).FromGdi());

return UI.Column(spacing: 16)
    .Padding(20)
    .Add(UI.Background(Color.FromArgb(30, 30, 30).FromGdi()))

    // Карточка 1: скруглённые углы
    .Add(UI.Border(cardBrush, cornerRadius: 8)
        .Padding(16)
        .Add(UI.Column(spacing: 8)
            .Add(UI.Label("Заголовок карточки", titleFont, whiteBrush))
            .Add(UI.Label("Текст внутри карточки с фоном и скруглёнными углами.", textFont, grayBrush))))

    // Карточка 2: прямоугольная (cornerRadius = 0)
    .Add(UI.Border(Color.FromArgb(60, 60, 60), cornerRadius: 0)
        .Padding(16)
        .Add(UI.Label("Прямоугольная карточка", textFont, whiteBrush)));
```

**Структура:**

`UI.Border` возвращает `OverlayNode` с первым ребёнком `ClipNode`, внутри которого `BackgroundNode`. Контент, добавленный через `.Add(...)`, рисуется поверх фона.

---

<a id="template-8"></a>
## Шаблон 8: ComputedObservable

Демонстрация реактивного обновления через `ComputedObservable<T>`.

```csharp
IFont font = UI.Font("Segoe UI", 14f);
IFont bigFont = UI.Font("Segoe UI", 48f, bold: true);
IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush accentBrush = UI.SolidBrush(Color.FromArgb(0, 120, 215).FromGdi());

// Источник данных
var counter = new Observable<int>(0);

// Вычисляемое свойство: текст счётчика
var counterText = new ComputedObservable<string>(
    () => counter.Value.ToString(),
    counter);

// Вычисляемое свойство: чётность
var parityText = new ComputedObservable<string>(
    () => counter.Value % 2 == 0 ? "чётное" : "нечётное",
    counter);

return UI.Column(spacing: 16)
    .Padding(40)
    .MainAlignment(MainAxisAlignment.Center)
    .Add(UI.Label("Счётчик", font, whiteBrush))
    .Add(UI.Label("0", bigFont, whiteBrush)
        .BindText(counterText))
    .Add(UI.Label("", font, accentBrush)
        .BindText(parityText));
```

**Поведение:**
- `counter` — источник данных.
- `counterText` и `parityText` — вычисляемые свойства, автоматически пересчитываемые при изменении `counter`.
- `.BindText(counterText)` — привязка метки к вычисляемому свойству.

### Пример: полное имя

```csharp
var firstName = new Observable<string>("Ivan");
var lastName = new Observable<string>("Petrov");

var fullName = new ComputedObservable<string>(
    () => firstName.Value + " " + lastName.Value,
    firstName,
    lastName);

return UI.Label("", font, whiteBrush).BindText(fullName);
```

---

<a id="template-9"></a>
## Шаблон 9: ConditionalNode

Условный рендеринг через `UI.When` — отображение одного из двух поддеревьев по `Observable<bool>`.

```csharp
IFont font = UI.Font("Segoe UI", 16f);
IBrush whiteBrush = UI.SolidBrush(Color.White.FromGdi());
IBrush grayBrush = UI.SolidBrush(Color.Gray.FromGdi());
IBrush greenBrush = UI.SolidBrush(Color.LimeGreen.FromGdi());

var isLoggedIn = new Observable<bool>(false);

return UI.Column(spacing: 16)
    .Padding(20)
    .Add(UI.Background(Color.FromArgb(30, 30, 30).FromGdi()))
    .Add(UI.When(
        isLoggedIn,
        trueNode: UI.Column(spacing: 8)
            .Add(UI.Label("Welcome back!", font, greenBrush))
            .Add(UI.Label("Вы успешно вошли в систему.", font, whiteBrush)),
        falseNode: UI.Column(spacing: 8)
            .Add(UI.Label("Please log in", font, grayBrush))
            .Add(UI.Label("Введите логин и пароль.", font, grayBrush))));
```

**Поведение:**
- Оба поддерева (`TrueNode` и `FalseNode`) хранятся в `Children` всегда.
- Неактивное поддерево пропускается в `Measure`/`Arrange` — не занимает места.
- При переключении `isLoggedIn.Value` **layout не пересчитывается автоматически**. Нужно вызвать `Measure`+`Arrange` вручную или перестроить дерево.

### Только true-ветка

```csharp
var isLoading = new Observable<bool>(true);

return UI.When(
    isLoading,
    trueNode: UI.Label("Loading...", font, grayBrush));
    // falseNode не задан — при false отображается пустота
```

---

<a id="recommendations"></a>
## Рекомендации по написанию скриптов

1. **Избегайте глобального состояния.** Скрипт выполняется многократно при каждом изменении кода (в режиме AutoRun). Не сохраняйте состояние в статических полях, если это не требуется явно.

2. **Используйте Fluent API.** Цепочки вызовов `.Add().Padding().HAlignment()` делают код декларативным и легко читаемым.

3. **Выносите повторяющиеся узлы в методы.** Как показано в шаблоне "Dashboard", локальные методы внутри скрипта помогают избежать дублирования кода и сохраняют читаемость.

4. **Фиксируйте размеры для отладки.** Если элемент ведёт себя неожиданно, временно добавьте `.SetSize(width, height)` или оберните его в `FixedNode`, чтобы изолировать проблему компоновки.

5. **Безопасность ресурсов.** Скрипт не должен создавать и диспоузить ресурсы (шрифты, кисти) вручную. Используйте фабрики `UI.Font()`, `UI.SolidBrush()`, которые делегируют управление жизненным циклом адаптеру.

6. **Используйте `ComputedObservable` вместо ручных подписок.** Для производных значений (например, текста на основе счётчика) используйте `ComputedObservable<T>` — он сам управляет подписками.

7. **Помните об ограничениях.** Адаптеры не поддерживают градиенты, тень и `TransformNode` — они бросают `NotSupportedException`. Используйте только сплошные кисти и стандартные контейнеры.

---

<a id="integration"></a>
## Интеграция с Playground

Для использования шаблона:
1. Откройте DisplayNodes Playground.
2. Нажмите `Файл` → `Новый` (или очистите текущий редактор).
3. Скопируйте содержимое одного из шаблонов выше в редактор.
4. Нажмите `F5` или дождитесь срабатывания `Автозапуска`.
5. Результат отобразится в панели Preview справа. Ошибки компиляции (если есть) появятся в нижней панели с возможностью перехода к строке по двойному клику.