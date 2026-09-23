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

namespace DisplayNodes.Fluent
{
    /// <summary>
    /// Управляет жизненным циклом корневого контейнера.
    /// </summary>
    public sealed class DisplayRoot : IDisposable
    {
        private readonly IRenderRoot _root;

        /// <summary>Инициализирует <see cref="DisplayRoot"/> через фабрику рендер-корня.</summary>
        /// <param name="factory">
        /// Фабрика корневого контейнера. Инкапсулирует знание о parent-компоненте и специфике бэкенда.
        /// </param>
        public DisplayRoot(IRenderRootFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _root = factory.Create();
        }

        /// <summary>Корневой компонент.</summary>
        public ILayoutComponent Root => _root.Root;

        /// <summary>Пересобирает UI-дерево. Автоматически удаляет предыдущий корень.</summary>
        public void Build(LayoutNode node, Point location, Size size)
        {
            Clear();
            _root.Build(node, location, size);
        }

        /// <summary>Удаляет корневой компонент без создания нового.</summary>
        public void Clear() => _root.Clear();

        /// <summary>Освобождает корневой компонент.</summary>
        public void Dispose() => _root.Dispose();
    }
}