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
using System.Collections.Generic;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Helpers
{
    /// <summary>
    /// Методы расширения для рекурсивного обхода и управления жизненным циклом деревьев <see cref="LayoutNode"/>.
    /// </summary>
    public static class LayoutNodeExtensions
    {
        /// <summary>
        /// Рекурсивно собирает все <see cref="IRenderComponent"/> в данном дереве узлов.
        /// </summary>
        public static void CollectComponents(this LayoutNode node, ICollection<IRenderComponent> list)
        {
            if (node is WidgetNode w)
                list.Add(w.Component);
            else if (node is ClipNode clip)
                list.Add(clip.Mask);

            foreach (var child in node.Children)
                child.CollectComponents(list);
        }

        /// <summary>
        /// Рекурсивно освобождает ресурсы всех узлов в дереве (в порядке «снизу вверх»).
        /// </summary>
        public static void DisposeTree(this LayoutNode node)
        {
            foreach (var child in node.Children)
                child.DisposeTree();
            if (node is IDisposable d)
                d.Dispose();
        }
    }
}