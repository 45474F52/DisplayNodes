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
using DisplayNodes.Fluent;
using DisplayNodes.WinFormsAdapter.Components;
using System;
using System.Windows.Forms;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Реализация <see cref="IRenderRoot"/> для WinForms.
    /// </summary>
    internal sealed class RenderRoot : IRenderRoot
    {
        private readonly Control _parent;
        private Layout _rootAdapter;

        public RenderRoot(Control parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ILayoutComponent Root => _rootAdapter;

        public void Build(LayoutNode node, Point location, Size size)
        {
            Clear();

            var panel = new Panel
            {
                Location = new System.Drawing.Point(location.X, location.Y),
                Size = new System.Drawing.Size(size.Width, size.Height),
                BackColor = System.Drawing.Color.Transparent,
            };

            _rootAdapter = new Layout(panel);
            node.Apply(_rootAdapter, location, size);
            _parent.Controls.Add(panel);
        }

        public void Clear()
        {
            if (_rootAdapter == null)
                return;

            _parent.Controls.Remove((Panel)_rootAdapter.Inner);
            _rootAdapter.Dispose();
            _rootAdapter = null;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
