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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.WinFormsAdapter.Components;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter.Component.Masks
{
	internal class MaskBase : Control, IMaskComponent
	{
		private IRenderComponent _parentAdapter;

		IRenderComponent IRenderComponent.Parent
		{
			get => _parentAdapter;
			set
			{
				_parentAdapter = value;
				Parent = ComponentHelper.ExtractInner(value);
			}
        }

        Point IRenderComponent.Location
        {
            get => new Point(Location.X, Location.Y);
            set => Location = new System.Drawing.Point(value.X, value.Y);
        }

        Size IRenderComponent.Size
        {
            get => new Size(Size.Width, Size.Height);
            set => Size = new System.Drawing.Size(value.Width, value.Height);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        // Пока маска не получила ненулевой размер (первый кадр лэйаута ещё не прошёл),
        // вместо Region строим пустую — иначе формы с нулевыми габаритами недопустимы для GDI+.
        private bool HasValidSize => Width > 0 && Height > 0;

        protected virtual System.Drawing.Region CreateRegion()
        {
            return new System.Drawing.Region(new System.Drawing.Rectangle(0, 0, Width, Height));
        }

        private void UpdateRegion()
        {
            Region?.Dispose();
            Region = HasValidSize ? CreateRegion() : new System.Drawing.Region(System.Drawing.Rectangle.Empty);
        }
    }
}
