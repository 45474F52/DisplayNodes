# DisplayNodes.Playground

IDE для интерактивной разработки UI-скриптов на DisplayNodes. Редактор с подсветкой синтаксиса, автодополнением, хот-релоадом UI и отладкой ошибок компиляции.

## Содержание

1. [Структура](#structure)
2. [Архитектура](#architecture)
3. [Компиляция скриптов](#compilation)
4. [Редактор кода](#editor)
5. [Инфраструктура](#infrastructure)
6. [Хот-релоад](#hot-reload)
7. [Абстракция preview](#preview)
8. [Экспорт preview](#export)
9. [Формат файлов](#format)
10. [Точка входа](#entry-point)
11. [Шаблоны скриптов](#templates)
12. [Зависимости](#dependencies)

---

<a id="structure"></a>
## Структура

```
Playground/
├── Compilation/          # Компиляция и выполнение скриптов
├── Editor/               # Редактор кода (CodeEditor, подсветка, автодополнение)
├── Infrastructure/       # Настройки, состояние, логирование, ViewModel
├── AppForm.cs            # Главное окно
└── Program.cs            # Точка входа
```

---

<a id="architecture"></a>
## Архитектура

```
┌──────────────────────────────────────────────────────────┐
│                        AppForm                           │  ← Главное окно, split-панели
├─────────────┬────────────────────────────────────────────┤
│             │                                            │
│ CodeEditor  │              AppViewModel                  │  ← Бизнес-логика
│ (Editor/)   │         ┌──────────────────────┐           │
│             │         │   ScriptRunner       │           │  ← Оркестратор
│             │         │   ├─ ICompiler       │           │
│             │         │   ├─ ErrorParser     │           │
│             │         │   └─ ScriptInvoker   │           │
│             │         └──────────────────────┘           │
│             │                                            │
├─────────────┴────────────────────────────────────────────┤
│  AppSettings / EditorSettings / AppState / RecentFiles   │  ← Инфраструктура
└──────────────────────────────────────────────────────────┘
```

---

<a id="compilation"></a>
## Компиляция скриптов

### Два режима компиляции

| Режим | Компилятор | C# | Именованные аргументы |
|---|---|---|---|
| **Внешний** | `csc.exe` 4.0+ из `%windir%\Microsoft.NET\Framework64\v4.0.30319` | 4.0+ | ✅ |
| **Встроенный** | `CSharpCodeProvider` (v3.5) | 3.0 | ❌ |

`ScriptRunner` автоматически ищет `csc.exe 4.0+`. Если не находит — fallback на встроенный. Текущий режим отображается в статус-баре.

### Обёртка скрипта

Пользовательский код оборачивается в шаблон `ScriptWrapper.TEMPLATE`:

```csharp
using System;
using System.Linq;
using System.Drawing;
using DisplayNodes.Core;
using DisplayNodes.Fluent;
// ... другие using

public static class UserScript
{
    public static LayoutNode Build()
    {
        // ← пользовательский код здесь
    }
}
```

**Важно:** `ScriptWrapper.HEADER_LINE_COUNT = 17` — смещение для корректного отображения номеров строк в ошибках. Обновлять вручную при изменении шаблона.

### Загрузка сборки

```csharp
// Assembly.Load(byte[]) — не кэширует сборку в AppDomain,
// поэтому повторная компиляция подхватывает изменения.
Assembly assembly = Assembly.Load(assemblyBytes);
Type type = assembly.GetType("UserScript");
LayoutNode root = (LayoutNode)type.GetMethod("Build").Invoke(null, null);
```

### Парсинг ошибок

`ErrorParser` парсит вывод обоих компиляторов и возвращает строки вида:

```
Line 12, Col 5: ; expected (CS1002)
```

Номера строк корректируются на `HEADER_LINE_COUNT` — пользователь видит реальные номера в своём коде.

---

<a id="editor"></a>
## Редактор кода

### `CodeEditor` (UserControl)

Главный компонент редактора. Собирает воедино:

| Компонент | Назначение |
|---|---|
| `ScrollRichTextBox` | Базовый редактор на `RichTextBox` с WinAPI-хуками |
| `BufferedPanel` (gutter) | Жёлоб с номерами строк |
| `SyntaxHighlighter` | Подсветка синтаксиса через `EM_SETCHARFORMAT` |
| `UndoManager` | Собственная система undo/redo с группировкой набора |
| `FindReplacePanel` | Поиск/замена (plain + regex) |
| `CompletionPanel` | Всплывающий список автодополнения |
| `CompletionTooltip` | Popup с сигнатурой, типом и XML-документацией |

### Подсветка синтаксиса

Собственный лексер `CSharpLexer` + WinAPI `EM_SETCHARFORMAT` (без пересоздания текста).

**Категории токенов:**
- `Keyword` — ключевые слова C#
- `ControlFlow` — `if`, `for`, `return`, `throw`, ...
- `Type` — классы, перечисления
- `Interface` — интерфейсы
- `Struct` — структуры (`Point`, `Size`, `Color`, ...)
- `Method` — методы (определяется по `(` справа)
- `String`, `Comment`, `Number`, `Preprocessor`

**Оптимизация:**
- Debounce-таймер: `HighlightSlowMs` (300 мс) при обычном вводе, `HighlightFastMs` (50 мс) при Backspace/Delete/Enter.
- `WM_SETREDRAW` отключает перерисовку на время применения.
- Позиция скролла и каретки сохраняются/восстанавливаются через `EM_GETSCROLLPOS`/`EM_SETSCROLLPOS`.

### Автодополнение

- Открывается по `.` после идентификатора.
- Источник — `ApiIndex` (рефлексия по загруженным сборкам).
- Фильтрация по вводимым символам.
- Принятие по `Tab`/`Enter`, закрытие по `Esc`.
- **Tooltip** с сигнатурой, типом возврата и XML-документацией (из своих сборок).
- Позиционирование с учётом краёв экрана (рабочая область монитора).
- `Form` с `WS_EX_NOACTIVATE` — не крадёт фокус, не закрывает `CompletionPanel`.

### `XmlDocProvider`

Загружает XML-документацию из `.xml`-файлов рядом со сборками.

- Ищет `{AssemblyName}.xml` рядом с `{AssemblyLocation}`.
- Поддерживает `<see cref>`, `<paramref>`, нормализацию whitespace.
- Документация только из своих сборок (BCL не читается).

### `ApiIndex`

Реестр типов и их публичных членов, собранный из рефлексии.

- `Initialize(XmlDocProvider, params Assembly[])`.
- `GetMembers(string typeName)` — возвращает `List<CompletionItem>` с сигнатурой, типом, XML-doc.
- Собирает методы, свойства и поля.
- Форматирует типы по правилам C# (`int`, `string`, `List<T>`, `T[]`).
- Потокобезопасен (внутренний `lock`).

### Undo/Redo

Собственная система (`UndoManager`), т.к. встроенный undo RichEdit конфликтует с программными правками.

**Особенности:**
- Группировка быстрого набора (`TypingGroupMs = 500 мс`) — один undo-шаг на слово.
- `SyncCaret()` — обновление позиции каретки при навигации.
- `BreakTypingGroup()` — сброс группы при стрелках/кликах.
- Встроенный undo RichEdit отключён (`EM_SETUNDOLIMIT = 0`).

### Поиск и замена

- Plain text и regex.
- Case-sensitive опционально.
- Циклический поиск (wrap around).
- Replace / Replace All.
- Статус-бар: «не найдено», «ошибка regex», «заменено N».

### Жёлоб (Gutter)

- Номера строк.
- Подсветка текущей строки.
- Клик по жёлобу — выделение всей строки.
- Ширина адаптируется под количество цифр.

### Оверлеи

Рисуются поверх текста в `WndProc(WM_PAINT)`:
- **Подсветка текущей строки** — полупрозрачная полоса.
- **Волнистые подчёркивания ошибок** — красная волна под строками с ошибками компиляции.
- **Направляющие отступа** — вертикальные линии на границах уровней.

### Горячие клавиши

| Клавиша | Действие |
|---|---|
| `F5` | Запустить скрипт |
| `Ctrl+F` | Поиск |
| `Ctrl+H` | Поиск и замена |
| `Ctrl+G` | Перейти к строке |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` / `Ctrl+Shift+Z` | Redo |
| `Ctrl+/` | Закомментировать / раскомментировать строки |
| `Ctrl+Shift+/` | Раскомментировать строки |
| `Ctrl+D` | Дублировать строки |
| `Ctrl+Shift+K` | Удалить строки |
| `Ctrl+E` | Экспорт изображения |
| `Alt+↑` / `Alt+↓` | Переместить строки вверх/вниз |
| `Tab` / `Shift+Tab` | Увеличить / уменьшить отступ |
| `Enter` | Новая строка с сохранением отступа (+1 уровень после `{`) |
| `Backspace` | Умный: стирает до границы отступа, удаляет пустые пары `()`, `[]`, `{}` |
| `(` `[` `{` `"` `'` | Автозакрытие: оборачивает выделение или вставляет пару |
| `)` `]` `}` `"` `'` | Overtype: пропускает символ, если он уже справа |
| `Esc` | Закрыть панель поиска / автодополнения |

---

<a id="infrastructure"></a>
## Инфраструктура

### `AppViewModel`

Бизнес-логика, не знает о UI:
- `Run(code)` — компиляция + выполнение.
- `Save(path, code)` / `Load(path)` — работа с файлами.
- `IsDirty` — флаг несохранённых изменений.
- `CurrentRoot` — текущий `LayoutNode` (автоматически диспоузит старый при пересборке).

### `AppSettings` / `EditorSettings`

Настройки хранятся в `%APPDATA%\DisplayNodesPlayground\settings.txt` (формат `key=value`).

**Категории:**
- **Редактор:** шрифт, размер отступа, табы, номера строк, гайды, подчёркивания.
- **Производительность:** debounce-задержки, интервалы подсветки.
- **Интерфейс:** размер preview по умолчанию.
- **Цвета:** 11 настраиваемых цветов подсветки (формат `R,G,B,A`).

### `AppState`

Геометрия окна, позиции сплиттеров, последний файл, флаг автозапуска.

### `RecentFilesManager`

Список последних файлов (макс. 10). Файл `recent.txt`. Автоматически убирает несуществующие.

### `AppLog`

Логирование ошибок в `%APPDATA%\DisplayNodesPlayground\error.log`.

---

<a id="hot-reload"></a>
## Хот-релоад

### Автоматический запуск

При включённом `AutoRun` скрипт перекомпилируется после `AutoRunDebounceMs` (500 мс) тишины в редакторе.

### Ресайз без перекомпиляции

При изменении размера окна/preview **перекомпиляция не запускается** — только `Measure` + `Arrange` на существующем дереве:

```csharp
private void RelayoutPreview()
{
    var newSize = _preview.CurrentSize;
    _vm.CurrentRoot.Measure(newSize);
    _vm.CurrentRoot.Arrange(new Rect(Point.Empty, newSize));
    _preview.Resize(newSize);
}
```

### Дебаунс при ресайзе

`Resize` приходит десятки раз в секунду при перетаскивании рамки. Запуск скрипта откладывается через `_layoutDebounceTimer`. `ResizeEnd` (когда пользователь отпустил рамку) — мгновенный запуск.

---

<a id="preview"></a>
## Абстракция preview

`IPreviewHost` — интерфейс, инкапсулирующий работу с конкретным адаптером:

```csharp
public interface IPreviewHost : IDisposable
{
    Size CurrentSize { get; }
    void Build(LayoutNode root);
    void Resize(Size size);
    void Clear();
    Bitmap ExportToBitmap();
}
```

**Реализации:**
- `WinFormsPreviewHost` — для `WinFormsAdapter`. Рендерит дерево как WinForms-контролы.
- (потенциально) другие хосты для других адаптеров.

**Преимущества:**
- `AppForm` не знает о конкретном адаптере.
- Переключение между адаптерами — замена одной строки в конструкторе.

---

<a id="export"></a>
## Экспорт preview

- Пункт меню `Файл` → `Экспорт изображения` (или `Ctrl+E`).
- Форматы: PNG / JPEG / BMP.
- Сохраняется текущее содержимое `picPreview`.

---

<a id="format"></a>
## Формат файлов

Скрипты сохраняются в `.dnp` (DisplayNodes Playground) — обычный текст с C#-кодом.

---

<a id="entry-point"></a>
## Точка входа

```csharp
[STAThread]
private static void Main()
{
    Adapter.Initialize();  // Инициализация бэкенда
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new AppForm());
}
```

---

<a id="templates"></a>
## Шаблоны скриптов

`DefaultScripts` содержит три шаблона:
- `HELLO` — базовый пример с колонкой и метками.
- `WITH_BACKGROUND` — пример с фоном и моноширинным шрифтом.
- `EMPTY` — пустой шаблон с подсказкой.

См. [DNP_TEMPLATES.md](../docs/DNP_TEMPLATES.md) для полного списка шаблонов (Flex, WrapPanel, Border, ComputedObservable, ConditionalNode).

---

<a id="dependencies"></a>
## Зависимости

- `DisplayNodes.Kernel` — UI-дерево, типы.
- `DisplayNodes.Gdi` — GDI+-обёртки.
- `DisplayNodes.WinFormsAdapter` — бэкенд для preview.
- `System.Windows.Forms` — UI приложения.
- `System.Drawing` — GDI+.

---

## Документация

- [DNP_TEMPLATES.md](../docs/DNP_TEMPLATES.md) — шаблоны скриптов
- [EXAMPLES.md](../docs/EXAMPLES.md) — примеры использования
- [ARCHITECTURE.md](../docs/ARCHITECTURE.md) — архитектура ядра