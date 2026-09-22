namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Шаблоны скриптов для DisplayNodes Playground.
	/// </summary>
	internal static class DefaultScripts
	{
		/// <summary>
		/// Базовый пример: колонка с метками.
		/// </summary>
		public const string HELLO =
@"IFont font = UI.Font(""Segoe UI"", 14f);
IBrush white = UI.Brush(Color.White.FromGdi());
IBrush green = UI.Brush(Color.LimeGreen.FromGdi());
IBrush gray = UI.Brush(Color.Gray.FromGdi());

var node = UI.Column(8)
    .Add(UI.Label(""Hello, DisplayNodes!"", font, white))
    .Add(UI.Fixed(0, 4))
    .Add(UI.Row(12)
        .Add(UI.Label(""Status:"", font, gray))
        .Add(UI.Label(""OK"", font, green)));
    
return node;";

		/// <summary>
		/// Пример с фоном и крупным текстом.
		/// </summary>
		public const string WITH_BACKGROUND =
@"IFont font = new Font(FontFamily.GenericMonospace, 28, FontStyle.Bold).Wrap();
using (SolidBrush brush = new SolidBrush(Color.Black))
{
    return UI.Column(12)
        .Padding(20)
        .Add(UI.Background(Color.FromArgb(240, 240, 245).FromGdi()))
        .Add(UI.Label(""Hello, DisplayNodes!"", font, brush.Wrap()))
        .Add(UI.Row(8)
            .Add(UI.Label(""Status:"", font, brush))
            .Add(UI.Label(""OK"", font, UI.Brush(Color.Green)))
        );
}";

		/// <summary>
		/// Пустой шаблон — для нового скрипта.
		/// </summary>
		public const string EMPTY =
@"// Метод должен вернуть LayoutNode.
//
// Пример:
// return UI.Label(""Hello"", UI.Font(""Segoe UI"", 14f), UI.Brush(Color.White));

throw new NotImplementedException();";
	}
}
