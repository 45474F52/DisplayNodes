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

namespace DisplayNodes.Playground.Editor
{
    /// <summary>
    /// Тип элемента автодополнения.
    /// </summary>
    internal enum CompletionItemKind
    {
        Method,
        Property,
        Field,
        Type
    }

    /// <summary>
    /// Элемент автодополнения с сигнатурой и документацией.
    /// </summary>
    internal sealed class CompletionItem
    {
        /// <summary>Имя для отображения в списке.</summary>
        public string Name { get; set; }

        /// <summary>Сигнатура (для методов — с параметрами, для свойств — с типом).</summary>
        public string Signature { get; set; }

        /// <summary>Тип возврата (или тип свойства/поля).</summary>
        public string ReturnType { get; set; }

        /// <summary>XML-документация (summary) или пустая строка.</summary>
        public string Documentation { get; set; }

        /// <summary>Тип элемента.</summary>
        public CompletionItemKind Kind { get; set; }

        /// <inheritdoc/>
        public override string ToString() => Name;
    }
}