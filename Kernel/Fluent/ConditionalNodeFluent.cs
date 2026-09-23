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
using System;

namespace DisplayNodes.Fluent
{
    /// <summary>
    /// Fluent-расширения для настройки <see cref="ConditionalNode"/>.
    /// </summary>
    public static class ConditionalNodeFluent
    {
        /// <summary>
        /// Подписывается на событие изменения реактивного условия <see cref="ConditionalNode.ConditionChanged"/>.
        /// </summary>
        public static ConditionalNode OnChanged(this ConditionalNode node, Action<bool> handler)
        {
            node.ConditionChanged += handler;
            return node;
        }
    }
}
