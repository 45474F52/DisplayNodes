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

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Шаблон обёртки для компиляции пользовательского скрипта.
	/// </summary>
	internal static class ScriptWrapper
	{
		/// <summary>
		/// Количество строк до начала пользовательского кода.
		/// Используется ErrorParser для корректного отображения номеров строк в ошибках компиляции.
		/// </summary>
		/// <remarks>
		/// Обновлять вручную при изменении шаблона Template.
		/// Считается как номер строки, на которой начинается пользовательский код.
		/// </remarks>
		public const int HEADER_LINE_COUNT = 17;

		/// <summary>
		/// Шаблон обёртки с плейсхолдером {0} для пользовательского кода.
		/// </summary>
		public const string TEMPLATE =
@"using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using DisplayNodes.Widgets;
using DisplayNodes.Gdi;
using LibDisplayDrawing;
using Color = System.Drawing.Color;

public static class UserScript
{
    public static LayoutNode Build()
    {
{0}
    }
}";
		public static string GetWrappedCode(string code) => TEMPLATE.Replace("{0}", code);
	}
}
