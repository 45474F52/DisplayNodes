# Шаблоны скритов `.dnp`

Руководство по использованию и созданию шаблонов скриптов для DisplayNodes Playground.  
Файлы скриптов используют расширение `.dnp` (DisplayNodes Playground) и представляют собой фрагменты кода на C#, которые возвращают экземпляр `LayoutNode`.

## Формат файла .dnp

Скрипт `.dnp` не является полноценным C#-файлом с пространством имен или классом. Playground автоматически оборачивает содержимое файла в статический класс `UserScript` и метод `Build()`. 

**Требования к скрипту:**
1. Код должен возвращать экземпляр `LayoutNode` (или наследника, например, `StackLayoutNode`, `GridNode`).
2. Допускается использование локальных переменных, условных операторов и циклов внутри метода.
3. Все необходимые пространства имен (`DisplayNodes.Core`, `DisplayNodes.Fluent`, `DisplayNodes.Core.Rendering`) уже подключены в обёртке.
4. Скрипт не должен содержать побочных эффектов (ввод-вывод, сетевые запросы), так как он выполняется в песочнице Playground.

---

## Шаблон 1: Базовый (Hello World)

Простейший пример, демонстрирующий создание вертикального стека, работу со шрифтами, кистями и базовыми виджетами.

```csharp
// Получаем ресурсы через статические фабрики UI
IFont titleFont = UI.Font("Segoe UI", 24f, bold: true);
IFont textFont = UI.Font("Segoe UI", 14f);

IBrush whiteBrush = UI.Brush(Color.White);
IBrush grayBrush = UI.Brush(Color.Gray);
IBrush accentBrush = UI.Brush(Color.FromArgb(0, 120, 215));

// Строим дерево узлов с использованием Fluent API
return UI.Column(spacing: 16)
    .Padding(24)
    .HAlignment(Alignment.Center)
    .Add(UI.Label("Добро пожаловать в DisplayNodes", titleFont, whiteBrush))
    .Add(UI.Label("Это декларативная система компоновки интерфейса.", textFont, grayBrush))
    .Add(UI.Fixed(0, 16)) // Распорка
    .Add(UI.Row(spacing: 12)
        .Add(UI.Label("Подробнее", textFont, whiteBrush)
            .FullBrush(accentBrush)
            .Padding(12, 6))
        .Add(UI.Label("Закрыть", textFont, whiteBrush)
            .FullBrush(Color.FromArgb(80, 80, 80))
            .Padding(12, 6)));
```

---

## Шаблон 2: Форма ввода (Grid Layout)

Демонстрация использования `GridNode` для создания структурированных форм с фиксированными и пропорциональными (Star) размерами колонок.

```csharp
IFont labelFont = UI.Font("Segoe UI", 12f);
IFont valueFont = UI.Font("Consolas", 14f);

IBrush labelBrush = UI.Brush(Color.FromArgb(180, 180, 180));
IBrush valueBrush = UI.Brush(Color.White);
IBrush bgBrush = UI.Brush(Color.FromArgb(45, 45, 48));

// Создаём сетку
var grid = UI.Grid()
    .Padding(20);

// Определяем строки: заголовок (Auto), поля (Auto), кнопки (Auto)
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

// Определяем колонки: метка (Auto), значение (Star - занимает всё оставшееся место)
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));

// Заполняем ячейки
grid.Add(UI.Label("Имя пользователя:", labelFont, labelBrush), row: 0, column: 0);
grid.Add(UI.Label("admin_user", valueFont, valueBrush)
    .FullBrush(Color.FromArgb(60, 60, 60))
    .Padding(8, 4), row: 0, column: 1);

grid.Add(UI.Label("Статус:", labelFont, labelBrush), row: 1, column: 0);
grid.Add(UI.Label("Активен", valueFont, UI.Brush(Color.LimeGreen))
    .FullBrush(Color.FromArgb(60, 60, 60))
    .Padding(8, 4), row: 1, column: 1);

grid.Add(UI.Label("Роль:", labelFont, labelBrush), row: 2, column: 0);
grid.Add(UI.Label("Administrator", valueFont, valueBrush)
    .FullBrush(Color.FromArgb(60, 60, 60))
    .Padding(8, 4), row: 2, column: 1);

// Кнопки действий в последней строке, растянуты на обе колонки (упрощённо через Row внутри Grid)
// В GridNode для span пока используется вложенный контейнер
var actions = UI.Row(spacing: 12)
    .MainAlignment(MainAxisAlignment.End)
    .Add(UI.Label("Сохранить", labelFont, UI.Brush(Color.White))
        .FullBrush(Color.FromArgb(0, 120, 215))
        .Padding(16, 8))
    .Add(UI.Label("Отмена", labelFont, UI.Brush(Color.White))
        .FullBrush(Color.FromArgb(80, 80, 80))
        .Padding(16, 8));

// Для имитации span добавляем действия в отдельную строку или используем Overlay/Column поверх
// В текущей версии GridNode не поддерживает row-span напрямую, поэтому добавим как отдельный элемент Column
return UI.Column(spacing: 0)
    .Add(UI.Background(Color.FromArgb(30, 30, 30)))
    .Add(grid)
    .Add(UI.Fixed(0, 10))
    .Add(actions.Margin(0, 0, 20, 20));
```

---

## Шаблон 3: Информационная панель (Dashboard)

