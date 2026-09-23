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
using DisplayNodes.WinFormsAdapter.Components.Masks;
using System;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Реализация <see cref="IWidgetFactory"/>. Создаёт обёртки над WinForms-контролами.
    /// </summary>
    internal sealed class WidgetFactory : IWidgetFactory
    {
        public ILabelComponent CreateLabel() => new Label();

        public IImageComponent CreateImage() => new Image();

        public IMaskComponent CreateRectMask() => new RectMask();

        public IMaskComponent CreateCircleMask() => new CircleMask();

        public IMaskComponent CreateEllipseMask() => new EllipseMask();

        public IMaskComponent CreateRoundedRectMask(float cornerRadius)
            => new RoundedRectMask(cornerRadius);

        public IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder)
            => new PathMask(pathBuilder);
    }
}
