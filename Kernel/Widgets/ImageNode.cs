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
	/// <summary>Виджет для отображения изображения.</summary>
	public class ImageNode : WidgetNode
	{
		/// <summary>Базовый компонент изображения.</summary>
		public new IImageComponent Component => (IImageComponent)base.Component;

		private Size? _fixedSize;

		/// <summary>Создаёт виджет изображения.</summary>
		/// <param name="bitmap">Исходное изображение.</param>
		/// <param name="component">Компонент рендерера (создаётся фабрикой).</param>
		public ImageNode(IImage bitmap, IImageComponent component)
			: base(component)
		{
			Component.Image = bitmap;
		}

		/// <summary>Привязывает изображение к реактивному источнику.</summary>
		public ImageNode BindBitmap(Observable<IImage> source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			Component.Image = source.Value;
			_ = AddSubscription(source.Subscribe(v => Component.Image = v));
			return this;
		}

		/// <summary>Привязывает режим отображения изображения к реактивному источнику.</summary>
		public ImageNode BindSizeMode(Observable<ImageSizeMode> source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			Component.SizeMode = source.Value;
			_ = AddSubscription(source.Subscribe(v => Component.SizeMode = v));
			return this;
		}

		/// <summary>Устанавливает фиксированный размер, игнорируя реальный размер изображения.</summary>
		public ImageNode SetSize(int width, int height)
		{
			_fixedSize = new Size(width, height);
			return this;
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available)
		{
			if (_fixedSize.HasValue)
				return _fixedSize.Value;

			if (Component.Image != null)
				return new Size(Component.Image.Width, Component.Image.Height);

			return Size.Empty;
		}

		/// <inheritdoc/>
		protected override void ApplyBounds()
		{
			Component.Location = Bounds.Point;
			Component.Size = Bounds.Size;
		}
	}
}