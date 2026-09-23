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

using System.IO;

namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Фабрика изображений. Абстрагирует создание изображений от бэкенда.
    /// </summary>
    public interface IImageFactory
    {
        /// <summary>Создаёт изображение из файла.</summary>
        IImage CreateFromFile(string path);

        /// <summary>Создаёт изображение из потока.</summary>
        IImage CreateFromStream(Stream stream);

        /// <summary>Создаёт изображение из массива байт.</summary>
        IImage CreateFromBytes(byte[] bytes);
    }
}
