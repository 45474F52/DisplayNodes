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

using System.Windows.Forms;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter.Component.Masks;

namespace DisplayNodes.WinFormsAdapter.Components
{
	internal abstract class ComponentBase : IRenderComponent
	{
		public Control Inner { get; }

		protected ComponentBase(Control inner)
		{
			Inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
		}

		private IRenderComponent _parentAdapter;
		public IRenderComponent Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				Inner.Parent = ComponentHelper.ExtractInner(value);
			}
		}

		public Point Location
		{
			get => new Point(Inner.Location.X, Inner.Location.Y);
			set => Inner.Location = new System.Drawing.Point(value.X, value.Y);
		}

		public Size Size
		{
			get => new Size(Inner.Size.Width, Inner.Size.Height);
			set => Inner.Size = new System.Drawing.Size(value.Width, value.Height);
		}

		public bool Visible
		{
			get => Inner.Visible;
			set => Inner.Visible = value;
		}
	}

	/// <summary>Хелпер для извлечения внутреннего <see cref="Control"/> из адаптера.</summary>
	internal static class ComponentHelper
	{
		/// <summary>
		/// Извлекает внутренний <see cref="Control"/> из адаптера.
		/// Возвращает <c>null</c>, если <paramref name="component"/> не является WinForms-адаптером.
		/// </summary>
		public static Control ExtractInner(IRenderComponent component)
		{
			if (component is ComponentBase c)
				return c.Inner;
			if (component is MaskBase m)
				return m;
			return null;
		}
	}
}
