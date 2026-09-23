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
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Загружает XML-документацию из файлов рядом со сборками
    /// и предоставляет summary по <see cref="MemberInfo"/>.
    /// </summary>
    /// <remarks>
    /// <para>Ищет <c>{AssemblyName}.xml</c> рядом с <c>{AssemblyLocation}</c>.
    /// Если файла нет — просто возвращает пустые строки.</para>
    /// <para>Поддерживаются только summary из своих сборок —
    /// BCL-документация не читается.</para>
    /// </remarks>
    internal sealed class XmlDocProvider
    {
        private readonly Dictionary<string, string> _summaries = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<string> _loadedFiles = new List<string>();

        /// <summary>
        /// Загружает XML-документацию для указанных сборок.
        /// </summary>
        public void LoadFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                return;

            foreach (Assembly asm in assemblies)
            {
                if (asm == null)
                    continue;

                try
                {
                    string location = asm.Location;
                    if (string.IsNullOrEmpty(location))
                        continue;

                    string xmlPath = Path.ChangeExtension(location, ".xml");
                    if (!File.Exists(xmlPath))
                        continue;

                    LoadFile(xmlPath);
                }
                catch (Exception ex)
                {
                    AppLog.Error("XmlDocProvider.LoadFromAssemblies", ex);
                }
            }
        }

        private void LoadFile(string xmlPath)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var members = doc.Root?.Element("members");
                if (members == null)
                    return;

                foreach (XElement member in members.Elements("member"))
                {
                    string name = member.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    XElement summary = member.Element("summary");
                    if (summary == null)
                        continue;

                    string text = ExtractText(summary);
                    if (!string.IsNullOrEmpty(text))
                        _summaries[name] = text;
                }

                _loadedFiles.Add(xmlPath);
            }
            catch (Exception ex)
            {
                AppLog.Error("XmlDocProvider.LoadFile", ex);
            }
        }

        /// <summary>
        /// Возвращает XML-doc для указанного члена, или <c>null</c>.
        /// </summary>
        public string GetSummary(MemberInfo member)
        {
            if (member == null)
                return null;

            string key = BuildMemberKey(member);
            return _summaries.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>
        /// Возвращает список загруженных файлов (для отладки).
        /// </summary>
        public IList<string> LoadedFiles => _loadedFiles.AsReadOnly();

        /// <summary>
        /// Строит ключ в формате XML-документации: <c>M:Namespace.Type.Method(...)</c>.
        /// </summary>
        private static string BuildMemberKey(MemberInfo member)
        {
            try
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        return BuildMethodKey((MethodInfo)member);
                    case MemberTypes.Property:
                        return "P:" + BuildTypeName(member.DeclaringType) + "." + member.Name;
                    case MemberTypes.Field:
                        return "F:" + BuildTypeName(member.DeclaringType) + "." + member.Name;
                    case MemberTypes.Constructor:
                        {
                            var ctor = (ConstructorInfo)member;
                            return "M:" + BuildTypeName(member.DeclaringType) + ".#ctor" + BuildParametersKey(ctor.GetParameters());
                        }
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string BuildMethodKey(MethodInfo method)
        {
            string typeName = BuildTypeName(method.DeclaringType);
            string paramKey = BuildParametersKey(method.GetParameters());
            return "M:" + typeName + "." + method.Name + paramKey;
        }

        private static string BuildParametersKey(ParameterInfo[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.Append('(');
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(BuildParameterTypeName(parameters[i].ParameterType));
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Строит имя типа в формате XML-doc: с обратной косой чертой для generic'ов, без пробелов.
        /// </summary>
        private static string BuildTypeName(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.IsGenericType)
            {
                var baseName = type.FullName ?? type.Name;
                int tick = baseName.IndexOf('`');
                if (tick >= 0)
                    baseName = baseName.Substring(0, tick);

                var args = type.GetGenericArguments();
                var sb = new System.Text.StringBuilder();
                sb.Append(baseName);
                sb.Append('{');
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        sb.Append(',');
                    sb.Append(BuildTypeName(args[i]));
                }
                sb.Append('}');
                return sb.ToString();
            }

            if (type.IsNested && type.DeclaringType != null)
                return BuildTypeName(type.DeclaringType) + "." + type.Name;

            return type.FullName ?? type.Name;
        }

        private static string BuildParameterTypeName(Type type)
        {
            if (type == null)
                return string.Empty;

            // Для параметров — без обратной косой черты для вложенных типов.
            if (type.IsGenericType)
                return BuildTypeName(type);

            if (type.IsNested && type.DeclaringType != null)
                return BuildTypeName(type);

            return type.FullName ?? type.Name;
        }

        private static string ExtractText(XElement element)
        {
            var sb = new System.Text.StringBuilder();

            foreach (XNode node in element.Nodes())
            {
                if (node is XText text)
                    sb.Append(text.Value);
                else if (node is XElement el)
                {
                    if (el.Name == "see" || el.Name == "seealso")
                    {
                        string cref = el.Attribute("cref")?.Value;
                        if (!string.IsNullOrEmpty(cref))
                            sb.Append(StripPrefix(cref));
                    }
                    else if (el.Name == "paramref")
                    {
                        sb.Append(el.Attribute("name")?.Value ?? "");
                    }
                    else if (el.Name == "typeparamref")
                    {
                        sb.Append(el.Attribute("name")?.Value ?? "");
                    }
                    else
                    {
                        sb.Append(el.Value);
                    }
                }
            }

            string result = sb.ToString();
            return NormalizeWhitespace(result);
        }

        private static string StripPrefix(string cref)
        {
            if (string.IsNullOrEmpty(cref))
                return cref;

            int colon = cref.IndexOf(':');
            if (colon >= 0 && colon < 2)
                return cref.Substring(colon + 1);

            return cref;
        }

        private static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Разбиваем по пробелам и пересобираем — так убираются переносы строк,
            // табы, множественные пробелы.
            string[] parts = text.Split(new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }
    }
}