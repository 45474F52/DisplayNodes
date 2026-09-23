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
using System.Collections;
using System.Collections.Generic;

namespace DisplayNodes.Core
{
    /// <summary>
    /// Реактивная коллекция, реализующая <see cref="IList{T}"/> и уведомляющая
    /// подписчиков о любых изменениях состава.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <remarks>
    /// <para>Событие <see cref="Changed"/> вызывается <b>вне</b> внутреннего lock'а —
    /// подписчик может безопасно изменять коллекцию из обработчика (это приведёт
    /// к повторному событию, но не к deadlock'у).</para>
    /// <para>Потокобезопасность: все операции изменения и чтения защищены
    /// внутренним lock'ом. Порядок событий соответствует порядку операций
    /// в одном потоке, но при параллельных изменениях порядок между потоками
    /// не гарантируется.</para>
    /// <para><b>Отложено:</b> автоматическая UI-интеграция через RepeaterNode.
    /// В текущей версии <see cref="ObservableList{T}"/> — только источник данных,
    /// без привязки к <see cref="LayoutNode"/>.</para>
    /// </remarks>
    public class ObservableList<T> : IList<T>, IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<T> _items;

        private bool _disposed;

        /// <summary>
        /// Событие изменения коллекции. Вызывается вне lock'а.
        /// </summary>
        /// <remarks>
        /// Может содержать несколько событий подряд при одной операции
        /// (например, <see cref="Clear"/> шлёт одно <see cref="ListChangeType.Reset"/>).
        /// </remarks>
        public event Action<ListChange<T>> Changed;

        /// <summary>Создаёт пустую коллекцию.</summary>
        public ObservableList()
        {
            _items = new List<T>();
        }

        /// <summary>Создаёт коллекцию с начальным набором элементов.</summary>
        /// <param name="items">Элементы. Не может быть <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="items"/> равен <c>null</c>.</exception>
        public ObservableList(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _items = new List<T>(items);
        }

