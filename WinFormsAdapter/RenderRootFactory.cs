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
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Фабрика <see cref="IRenderRoot"/> для WinForms.
    /// </summary>
    public sealed class RenderRootFactory : IRenderRootFactory
    {
        private readonly Control _parent;

        /// <summary>Создаёт фабрику корневых контейнеров.</summary>
        /// <param name="parent">Родительский компонент.</param>
        public RenderRootFactory(Control parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <inheritdoc/>
        public IRenderRoot Create() => new RenderRoot(_parent);
    }
}
