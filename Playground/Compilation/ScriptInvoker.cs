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
using System.Reflection;

using DisplayNodes.Core;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Загружает скомпилированную сборку и вызывает метод Build().
	/// </summary>
	internal sealed class ScriptInvoker
	{
		/// <summary>
		/// Загрузить сборку из байт и вызвать UserScript.Build().
		/// Возвращает LayoutNode или null при ошибке.
		/// </summary>
		public LayoutNode Invoke(byte[] assemblyBytes, out string error)
		{
			error = null;

			try
			{
				// Загрузка через массив байт используется специально,
				// потому что загрузка по имени кэширует сборки в AppDomain,
				// и повторная компиляция не подхватит изменения.
				Assembly assembly = Assembly.Load(assemblyBytes);

				Type type = assembly.GetType("UserScript");
				if (type == null)
				{
					error = "Скомпилированный тип UserScript не найден.";
					return null;
				}

				MethodInfo method = type.GetMethod("Build");
				if (method == null)
				{
					error = "Метод UserScript.Build не найден.";
					return null;
				}

				try
				{
					return (LayoutNode)method.Invoke(null, null);
				}
				catch (Exception ex)
				{
					Exception inner = ex.InnerException ?? ex;
					error = inner.Message + Environment.NewLine + inner.StackTrace;
					return null;
				}
			}
			catch (Exception ex)
			{
				error = "Ошибка загрузки сборки: " + ex.Message;
				return null;
			}
		}
	}
}
