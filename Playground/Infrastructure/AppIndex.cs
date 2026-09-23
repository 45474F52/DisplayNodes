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
using System.Reflection;

using DisplayNodes.Playground.Editor;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Реестр типов и их публичных членов, собранный из рефлексии.
    /// Потокобезопасный: все публичные методы защищены lock'ом.
    /// </summary>
    internal static class ApiIndex
    {
        private static readonly object _lock = new object();

        // Имя типа → список членов (методы, свойства, поля).
        private static readonly Dictionary<string, List<CompletionItem>> _byType =
            new Dictionary<string, List<CompletionItem>>(StringComparer.Ordinal);

        // Все типы по имени.
        private static readonly Dictionary<string, Type> _types =
            new Dictionary<string, Type>(StringComparer.Ordinal);

        private static readonly HashSet<string> _namespaces =
            new HashSet<string>(StringComparer.Ordinal);

        private static XmlDocProvider _docProvider;

        public static bool IsInitialized { get; private set; }

        public static void Initialize(XmlDocProvider docProvider, params Assembly[] assemblies)
        {
            lock (_lock)
            {
                if (IsInitialized)
                    return;

                IsInitialized = true;
                _docProvider = docProvider;

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
            _types[t.Name] = t;

            if (!string.IsNullOrEmpty(t.Namespace))
            {
                string ns = t.Namespace;
                _namespaces.Add(ns);
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

            var members = new List<CompletionItem>();

            MethodInfo[] methods = t.GetMethods(BindingFlags.Public
                | BindingFlags.Static | BindingFlags.Instance
                | BindingFlags.DeclaredOnly);
            foreach (var m in methods)
            {
                if (m.IsSpecialName)
                    continue;
                if (ContainsName(members, m.Name))
                    continue;
                members.Add(BuildMethodItem(m));
            }

            PropertyInfo[] props = t.GetProperties(BindingFlags.Public
                | BindingFlags.Static | BindingFlags.Instance
                | BindingFlags.DeclaredOnly);
            foreach (var p in props)
            {
                if (ContainsName(members, p.Name))
                    continue;
                members.Add(BuildPropertyItem(p));
            }

            FieldInfo[] fields = t.GetFields(BindingFlags.Public
                | BindingFlags.Static | BindingFlags.Instance
                | BindingFlags.DeclaredOnly);
            foreach (var f in fields)
            {
                if (ContainsName(members, f.Name))
                    continue;
                members.Add(BuildFieldItem(f));
            }

            members.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            _byType[t.Name] = members;
        }

        private static bool ContainsName(List<CompletionItem> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Name == name)
                    return true;
            return false;
        }

        private static CompletionItem BuildMethodItem(MethodInfo m)
        {
            string signature = BuildMethodSignature(m);
            string doc = _docProvider?.GetSummary(m);

            return new CompletionItem
            {
                Name = m.Name,
                Kind = CompletionItemKind.Method,
                Signature = signature,
                ReturnType = FriendlyTypeName(m.ReturnType),
                Documentation = doc
            };
        }

        private static CompletionItem BuildPropertyItem(PropertyInfo p)
        {
            string typeName = FriendlyTypeName(p.PropertyType);

            return new CompletionItem
            {
                Name = p.Name,
                Kind = CompletionItemKind.Property,
                Signature = typeName + " " + p.Name + " { get; }",
                ReturnType = typeName,
                Documentation = _docProvider?.GetSummary(p)
            };
        }

        private static CompletionItem BuildFieldItem(FieldInfo f)
        {
            string typeName = FriendlyTypeName(f.FieldType);

            return new CompletionItem
            {
                Name = f.Name,
                Kind = CompletionItemKind.Field,
                Signature = typeName + " " + f.Name,
                ReturnType = typeName,
                Documentation = _docProvider?.GetSummary(f)
            };
        }

        private static string BuildMethodSignature(MethodInfo m)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(FriendlyTypeName(m.ReturnType));
            sb.Append(' ');
            sb.Append(m.Name);
            sb.Append('(');

            ParameterInfo[] ps = m.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(FriendlyTypeName(ps[i].ParameterType));
                sb.Append(' ');
                sb.Append(ps[i].Name);
            }

            sb.Append(')');
            return sb.ToString();
        }

        private static string FriendlyTypeName(Type t)
        {
            if (t == null)
                return "?";

            // Примитивы — по ключевым словам C#.
            if (t == typeof(void)) return "void";
            if (t == typeof(int)) return "int";
            if (t == typeof(long)) return "long";
            if (t == typeof(short)) return "short";
            if (t == typeof(byte)) return "byte";
            if (t == typeof(sbyte)) return "sbyte";
            if (t == typeof(uint)) return "uint";
            if (t == typeof(ulong)) return "ulong";
            if (t == typeof(ushort)) return "ushort";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(string)) return "string";
            if (t == typeof(object)) return "object";
            if (t == typeof(double)) return "double";
            if (t == typeof(float)) return "float";
            if (t == typeof(decimal)) return "decimal";
            if (t == typeof(char)) return "char";

            // Generic — рекурсивно.
            if (t.IsGenericType)
            {
                string baseName = t.Name;
                int tick = baseName.IndexOf('`');
                if (tick >= 0)
                    baseName = baseName.Substring(0, tick);

                var args = t.GetGenericArguments();
                var sb = new System.Text.StringBuilder();
                sb.Append(baseName);
                sb.Append('<');
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(FriendlyTypeName(args[i]));
                }
                sb.Append('>');
                return sb.ToString();
            }

            if (t.IsArray)
                return FriendlyTypeName(t.GetElementType()) + "[]";

            return t.Name;
        }

        public static List<CompletionItem> GetMembers(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            lock (_lock)
            {
                if (_byType.TryGetValue(typeName, out List<CompletionItem> members))
                    return new List<CompletionItem>(members);

                Type t = Type.GetType(typeName);
                if (t != null)
                {
                    RegisterTypeInternal(t);
                    if (_byType.TryGetValue(typeName, out members))
                        return new List<CompletionItem>(members);
                }
                return null;
            }
        }

        public static bool KnowsType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;
            lock (_lock)
                return _types.ContainsKey(typeName);
        }

        public static bool KnowsTypeOrNamespace(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            lock (_lock)
                return _types.ContainsKey(name) || _namespaces.Contains(name);
        }

        public static Type FindType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            lock (_lock)
                return _types.TryGetValue(name, out Type t) ? t : null;
        }

        public static bool IsInterface(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return false;
            lock (_lock)
                return _types.TryGetValue(typeName, out Type t) && t.IsInterface;
        }
    }
}