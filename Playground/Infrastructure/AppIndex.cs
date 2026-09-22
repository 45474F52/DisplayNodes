using System;
using System.Collections.Generic;
using System.Reflection;

namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Реестр типов и их публичных членов, собранный из рефлексии.
	/// Потокобезопасный: все публичные методы защищены lock'ом.
	/// </summary>
	internal static class ApiIndex
	{
		private static readonly object _lock = new object();

		// Имя типа → список имён членов.
		private static readonly Dictionary<string, List<string>> _byType =
			new Dictionary<string, List<string>>(StringComparer.Ordinal);

		// Все типы по имени, чтобы быстро находить нужный.
		private static readonly Dictionary<string, Type> _types =
			new Dictionary<string, Type>(StringComparer.Ordinal);

		private static readonly HashSet<string> _namespaces =
			new HashSet<string>(StringComparer.Ordinal);

		public static bool IsInitialized { get; private set; }

		public static void Initialize(params Assembly[] assemblies)
		{
			lock (_lock)
			{
				if (IsInitialized)
					return;

				IsInitialized = true;

				foreach (var asm in assemblies)
				{
					if (asm == null)
						continue;

					Type[] types;

					try
					{
						types = asm.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						types = ex.Types;
					}
					foreach (Type t in types)
					{
						if (t == null)
							continue;

						if (!t.IsPublic && !t.IsNestedPublic)
							continue;

						if (t.IsGenericTypeDefinition)
							continue;

						RegisterTypeInternal(t);
					}
				}
			}
		}

		/// <summary>
		/// Зарегистрировать один тип (например, String/Int32 из mscorlib
		/// по требованию). Полезно, чтобы не сканировать всю mscorlib.
		/// </summary>
		public static void RegisterType(Type t)
		{
			if (t == null)
				return;

			lock (_lock)
			{
				if (_byType.ContainsKey(t.Name))
					return;

				RegisterTypeInternal(t);
			}
		}

		private static void RegisterTypeInternal(Type t)
		{
			// Вызывается только под lock'ом.
			_types[t.Name] = t;

			// Собираем namespace-ы: для типа System.Drawing.Color
			// добавляем System, System.Drawing.
			if (!string.IsNullOrEmpty(t.Namespace))
			{
				string ns = t.Namespace;
				_namespaces.Add(ns);
				// Все префиксы: A.B.C → A, A.B, A.B.C
				int idx = 0;
				while (true)
				{
					idx = ns.IndexOf('.', idx);
					if (idx < 0)
						break;
					_namespaces.Add(ns.Substring(0, idx));
					idx++;
				}
			}

			// Члены типа.
			var members = new List<string>();
			MethodInfo[] methods = t.GetMethods(BindingFlags.Public
				| BindingFlags.Static | BindingFlags.Instance
				| BindingFlags.DeclaredOnly);
			foreach (var m in methods)
			{
				if (m.IsSpecialName)
					continue;
				if (!members.Contains(m.Name))
					members.Add(m.Name);
			}

			PropertyInfo[] props = t.GetProperties(BindingFlags.Public
				| BindingFlags.Static | BindingFlags.Instance
				| BindingFlags.DeclaredOnly);
			foreach (var p in props)
			{
				if (!members.Contains(p.Name))
					members.Add(p.Name);
			}

			members.Sort(StringComparer.Ordinal);
			_byType[t.Name] = members;
		}

		/// <summary>
		/// Возвращает методы и свойства типа по имени, или null если тип неизвестен.
		/// Возвращает копию списка — безопасна для использования вне lock'а.
		/// </summary>
		public static List<string> GetMembers(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
				return null;

			lock (_lock)
			{
				if (_byType.TryGetValue(typeName, out List<string> members))
					return new List<string>(members);

				Type t = Type.GetType(typeName);
				if (t != null)
				{
					RegisterTypeInternal(t);
					if (_byType.TryGetValue(typeName, out members))
						return new List<string>(members);
				}
				return null;
			}
		}

		/// <summary>Известен ли такой тип.</summary>
		public static bool KnowsType(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
				return false;

			lock (_lock)
			{
				return _types.ContainsKey(typeName);
			}
		}

		/// <summary>
		/// Знает ли ApiIndex такой идентификатор как тип ИЛИ namespace.
		/// Используется лексером для подсветки квалифицированных имён.
		/// </summary>
		public static bool KnowsTypeOrNamespace(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			lock (_lock)
			{
				return _types.ContainsKey(name) || _namespaces.Contains(name);
			}
		}

		public static Type FindType(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			lock (_lock)
			{
				return _types.TryGetValue(name, out Type t) ? t : null;
			}
		}

        /// <summary>
        /// Является ли указанный тип интерфейсом.
        /// </summary>
        public static bool IsInterface(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;

            lock (_lock)
            {
                return _types.TryGetValue(typeName, out Type t) && t.IsInterface;
            }
        }
    }
}
