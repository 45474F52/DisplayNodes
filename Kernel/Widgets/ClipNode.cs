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
            // Отступы уменьшают доступную область детей и прибавляются к итоговому размеру
            // (как у остальных контейнеров).
            Size inner = available.Deflate(Padding);
            int maxWidth = 0, maxHeight = 0;
            foreach (LayoutNode child in Children)
            {
                Size size = child.Measure(inner);
                maxWidth = Math.Max(maxWidth, size.Width);
                maxHeight = Math.Max(maxHeight, size.Height);
            }

            // ВАЖНО: не подменять нулевой результат единицей. Клип, содержащий только фон
            // (UI.Border без контента), по контракту имеет DesiredSize 0x0; «защита от нуля»
            // на уровне измерений ломала лэйаут (колонка растягивалась на весь слот) и не
            // решала проблему GDI+ — валидность формы обеспечивает MaskBase: регион строится
            // только при ненулевом размере контрола, а до первого реального Arrange
            // контрол скрыт (см. WinFormsAdapter/Components/Masks/MaskBase.cs).
            return new Size(maxWidth + Padding.Horizontal, maxHeight + Padding.Vertical);
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect)
        {
            // Маска и дети получают СЛОТ целиком, а не Bounds после выравнивания:
            // иначе клип с DesiredSize 0x0 (UI.Border без контента), выровненный по Start,
            // схлопывался бы в точку — «невидимый» фон. Фон внутри обязан растянуться на весь слот.
            Rect slot = finalRect.Deflate(Margin);
            Bounds = slot;

            Mask.Location = slot.Point;
            Mask.Size = slot.Size;
            Rect inner = slot.Deflate(Padding);
            foreach (LayoutNode child in Children)
                child.Arrange(inner);
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