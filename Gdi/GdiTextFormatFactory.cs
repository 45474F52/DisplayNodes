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

using System.Drawing;

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Gdi
{
        /// <summary>
        /// GDI-реализация <see cref="ITextFormatFactory"/>. Создаёт <see cref="GdiTextFormat"/>
        /// поверх <see cref="StringFormat"/>, заполняя оба измерения: Alignment и LineAlignment.
        /// </summary>
        public sealed class GdiTextFormatFactory : ITextFormatFactory
        {
                /// <inheritdoc/>
                public ITextFormat Create(Alignment horizontal, Alignment vertical)
                        => new StringFormat
                        {
                                Alignment = ToStringAlignment(horizontal),
                                LineAlignment = ToStringAlignment(vertical)
                        }.Wrap();

                private static StringAlignment ToStringAlignment(Alignment alignment)
                {
                        switch (alignment)
                        {
                                case Alignment.Center: return StringAlignment.Center;
                                case Alignment.End: return StringAlignment.Far;
                                case Alignment.Start:
                                default: return StringAlignment.Near;
                        }
                }
        }
}
