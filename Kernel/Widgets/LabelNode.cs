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
		public LabelNode BindForegroundBrush(Observable<IBrush> observable)
		{
			if (observable == null)
				throw new ArgumentNullException(nameof(observable));
			Component.ForegroundBrush = observable.Value;
			_ = AddSubscription(observable.Subscribe(v => Component.ForegroundBrush = v));
			return this;
		}

		/// <summary>Привязывает кисть фона к <see cref="Observable{T}"/>.</summary>
		public LabelNode BindBackgroundBrush(Observable<IBrush> observable)
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
			// Отступы прибавляются к размеру контента, а текст измеряется во внутренней области
			// (Bounds в ApplyBounds — это уже область без padding'а).
			Size inner = available.Deflate(Padding);

			Size content;
			if (_fixedSize.HasValue)
				content = _fixedSize.Value;
			else if (string.IsNullOrEmpty(Component.Text) || Component.Font == null)
				content = Size.Empty;
			else
			{
				int maxWidth = inner.Width == int.MaxValue
					? 0
					: Math.Max(0, inner.Width);
				content = _measurer.MeasureArea(Component.Text, Component.Font, maxWidth);
			}

			Component.Padding = Padding;
			return new Size(content.Width + Padding.Horizontal, content.Height + Padding.Vertical);
		}

		/// <inheritdoc/>
		protected override void ApplyBounds()
		{
			Component.Location = Bounds.Point;
			Component.Size = Bounds.Size;
			Component.Padding = Padding;
		}
	}
}