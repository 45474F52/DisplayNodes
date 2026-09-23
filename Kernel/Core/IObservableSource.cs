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

namespace DisplayNodes.Core
{
    /// <summary>
    /// None-generic представление реактивного источника. Позволяет подписываться
    /// на <see cref="Observable{T}"/> с разными <c>T</c> единообразно.
    /// </summary>
    /// <remarks>
    /// <para>Основное назначение — использование в <see cref="ComputedObservable{T}"/>:
    /// из-за инвариантности generic'ов в C# нельзя передать <see cref="Observable{T}"/>
    /// с разными типами в один массив <c>Observable&lt;object&gt;[]</c>. Через
    /// <see cref="IObservableSource"/> это возможно.</para>
    /// <para>Значения value-type будут упакованы (boxing) при уведомлении.
    /// Для ссылочных типов — без оверхеда.</para>
    /// </remarks>
    public interface IObservableSource
    {
        /// <summary>
        /// Подписывается на изменения значения.
        /// </summary>
        /// <param name="callback">Метод, вызываемый при изменении значения.</param>
        /// <returns>
        /// Объект <see cref="IDisposable"/>, который нужно вызвать для отписки.
        /// </returns>
        IDisposable Subscribe(Action<object> callback);
    }
}