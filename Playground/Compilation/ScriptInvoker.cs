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