Сложный пример, сочетающий `UniformGridNode`, `ClipNode` (маски), `OverlayNode` и фоновые элементы.

```csharp
IFont headerFont = UI.Font("Segoe UI", 16f, bold: true);
IFont valueFont = UI.Font("Segoe UI", 28f, bold: true);
IFont subFont = UI.Font("Segoe UI", 12f);

IBrush whiteBrush = UI.Brush(Color.White);
IBrush grayBrush = UI.Brush(Color.Gray);

// Функция-помощник для создания карточки метрики
LayoutNode CreateMetricCard(string title, string value, Color accentColor)
{
    return UI.Overlay()
        .Add(UI.Background(Color.FromArgb(45, 45, 48)))
        .Add(UI.ClipRoundedRect(cornerRadius: 8f)
            .Add(UI.Background(accentColor))
            .Add(UI.Column(spacing: 8)
                .Padding(16)
                .Add(UI.Label(title, subFont, whiteBrush))
                .Add(UI.Label(value, valueFont, whiteBrush))));
}

// Основная компоновка
return UI.Column(spacing: 16)
    .Padding(20)
    .Add(UI.Background(Color.FromArgb(30, 30, 30)))
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

## Шаблон 4: Минимальный (Empty)

Пустой шаблон для начала разработки с нуля. Содержит только необходимую структуру возврата.

```csharp
// Метод Build() должен возвращать экземпляр LayoutNode.
// Раскомментируйте и измените код ниже для начала работы.

/*
IFont font = UI.Font("Segoe UI", 14f);
IBrush brush = UI.Brush(Color.White);

return UI.Column(spacing: 8)
    .Padding(16)
    .Add(UI.Label("Начните редактирование здесь", font, brush));
*/

throw new NotImplementedException("Скрипт должен возвращать LayoutNode. Удалите эту строку и раскомментируйте код выше.");
```

---

## Шаблон 5: Демонстрация выравнивания (Alignment Demo)

Наглядный пример работы свойств `HAlignment`, `VAlignment` и `MainAxisAlignment` в различных контейнерах.

```csharp
IFont font = UI.Font("Segoe UI", 12f);
IBrush boxBrush = UI.Brush(Color.FromArgb(60, 60, 60));
IBrush textBrush = UI.Brush(Color.White);

// Вспомогательный метод для создания тестового блока
LayoutNode MakeBox(string text, Alignment hAlign, Alignment vAlign)
{
    return UI.Label(text, font, textBrush)
        .FullBrush(boxBrush)
        .Padding(12, 8)
        .HAlignment(hAlign)
        .VAlignment(vAlign);
}

return UI.Column(spacing: 20)
    .Padding(20)
    .Add(UI.Background(Color.FromArgb(30, 30, 30)))
    
    // Пример 1: StackLayoutNode с MainAxisAlignment
    .Add(UI.Label("MainAxisAlignment.SpaceBetween", font, textBrush))
    .Add(UI.Row(spacing: 0)
        .MainAlignment(MainAxisAlignment.SpaceBetween)
        .SetSize(400, 40) // Фиксируем размер для наглядности
        .Add(MakeBox("Start", Alignment.Start, Alignment.Center))
        .Add(MakeBox("Center", Alignment.Center, Alignment.Center))
        .Add(MakeBox("End", Alignment.End, Alignment.Center)))
        
    // Пример 2: Выравнивание внутри Overlay
    .Add(UI.Label("Overlay с разными выравниваниями", font, textBrush))
    .Add(UI.Overlay()
        .SetSize(400, 100)
        .Add(UI.Background(Color.FromArgb(45, 45, 48)))
        .Add(MakeBox("Top-Left", Alignment.Start, Alignment.Start))
        .Add(MakeBox("Center", Alignment.Center, Alignment.Center))
        .Add(MakeBox("Bottom-Right", Alignment.End, Alignment.End))
        .Add(MakeBox("Stretch", Alignment.Stretch, Alignment.Stretch)));
```

---

## Рекомендации по написанию скриптов

1. **Избегайте глобального состояния**: Скрипт выполняется многократно при каждом изменении кода (в режиме AutoRun). Не сохраняйте состояние в статических полях, если это не требуется явно.
2. **Используйте Fluent API**: Цепочки вызовов `.Add().Padding().HAlignment()` делают код декларативным и легко читаемым.
3. **Выносите повторяющиеся узлы в методы**: Как показано в шаблоне "Dashboard", локальные методы внутри скрипта помогают избежать дублирования кода и сохраняют читаемость.
4. **Фиксируйте размеры для отладки**: Если элемент ведёт себя неожиданно, временно добавьте `.SetSize(width, height)` или оберните его в `FixedNode`, чтобы изолировать проблему компоновки.
5. **Безопасность ресурсов**: Скрипт не должен создавать и диспоузить GDI-ресурсы (шрифты, кисти) вручную. Используйте фабрики `UI.Font()`, `UI.Brush()`, которые делегируют управление жизненным циклом адаптеру.

## Интеграция с Playground

Для использования шаблона:
1. Откройте DisplayNodes Playground.
2. Нажмите `Файл` → `Новый` (или очистите текущий редактор).
3. Скопируйте содержимое одного из шаблонов выше в редактор.
4. Нажмите `F5` или дождитесь срабатывания `Автозапуска`.
5. Результат отобразится в панели Preview справа. Ошибки компиляции (если есть) появятся в нижней панели с возможностью перехода к строке по двойному клику.