        /// <inheritdoc/>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _items.Count;
                }
            }
        }

        /// <inheritdoc/>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
        public bool IsReadOnly
        {
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return false;
                }
            }
        }

        /// <summary>
		/// Индексатор. <b>Setter</b> вызывает событие <see cref="ListChangeType.Replace"/>.
		/// </summary>
		/// <param name="index">Индекс элемента.</param>
		/// <exception cref="ArgumentOutOfRangeException">Если индекс вне диапазона.</exception>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _items[index];
                }
            }
            set
            {
                ListChange<T> change;

                lock (_lock)
                {
                    ThrowIfDisposed();

                    T oldItem = _items[index];
                    if (EqualityComparer<T>.Default.Equals(oldItem, value))
                        return;

                    _items[index] = value;
                    change = new ListChange<T>(ListChangeType.Replace, index, index, value);
                }

                RaiseChanged(change);
            }
        }

        /// <summary>
		/// Добавляет элемент в конец. Вызывает событие <see cref="ListChangeType.Add"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void Add(T item)
        {
            int index;
            lock (_lock)
            {
                ThrowIfDisposed();
                index = _items.Count;
                _items.Add(item);
            }
            RaiseChanged(new ListChange<T>(ListChangeType.Add, -1, index, item));
        }

        /// <summary>
        /// Вставляет элемент по индексу. Вызывает событие <see cref="ListChangeType.Insert"/>.
        /// </summary>
        /// <param name="index">Индекс вставки (0..<see cref="Count"/>).</param>
        /// <param name="item">Элемент вставки в коллекцию.</param>
		/// <exception cref="ArgumentOutOfRangeException">Если индекс вне диапазона.</exception>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void Insert(int index, T item)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                _items.Insert(index, item);
            }
            RaiseChanged(new ListChange<T>(ListChangeType.Insert, -1, index, item));
        }

        /// <summary>
        /// Удаляет первое вхождение элемента. Вызывает событие <see cref="ListChangeType.Remove"/>
        /// только если элемент был найден.
        /// </summary>
        /// <returns><c>true</c>, если элемент был удалён.</returns>
        /// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
        public bool Remove(T item)
        {
            int index;
            lock (_lock)
            {
                ThrowIfDisposed();
                index = _items.IndexOf(item);
                if (index < 0)
                    return false;
                _items.RemoveAt(index);
            }

            RaiseChanged(new ListChange<T>(ListChangeType.Remove, index, -1, item));
            return true;
        }

        /// <summary>
		/// Удаляет элемент по индексу. Вызывает событие <see cref="ListChangeType.Remove"/>.
		/// </summary>
		/// <param name="index">Индекс удаления.</param>
		/// <exception cref="ArgumentOutOfRangeException">Если индекс вне диапазона.</exception>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void RemoveAt(int index)
        {
            T removed;
            lock (_lock)
            {
                ThrowIfDisposed();
                removed = _items[index];
                _items.RemoveAt(index);
            }
            RaiseChanged(new ListChange<T>(ListChangeType.Remove, index, -1, removed));
        }

        /// <summary>
		/// Очищает коллекцию. Вызывает одно событие <see cref="ListChangeType.Reset"/>,
		/// если коллекция не была пуста.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void Clear()
        {
            bool wasEmpty;
            lock (_lock)
            {
                ThrowIfDisposed();
                wasEmpty = _items.Count == 0;
                if (!wasEmpty)
                    _items.Clear();
            }

            if (!wasEmpty)
                RaiseChanged(new ListChange<T>(ListChangeType.Reset, -1, -1, default));
        }

        /// <summary>
		/// Перемещает элемент с одной позиции на другую. Вызывает событие
		/// <see cref="ListChangeType.Move"/>.
		/// </summary>
		/// <param name="oldIndex">Текущий индекс.</param>
		/// <param name="newIndex">Новый индекс.</param>
		/// <exception cref="ArgumentOutOfRangeException">Если индексы вне диапазона.</exception>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            T item;
            lock (_lock)
            {
                ThrowIfDisposed();

                if (oldIndex < 0 || oldIndex >= _items.Count)
                    throw new ArgumentOutOfRangeException(nameof(oldIndex));
                if (newIndex < 0 || newIndex >= _items.Count)
                    throw new ArgumentOutOfRangeException(nameof(newIndex));

                item = _items[oldIndex];
                _items.RemoveAt(oldIndex);
                _items.Insert(newIndex, item);
            }

            RaiseChanged(new ListChange<T>(ListChangeType.Move, oldIndex, newIndex, item));
        }

        /// <inheritdoc/>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public bool Contains(T item)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _items.Contains(item);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
        public int IndexOf(T item)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _items.IndexOf(item);
            }
        }

        /// <inheritdoc/>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                _items.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
		/// Возвращает снимок коллекции. Изменения снимка не влияют на оригинал.
		/// </summary>
		/// <remarks>
		/// Используется для безопасного перебора, когда коллекция может
		/// измениться во время итерации.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public List<T> ToList()
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return new List<T>(_items);
            }
        }

        /// <summary>
		/// Возвращает enumerator внутреннего <see cref="List{T}"/>.
		/// </summary>
		/// <remarks>
		/// При модификации коллекции во время <c>foreach</c> бросается
		/// <see cref="InvalidOperationException"/>, как в стандартном <see cref="List{T}"/>.
		/// Для безопасного перебора используйте <see cref="ToList"/>.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Если коллекция была удалена.</exception>
		public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _items.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Уведомляет подписчиков об изменении коллекции.
        /// </summary>
        /// <param name="change">Описание изменения коллекции</param>
        /// <remarks>
        /// Multicast delegate вызывает подписчиков последовательно внутри одного вызова.<br/>
        /// Если один бросает исключение — остальные не вызываются, потому что исключение выходит
        /// за пределы всего multicast-вызова.<br/>
        /// По-этому MulticastDelegate (<see cref="Changed"/>) разбивается на отдельные делегаты
        /// через <see cref="MulticastDelegate.GetInvocationList"/>,<br/>
        /// где каждый вызывается в своём <c>try/catch</c>. Тогда исключение одного не мешает остальным.
        /// </remarks>
        private void RaiseChanged(ListChange<T> change)
        {
            Action<ListChange<T>> handler = Changed;
            if (handler == null)
                return;

            Delegate[] invocationList = handler.GetInvocationList();

            if (invocationList.Length == 1)
            {
                try
                {
                    ((Action<ListChange<T>>)invocationList[0])(change);
                }
                catch { }
                return;
            }

            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<ListChange<T>>)invocationList[i])(change);
                }
                catch { }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
		/// Освобождает ресурсы и делает коллекцию непригодной для использования.
		/// </summary>
		/// <remarks>
		/// После вызова любая операция (кроме повторного <see cref="Dispose"/>)
		/// бросает <see cref="ObjectDisposedException"/>.
		/// </remarks>
		public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _items.Clear();
            }

            Changed = null;
        }
    }
}