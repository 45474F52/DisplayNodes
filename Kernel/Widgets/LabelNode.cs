using System;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Widgets
{
	/// <summary>
	/// Виджет для отображения текста.
	/// </summary>
	public class LabelNode : WidgetNode
	{
		/// <summary>Базовый компонент текстовой метки.</summary>
		public new ILabelComponent Component => (ILabelComponent)base.Component;

		private readonly ITextMeasurer _measurer;
		private Size? _fixedSize;

		/// <summary>Создаёт виджет текстовой метки.</summary>
		/// <param name="text">Отображаемый текст.</param>
		/// <param name="font">Шрифт текста.</param>
		/// <param name="brush">Кисть для цвета текста.</param>
		/// <param name="component">Компонент рендерера (создаётся фабрикой).</param>
		/// <param name="measurer">Измеритель текста.</param>
		public LabelNode(string text, IFont font, IBrush brush,
						 ILabelComponent component, ITextMeasurer measurer)
			: base(component)
		{
			_measurer = measurer ?? throw new ArgumentNullException(nameof(measurer));
			Component.Text = text ?? string.Empty;
			Component.Font = font;
			Component.ForegroundBrush = brush;
		}

		/// <summary>Привязывает текст к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindText(Observable<string> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));
			Component.Text = observable.Value ?? string.Empty;
			_ = AddSubscription(observable.Subscribe(v => Component.Text = v ?? string.Empty));
			return this;
		}

		/// <summary>Привязывает кисть текста к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindBrush(Observable<IBrush> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));
			Component.ForegroundBrush = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => Component.ForegroundBrush = v));
			return this;
		}

		/// <summary>Привязывает кисть фона к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindFullBrush(Observable<IBrush> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));
			Component.BackgroundBrush = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => Component.BackgroundBrush = v));
			return this;
		}

		/// <summary>Привязывает шрифт текста к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindFont(Observable<IFont> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));

			Component.Font = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => Component.Font = v));
			return this;
		}

		/// <summary>Привязывает способ отрисовки текста к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindDrawMethod(Observable<TextDrawMethod> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));

			if (!(Component is ITextLayoutComponent t))
				throw new ArgumentException("Could not bind Draw Method to ILabelComponent (it is not realising ITextLayoutComponent interface)");

			t.DrawMethod = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => t.DrawMethod = v));
			return this;
		}

		/// <summary>Привязывает способ растягивания текста к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindStretch(Observable<LabelStretch> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));

            if (!(Component is ITextLayoutComponent t))
                throw new ArgumentException("Could not bind Label Stretch to ILabelComponent (it is not realising ITextLayoutComponent interface)");

            t.Stretch = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => t.Stretch = v));
			return this;
		}

		/// <summary>
		/// Устанавливает фиксированный размер. Если не задан, размер вычисляется автоматически по тексту.
		/// </summary>
		public LabelNode SetSize(int width, int height)
		{
			_fixedSize = new Size(width, height);
			return this;
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available)
		{
			if (_fixedSize.HasValue)
				return _fixedSize.Value;
			if (string.IsNullOrEmpty(Component.Text) || Component.Font == null)
				return Size.Empty;
			int maxWidth = available.Width == int.MaxValue
				? 0
				: Math.Max(0, available.Width);
			return _measurer.MeasureArea(Component.Text, Component.Font, maxWidth);
		}

		/// <inheritdoc/>
		protected override void ApplyBounds()
		{
			Component.Location = Bounds.Point;
			Component.Size = Bounds.Size;
		}
	}
}