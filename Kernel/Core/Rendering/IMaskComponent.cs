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

using DisplayNodes.Widgets;

namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Контракт компонента-маски для обрезки содержимого.
    /// </summary>
    /// <remarks>
    /// <para>Реализуется адаптерами рендерера (например, <c>GdiRectMask</c>, <c>GdiCircleMask</c>)
    /// и используется виджетом <see cref="ClipNode"/> для ограничения области отрисовки дочерних элементов.</para>
    /// <para>Маска является <see cref="IRenderComponent"/> и участвует в дереве компонентов:
    /// дети <see cref="ClipNode"/> привязываются к маске как к родителю, а не к внешнему контейнеру.</para>
    /// </remarks>
    public interface IMaskComponent : IRenderComponent
    {
    }
}