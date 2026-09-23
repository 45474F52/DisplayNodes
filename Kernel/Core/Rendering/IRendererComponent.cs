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

namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Базовый контракт компонента рендерера
    /// </summary>
    /// <remarks>
    /// Установка <see cref="Parent"/> в адаптере автоматически синхронизируется
    /// с коллекцией детей родительского компонента.
    /// Установка <c>Parent = null</c> удаляет компонент из старого родителя.
    /// </remarks>
    public interface IRenderComponent
    {
        /// <summary>Родительский компонент. Setter автоматически управляет вхождением в Controls родителя.</summary>
        IRenderComponent Parent { get; set; }

        /// <summary>Позиция компонента относительно родителя.</summary>
        Point Location { get; set; }

        /// <summary>Размер компонента.</summary>
        Size Size { get; set; }

        /// <summary>Видимость компонента.</summary>
        bool Visible { get; set; }
    }
}