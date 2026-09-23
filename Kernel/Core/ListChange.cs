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

namespace DisplayNodes.Core
{
    /// <summary>
	/// Описание одного изменения в <see cref="ObservableList{T}"/>.
	/// </summary>
	/// <typeparam name="T">Тип элемента коллекции.</typeparam>
	/// <remarks>
	/// <para>Для <see cref="ListChangeType.Add"/> и <see cref="ListChangeType.Insert"/>:
	/// <see cref="OldIndex"/> = -1, <see cref="NewIndex"/> = индекс вставки, <see cref="Item"/> = элемент.</para>
	/// <para>Для <see cref="ListChangeType.Remove"/>:
	/// <see cref="OldIndex"/> = индекс удаления, <see cref="NewIndex"/> = -1, <see cref="Item"/> = удалённый элемент.</para>
	/// <para>Для <see cref="ListChangeType.Replace"/>:
	/// <see cref="OldIndex"/> = <see cref="NewIndex"/> = индекс, <see cref="Item"/> = новый элемент.</para>
	/// <para>Для <see cref="ListChangeType.Move"/>:
	/// <see cref="OldIndex"/> = откуда, <see cref="NewIndex"/> = куда, <see cref="Item"/> = перемещённый элемент.</para>
	/// <para>Для <see cref="ListChangeType.Reset"/>: все поля = -1 / default.</para>
	/// </remarks>
	public readonly struct ListChange<T>
    {
        /// <summary>Тип изменения.</summary>
        public readonly ListChangeType Type;

        /// <summary>Индекс до изменения (-1, если не применимо).</summary>
        public readonly int OldIndex;

        /// <summary>Индекс после изменения (-1, если не применимо).</summary>
        public readonly int NewIndex;

        /// <summary>Элемент, к которому относится изменение (для Reset — default).</summary>
        public readonly T Item;

        /// <summary>Создаёт описание изменения.</summary>
        public ListChange(ListChangeType type, int oldIndex, int newIndex, T item)
        {
            Type = type;
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Item = item;
        }

        /// <inheritdoc/>
        public override string ToString()
            => Type + " [" + OldIndex + " → " + NewIndex + "] " + Item;
    }
}
