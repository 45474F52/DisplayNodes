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
using System.Drawing;

using DisplayNodes.Core;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Абстракция области предпросмотра. Инкапсулирует работу с конкретным адаптером отображения.
    /// </summary>
    /// <remarks>
    /// <para><b>Жизненный цикл:</b></para>
    /// <list type="number">
    ///   <item>Создание через фабрику конкретного адаптера.</item>
    ///   <item><see cref="Build"/> — построение дерева в указанном размере.</item>
    ///   <item><see cref="Resize"/> — обновление размера без перестройки дерева.</item>
    ///   <item><see cref="Clear"/> — удаление дерева.</item>
    ///   <item><see cref="Dispose"/> — освобождение ресурсов.</item>
    /// </list>
    /// </remarks>
    internal interface IPreviewHost : IDisposable
    {
        /// <summary>
        /// Текущий размер области предпросмотра.
        /// </summary>
        Size CurrentSize { get; }

        /// <summary>
        /// Строит дерево в области предпросмотра.
        /// Предыдущее дерево автоматически освобождается.
        /// </summary>
        /// <param name="root">Корневой узел дерева.</param>
        void Build(LayoutNode root);

        /// <summary>
        /// Обновляет размер области без перестройки дерева.
        /// Пересчитывает layout существующего дерева.
        /// </summary>
        void Resize(Size size);

        /// <summary>
        /// Удаляет текущее дерево (без создания нового).
        /// </summary>
        void Clear();

        /// <summary>
        /// Экспортирует текущее отображение в <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>Bitmap с текущим preview или <c>null</c>, если экспорт невозможен.</returns>
        Bitmap ExportToBitmap();
    }
}