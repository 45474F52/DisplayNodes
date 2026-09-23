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

namespace DisplayNodes.Core
{
    /// <summary>
    /// Реактивное свойство, значение которого вычисляется на основе одного
    /// или нескольких других <see cref="IObservableSource"/>.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <remarks>
    /// <para>При изменении любой зависимости вызывается <c>compute</c>, и если новое
    /// значение отличается от текущего — уведомляются подписчики.</para>
    /// <para>Аналог <c>computed</c> в Vue.js, <c>derived</c> в Svelte, <c>Select</c> в Rx.NET.</para>
    /// <example>
    /// <code>
    /// var firstName = new Observable&lt;string&gt;("Ivan");
    /// var lastName = new Observable&lt;string&gt;("Petrov");
    ///
    /// var fullName = new ComputedObservable&lt;string&gt;(
    ///     () => firstName.Value + " " + lastName.Value,
    ///     firstName, lastName);
    ///
    /// // fullName.Value == "Ivan Petrov"
    /// firstName.Value = "Petr";
    /// // fullName.Value == "Petr Petrov" — автоматически
    /// </code>
    /// </example>
    /// </remarks>
    public class ComputedObservable<T> : Observable<T>, IDisposable
    {
        private readonly object _lock = new object();
        private readonly Func<T> _compute;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private bool _disposed;

        /// <summary>
        /// Создаёт ComputedObservable, вычисляющий значение через <paramref name="compute"/>
        /// на основе указанных зависимостей.
        /// </summary>
        /// <param name="compute">
        /// Функция, вычисляющая значение. Вызывается при создании и при каждом изменении
        /// любой зависимости.
        /// </param>
        /// <param name="dependencies">
        /// Реактивные источники, от которых зависит вычисляемое значение.
        /// Может быть пустым — тогда значение вычисляется один раз и не пересчитывается.
        /// </param>
        /// <exception cref="ArgumentNullException">Если <paramref name="compute"/> равен <c>null</c>.</exception>
        public ComputedObservable(Func<T> compute, params IObservableSource[] dependencies)
            : base(ComputeInitial(compute))
        {
            _compute = compute;

            if (dependencies != null)
            {
                foreach (IObservableSource source in dependencies)
                {
                    if (source == null)
                        continue;

                    _subscriptions.Add(source.Subscribe(OnDependencyChanged));
                }
            }
        }

        /// <summary>
        /// Пересчитывает значение на основе текущих значений зависимостей.
        /// Уведомляет подписчиков, если значение изменилось.
        /// </summary>
        /// <remarks>
        /// Обычно вызывать вручную не нужно — метод вызывается автоматически
        /// при изменении зависимостей. Может быть полезен, если зависимость
        /// изменилась в обход <see cref="Observable{T}.Value"/> (например, была
        /// заменена на месте).
        /// </remarks>
        public void Refresh()
        {
            OnDependencyChanged(null);
        }

        private void OnDependencyChanged(object _)
        {
            if (_disposed)
                return;

            // Внешний try/catch — чтобы исключение в compute не сломало цепочку уведомлений от Observable
            T newValue;
            try
            {
                newValue = _compute();
            }
            catch
            {
                // Игнорируем ошибку вычисления — старое значение остаётся.
                // Подписчики не уведомляются.
                return;
            }

            Value = newValue;
        }

        private static T ComputeInitial(Func<T> compute)
        {
            if (compute == null)
                throw new ArgumentNullException(nameof(compute));

            return compute();
        }

        /// <summary>
        /// Отписывается от всех зависимостей.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;

                foreach (IDisposable sub in _subscriptions)
                {
                    try
                    { sub?.Dispose(); }
                    catch { }
                }

                _subscriptions.Clear();
            }
        }
    }
}