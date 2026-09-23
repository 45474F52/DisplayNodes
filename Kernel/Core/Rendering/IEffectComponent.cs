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
    /// Контракт для компонентов, поддерживающих эффекты обработки изображения.
    /// </summary>
    public interface IEffectComponent
    {
        /// <summary>Прозрачность</summary>
        double Opacity { get; set; }

        /// <summary>Яркость</summary>
        double Brightness { get; set; }

        /// <summary>Контраст</summary>
        double Contrast { get; set; }

        /// <summary>
        /// Тень компонента. <c>null</c> — без тени.
        /// </summary>
        /// <remarks>
        /// <para><b>Ограничение.</b> В текущей версии адаптеры не поддерживают тень.
        /// Установка не-null значения приводит к <see cref="System.NotSupportedException"/>
        /// в setter'е компонента.</para>
        /// </remarks>
        Shadow? Shadow { get; set; }
    }
}
