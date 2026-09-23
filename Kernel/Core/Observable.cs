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
    /// Реактивное свойство. Уведомляет подписчиков об изменении значения.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <remarks>
    /// Используется для привязки данных к виджетам. При изменении <see cref="Value"/>
    /// все подписанные виджеты автоматически обновляются.
    /// <code>
    /// var mode = new Observable&lt;Bitmap&gt;(initialMode);
    /// var imageNode = UI.Image(null).BindBitmap(mode);
    ///
    /// // UI обновится автоматически
    /// mode.Value = newMode;
    /// </code>
    /// </remarks>
    public class Observable<T> : IObservableSource
	{
		private readonly object _lock = new object();
		private readonly List<Action<T>> _subscribers = new List<Action<T>>();

		private T _value;

		/// <summary>
		/// Создаёт Observable с начальным значением.
		/// </summary>
		/// <param name="initialValue">Начальное значение.</param>
		public Observable(T initialValue)
		{
			_value = initialValue;
		}

		/// <summary>Текущее значение. При установке уведомляет подписчиков (в том же потоке).</summary>
		/// <remarks>
		/// Если новое значение равно текущему (через <see cref="EqualityComparer{T}.Default"/>),
		/// уведомление не происходит.
		/// </remarks>
		public T Value
		{
			get
			{
				lock (_lock)
					return _value;
			}
			set
			{
				Action<T>[] toNotify;

				lock (_lock)
				{
					if (EqualityComparer<T>.Default.Equals(_value, value))
						return;

					_value = value;
					toNotify = _subscribers.ToArray();
				}

				foreach (var cb in toNotify)
				{
					try
					{ cb(value); }
					catch { /* не ломаем цепочку */ }
				}
			}
		}

		/// <summary>
		/// Подписывается на изменения значения.
		/// </summary>
		/// <param name="callback">Метод, вызываемый при изменении значения.</param>
		/// <returns>
		/// Объект <see cref="IDisposable"/>, который нужно вызвать для отписки.
		/// Обычно виджеты делают это автоматически в <see cref="IDisposable.Dispose"/>.
		/// </returns>
		public IDisposable Subscribe(Action<T> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			lock (_lock)
				_subscribers.Add(callback);

			return new Subscription(this, callback);
		}

        /// <summary>
        /// Явная реализация <see cref="IObservableSource.Subscribe"/>.<br/>
        /// Оборачивает <see cref="Action{T}"/> (где <c>T</c> — тип значения) в <see cref="Action{T}"/>
        /// (где <c>T</c> — <see cref="object"/>).<br/>
		/// <b>Для value-type это boxing при уведомлении.</b>
        /// </summary>
        IDisposable IObservableSource.Subscribe(Action<object> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            return Subscribe(v => callback(v));
        }

        /// <summary>
        /// Отписывает callback от уведомлений.
        /// </summary>
        internal void Unsubscribe(Action<T> callback)
		{
			lock (_lock)
				_ = _subscribers.Remove(callback);
		}

		/// <summary>
		/// Subscription для отписки от Observable.
		/// </summary>
		private class Subscription : IDisposable
		{
			private Observable<T> _observable;
			private Action<T> _callback;

			public Subscription(Observable<T> observable, Action<T> callback)
			{
				_observable = observable;
				_callback = callback;
			}

			public void Dispose()
			{
				if (_observable != null && _callback != null)
				{
					_observable.Unsubscribe(_callback);
					_observable = null;
					_callback = null;
				}
			}
		}
	}
}