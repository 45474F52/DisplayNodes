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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
	/// <summary>Fluent-расширения для настройки <see cref="LabelNode"/>.</summary>
	public static class LabelNodeFluent
	{
		/// <summary>Устанавливает цвет фона.</summary>
		public static LabelNode BackgroundBrush(this LabelNode node, Color color)
		{
			if (UI.BrushFactory == null)
				throw new InvalidOperationException("UI.BrushFactory is not initialized.");

			node.Component.BackgroundBrush = UI.BrushFactory.CreateSolidBrush(color);
			return node;
		}

		/// <summary>Устанавливает абстрактную кисть фона напрямую.</summary>
		public static LabelNode BackgroundBrush(this LabelNode node, IBrush brush)
		{
			node.Component.BackgroundBrush = brush;
			return node;
		}

		/// <summary>Устанавливает режим растяжки текста.</summary>
		public static LabelNode Stretch(this LabelNode node, LabelStretch stretch)
		{
			if (node.Component is ITextLayoutComponent t) t.Stretch = stretch;
			return node;
		}

		/// <summary>Устанавливает выравнивание текста по горизонтали и вертикали.</summary>
		/// <remarks>
		/// Пример: <c>UI.Label("Hello", font, brush).TextAlign(Alignment.Center)</c> —
		/// точно по центру виджета по обеим осям.
		/// </remarks>
		public static LabelNode TextAlign(this LabelNode node, Alignment horizontal, Alignment? vertical = null)
				=> node.Format(UI.TextFormat(horizontal, vertical));

		/// <summary>Устанавливает абстрактный формат текста.</summary>
		public static LabelNode Format(this LabelNode node, ITextFormat format)
		{
			node.Component.Format = format;
			return node;
		}

		/// <summary>Устанавливает метод отрисовки текста.</summary>
		public static LabelNode DrawMethod(this LabelNode node, TextDrawMethod method)
		{
			if (node.Component is ITextLayoutComponent t) t.DrawMethod = method;
			return node;
		}

		/// <summary>Привязывает форматирование текста к реактивному источнику.</summary>
		public static LabelNode BindFormat(this LabelNode node, Observable<ITextFormat> source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			node.Component.Format = source.Value;
			_ = node.AddSubscription(source.Subscribe(v => node.Component.Format = v));
			return node;
		}
	}
}