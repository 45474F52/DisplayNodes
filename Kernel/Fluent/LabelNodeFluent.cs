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
		public static LabelNode FullBrush(this LabelNode node, Color color)
		{
			if (UI.BrushFactory == null)
				throw new InvalidOperationException("UI.BrushFactory is not initialized.");

			node.Component.BackgroundBrush = UI.BrushFactory.CreateSolidBrush(color);
			return node;
		}

		/// <summary>Устанавливает абстрактную кисть фона напрямую.</summary>
		public static LabelNode FullBrush(this LabelNode node, IBrush brush)
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