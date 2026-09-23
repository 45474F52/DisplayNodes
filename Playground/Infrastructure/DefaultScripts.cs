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

IBrush transparent = UI.SolidBrush(Color.FromArgb(255, 45, 45, 48).FromGdi());
IBrush red = UI.SolidBrush(Color.Red.FromGdi());
IBrush green = UI.SolidBrush(Color.Green.FromGdi());
IBrush blue = UI.SolidBrush(Color.Blue.FromGdi());

var node = UI.Column(8).Margin(150)
    .Add(UI.Label(""Hello, DisplayNodes!"", font, red).BackgroundBrush(transparent))
    .Add(UI.Fixed(0, 4))
    .Add(UI.Row(12)
        .Add(UI.Label(""Status:"", font, blue).BackgroundBrush(transparent))
        .Add(UI.Label(""OK"", font, green).BackgroundBrush(transparent)));
    
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
            .Add(UI.Label(""OK"", font, UI.SolidBrush(Color.Green)))
        );
}";

		/// <summary>
		/// Пустой шаблон — для нового скрипта.
		/// </summary>
		public const string EMPTY =
@"// Метод должен вернуть LayoutNode.
//
// Пример:
// return UI.Label(""Hello"", UI.Font(""Segoe UI"", 14f), UI.SolidBrush(Color.White));

throw new NotImplementedException();";
	}
}
