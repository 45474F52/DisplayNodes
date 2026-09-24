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
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Widgets
{
    /// <summary>
    /// Контейнер-маска. Обрезает содержимое по форме заданной маски.<br/>
    /// Дети привязываются к маске, а не к внешнему родителю.
    /// </summary>
    public class ClipNode : LayoutNode, IDisposable
    {
        /// <summary>Маска, ограничивающая область отрисовки.</summary>
        public IMaskComponent Mask { get; }

        private bool _disposed;

        /// <summary>Создаёт узел-маску с заданной формой.</summary>
        /// <param name="mask">Экземпляр маски (реализация <see cref="IMaskComponent"/>).</param>
        public ClipNode(IMaskComponent mask)
        {
            Mask = mask ?? throw new ArgumentNullException(nameof(mask));
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size available)
        {
            int maxWidth = 0, maxHeight = 0;
            foreach (LayoutNode child in Children)
            {
                Size size = child.Measure(available);
                maxWidth = Math.Max(maxWidth, size.Width);
                maxHeight = Math.Max(maxHeight, size.Height);
            }

            // Фон внутри клипа измеряется в нулевой доступный слот (BackgroundNode.DesiredSize = 0),
            // поэтому сам клип может получить нулевую высоту. Нулевая форма недопустима для GDI+
            // (Region/AddArc падают с ArgumentException "Недопустимый параметр"), так что
            // возвращаем минимально допустимый размер — он перекрывается реальным при Arrange.
            return new Size(Math.Max(1, maxWidth), Math.Max(1, maxHeight));
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect)
        {
            Mask.Location = finalRect.Point;
            Mask.Size = finalRect.Size;
            foreach (LayoutNode child in Children)
                child.Arrange(finalRect);
        }

        /// <summary>Освобождает ресурсы маски.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            (Mask as IDisposable)?.Dispose();
        }
    }
}