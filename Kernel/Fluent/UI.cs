///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
	/// <summary>
	/// Фабрика узлов UI. Предоставляет короткие методы для создания контейнеров и виджетов.
	/// </summary>
	/// <remarks>
	/// Требует предварительной инициализации <see cref="Factory"/> и <see cref="Measurer"/>.
	/// </remarks>
	public static class UI
	{
		/// <summary>Фабрика компонентов рендерера. Должна быть установлена перед использованием.</summary>
		public static IWidgetFactory Factory { get; set; }

		/// <summary>Измеритель текста. Должен быть установлен перед использованием.</summary>
		public static ITextMeasurer Measurer { get; set; }

		/// <summary>Фабрика кистей. Должна быть установлена перед использованием.</summary>
		public static IBrushFactory BrushFactory { get; set; }

		/// <summary>Фабрика шрифтов. Должна быть установлена перед использованием.</summary>
		public static IFontFactory FontFactory { get; set; }

		/// <summary>Фабрика изображений. Должна быть установлена перед использованием.</summary>
		public static IImageFactory ImageFactory { get; set; }

		/// <summary>Фабрика форматов текста. Должна быть установлена перед использованием.</summary>
		public static ITextFormatFactory TextFormatFactory { get; set; }

		private static IWidgetFactory RequireFactory()
			=> Factory ?? throw new InvalidOperationException("UI.Factory is not initialized. Call Adapter.Initialize() first.");

		private static ITextMeasurer RequireMeasurer()
			=> Measurer ?? throw new InvalidOperationException("UI.Measurer is not initialized. Call Adapter.Initialize() first.");

		private static IBrushFactory RequireBrushFactory()
			=> BrushFactory ?? throw new InvalidOperationException("UI.BrushFactory is not initialized. Call Adapter.Initialize() first.");

		private static IFontFactory RequireFontFactory()
			=> FontFactory ?? throw new InvalidOperationException("UI.FontFactory is not initialized. Call Adapter.Initialize() first.");

		private static IImageFactory RequireImageFactory()
			=> ImageFactory ?? throw new InvalidOperationException("UI.ImageFactory is not initialized. Call Adapter.Initialize() first.");

		private static ITextFormatFactory RequireTextFormatFactory()
			=> TextFormatFactory ?? throw new InvalidOperationException("UI.TextFormatFactory is not initialized. Call Adapter.Initialize() first.");

		/// <summary>Создаёт шрифт</summary>
		public static IFont Font(string family, float size, bool bold = false, bool italic = false)
			=> RequireFontFactory().Create(family, size, bold, italic);

		/// <summary>Создаёт изображение по переданному пути</summary>
		public static IImage ImageFromFile(string path) => RequireImageFactory().CreateFromFile(path);

		/// <summary>Создаёт изображение по переданному потоку</summary>
		public static IImage ImageFromStream(Stream stream) => RequireImageFactory().CreateFromStream(stream);

		/// <summary>Создаёт изображение по переданному массиву байт</summary>
		public static IImage ImageFromBytes(byte[] bytes) => RequireImageFactory().CreateFromBytes(bytes);

		/// <summary>Создаёт кисть по переданному цвету</summary>
		public static IBrush SolidBrush(Color color) => RequireBrushFactory().CreateSolidBrush(color);

		/// <summary>Создаёт формат текста с горизонтальным и вертикальным выравниванием.</summary>
		/// <param name="horizontal">Выравнивание по горизонтали.</param>
		/// <param name="vertical">Выравнивание по вертикали (по умолчанию как по горизонтали).</param>
		public static ITextFormat TextFormat(Alignment horizontal, Alignment? vertical = null)
				=> RequireTextFormatFactory().Create(horizontal, vertical ?? horizontal);

		/// <summary>Создаёт стек-контейнер (строка или столбец).</summary>
		public static StackLayoutNode Stack(bool vertical = true, int spacing = 0)
			=> new StackLayoutNode { IsVertical = vertical, Spacing = spacing };

		/// <summary>Создаёт горизонтальный стек (строку).</summary>
		public static StackLayoutNode Row(int spacing = 0)
			=> Stack(vertical: false, spacing: spacing);

		/// <summary>Создаёт вертикальный стек (столбец).</summary>
		public static StackLayoutNode Column(int spacing = 0)
			=> Stack(vertical: true, spacing: spacing);

		/// <summary>Создаёт равномерную сетку с фиксированным числом строк и колонок.</summary>
		public static UniformGridNode UniformGrid(int rows, int columns, int spacing = 0)
			=> new UniformGridNode(rows, columns, spacing);

		/// <summary>Создаёт сетку с произвольными размерами строк/колонок.</summary>
		public static GridNode Grid()
			=> new GridNode();

        /// <summary>
        /// Создаёт контейнер с автоматическим переносом по заданной оси
        /// </summary>
        /// <param name="direction">Ось переноса дочерних компонентов</param>
        /// <param name="spacing">Расстояние между дочерними компонентами по <b>главной оси</b></param>
        /// <param name="lineSpacing">Расстояние <b>вдоль</b> главной оси</param>
        public static WrapPanelNode WrapPanel(WrapDirection direction = WrapDirection.Horizontal, int spacing = 0, int lineSpacing = 0)
			=> new WrapPanelNode
			{
				Direction = direction,
				Spacing = spacing,
				LineSpacing = lineSpacing
			};

		/// <summary>Создаёт контейнер-оверлей.</summary>
		public static OverlayNode Overlay()
			=> new OverlayNode();

        /// <summary>
        /// Создаёт контейнер с фоном и, опционально, скруглёнными углами.
        /// Возвращает <see cref="OverlayNode"/>, готовый для добавления контента поверх фона.
        /// </summary>
        /// <param name="background">Кисть фона.</param>
        /// <param name="cornerRadius">
        /// Радиус скругления углов. <c>0</c> — прямоугольная маска (<see cref="Clip"/>),
        /// <c>&gt; 0</c> — скруглённый прямоугольник (<see cref="ClipRoundedRect"/>).
        /// </param>
        /// <returns>
        /// <see cref="OverlayNode"/>, в который первым ребёнком добавлен клип с фоном.
        /// Пользователь добавляет контент через <c>.Add(...)</c> — контент рисуется поверх фона.
        /// </returns>
        /// <remarks>
        /// Пример:
        /// <code>
        /// UI.Border(UI.Brush(Color.Gray), cornerRadius: 8)
        ///     .Padding(12)
        ///     .Add(UI.Label("Content", font, brush));
        /// </code>
        /// </remarks>
        public static OverlayNode Border(IBrush background, int cornerRadius = 0)
        {
            if (background == null)
                throw new ArgumentNullException(nameof(background));

            ClipNode clip = cornerRadius > 0
                ? ClipRoundedRect(cornerRadius)
                : Clip();

            clip.Add(Background(background));

            var overlay = new OverlayNode();
            overlay.Children.Add(clip);
            return overlay;
        }

        /// <summary>Создаёт контейнер с фоном заданного цвета.</summary>
        public static OverlayNode Border(Color color, int cornerRadius = 0)
			=> Border(SolidBrush(color), cornerRadius);

        /// <summary>Создаёт текстовую метку.</summary>
        public static LabelNode Label(string text, IFont font, IBrush brush)
			=> new LabelNode(text, font, brush, RequireFactory().CreateLabel(), RequireMeasurer());

		/// <summary>Создаёт узел изображения.</summary>
		public static ImageNode Image(IImage image = null)
			=> new ImageNode(image, RequireFactory().CreateImage());

		/// <summary>Создаёт узел фиксированного размера.</summary>
		public static FixedNode Fixed(int width, int height) => new FixedNode(width, height);

		/// <summary>Создаёт пустой разделитель фиксированного размера.</summary>
		public static FixedNode Spacer(int width, int height) => new FixedNode(width, height);

        /// <summary>Создаёт узел фона заданного цвета.</summary>
        public static BackgroundNode Background(Color color)
            => new BackgroundNode(SolidBrush(color), RequireFactory().CreateLabel());

        /// <summary>Создаёт узел фона с заданной кистью.</summary>
        public static BackgroundNode Background(IBrush brush)
            => new BackgroundNode(brush, RequireFactory().CreateLabel());

        /// <summary>Создаёт прямоугольную маску (по умолчанию).</summary>
        public static ClipNode Clip() => new ClipNode(RequireFactory().CreateRectMask());

		/// <summary>Создаёт круговую маску.</summary>
		public static ClipNode ClipCircle() => new ClipNode(RequireFactory().CreateCircleMask());

		/// <summary>Создаёт эллиптическую маску.</summary>
		public static ClipNode ClipEllipse() => new ClipNode(RequireFactory().CreateEllipseMask());

		/// <summary>Создаёт маску со скруглёнными углами.</summary>
		public static ClipNode ClipRoundedRect(float cornerRadius = 8f)
			=> new ClipNode(RequireFactory().CreateRoundedRectMask(cornerRadius));

		/// <summary>Создаёт маску с произвольной формой.</summary>
		public static ClipNode ClipPath(Func<Rect, IGraphicsPath> pathBuilder = null)
			=> new ClipNode(RequireFactory().CreatePathMask(pathBuilder));

        /// <summary>Создаёт линейный градиент.</summary>
        public static IBrush LinearGradient(Point start, Point end, params GradientStop[] stops)
            => RequireBrushFactory().CreateLinearGradient(start, end, stops);

        /// <summary>Создаёт радиальный градиент.</summary>
        public static IBrush RadialGradient(Point center, Percent radius, params GradientStop[] stops)
            => RequireBrushFactory().CreateRadialGradient(center, radius, stops);

        /// <summary>
        /// Создаёт контейнер с аффинным преобразованием содержимого.
        /// </summary>
        /// <param name="scaleX">Масштаб по X (по умолчанию 1).</param>
        /// <param name="scaleY">Масштаб по Y (по умолчанию 1).</param>
        /// <param name="rotation">Поворот в градусах (по умолчанию 0).</param>
        /// <remarks>
        /// В текущей версии адаптеры не поддерживают трансформации — при
        /// <see cref="DisplayRoot.Build"/> бросается <see cref="NotSupportedException"/>.
        /// </remarks>
        public static TransformNode Transform(float scaleX = 1f, float scaleY = 1f, float rotation = 0f)
            => new TransformNode(new Transform(scaleX, scaleY, rotation, new Point(50, 50)));

        /// <summary>
        /// Создаёт контейнер с аффинным преобразованием и явным origin.
        /// </summary>
        public static TransformNode Transform(float scaleX, float scaleY, float rotation, Point origin)
            => new TransformNode(new Transform(scaleX, scaleY, rotation, origin));

        /// <summary>
        /// Создаёт условный контейнер: показывает <paramref name="trueNode"/> при
        /// <paramref name="condition"/> == <c>true</c>, иначе <paramref name="falseNode"/>.
        /// </summary>
        /// <param name="condition">Реактивное условие.</param>
        /// <param name="trueNode">Узел для <c>true</c>. Может быть <c>null</c>.</param>
        /// <param name="falseNode">Узел для <c>false</c>. Может быть <c>null</c>.</param>
		public static ConditionalNode When(Observable<bool> condition, LayoutNode trueNode, LayoutNode falseNode = null)
			=> new ConditionalNode(condition, trueNode, falseNode);
    }
}