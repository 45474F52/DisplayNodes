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
using System.IO;

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-реализация <see cref="IImageFactory"/>. Создаёт изображение.
    /// </summary>
    public sealed class GdiImageFactory : IImageFactory
    {
        /// <inheritdoc/>
        public IImage CreateFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (Bitmap safetyCopy = new Bitmap(ms))
                {
                    return new GdiImage(new Bitmap(safetyCopy));
                }
            }
        }

        /// <inheritdoc/>
        public IImage CreateFromFile(string path)
        {
            return new GdiImage(new Bitmap(path));
        }

        /// <inheritdoc/>
        public IImage CreateFromStream(Stream stream)
        {
            using (Bitmap safetyCopy = new Bitmap(stream))
            {
                return new GdiImage(new Bitmap(safetyCopy));
            }
        }
    }
}
