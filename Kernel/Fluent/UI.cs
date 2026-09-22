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
		public static IBrush Brush(Color color) => RequireBrushFactory().CreateSolidBrush(color);

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

		/// <summary>Создаёт контейнер-оверлей.</summary>
		public static OverlayNode Overlay()
			=> new OverlayNode();

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

		/// <summary>Создаёт узел фона.</summary>
		public static BackgroundNode Background(Color color)
			=> new BackgroundNode(color, RequireFactory().CreateLabel(), RequireBrushFactory().CreateSolidBrush);

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
	}
}