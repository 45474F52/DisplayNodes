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

using DisplayNodes.Core.Rendering;
using System.Drawing;

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-реализация <see cref="IFontFactory"/>. Создаёт шрифт из необходимых аргументов.
    /// </summary>
    public sealed class GdiFontFactory : IFontFactory
    {
        /// <inheritdoc/>
        public IFont Create(string family, float size, bool bold = false, bool italic = false)
        {
            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            return new GdiFont(new Font(family, size, style, GraphicsUnit.Point));
        }
    }
}
