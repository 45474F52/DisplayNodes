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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Единое хранилище настроек приложения: геометрия окна, сплиттеры,
    /// параметры редактора, флаги. Файл — key=value в %APPDATA%.
    /// </summary>
    /// <remarks>
    /// При первом запуске автоматически мигрирует данные из старого
    /// <c>state.txt</c> (переименовывает в <c>settings.txt</c>).
    /// </remarks>
    internal sealed class AppSettings
    {
        private readonly string _storageFile;
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public AppSettings()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DisplayNodesPlayground");

            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                AppLog.Error("AppSettings.ctor", ex);
            }

            _storageFile = Path.Combine(dir, "settings.txt");

            string oldStateFile = Path.Combine(dir, "state.txt");
            if (!File.Exists(_storageFile) && File.Exists(oldStateFile))
            {
                try
                {
                    File.Move(oldStateFile, _storageFile);
                }
                catch (Exception ex)
                {
                    AppLog.Error("AppSettings.Migrate", ex);
                }
            }

            Load();
        }

        public string GetString(string key, string @default = null)
            => _values.TryGetValue(key, out string value) ? value : @default;

        public int GetInt(string key, int @default = default)
        {
            if (_values.TryGetValue(key, out string value))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                {
                    return result;
                }
            }

            return @default;
        }

        public double GetDouble(string key, double @default = default)
        {
            if (_values.TryGetValue(key, out string value))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                {
                    return result;
                }
            }

            return @default;
        }

        public bool GetBool(string key, bool @default = default)
        {
            if (_values.TryGetValue(key, out string value))
            {
                if (bool.TryParse(value, out bool result))
                {
                    return result;
                }
            }

            return @default;
        }

        public void SetString(string key, string value)
        {
            if (value == null)
                _values.Remove(key);
            else
                _values[key] = value;
        }

        public void SetInt(string key, int value)
            => _values[key] = value.ToString(CultureInfo.InvariantCulture);

        public void SetDouble(string key, double value)
            => _values[key] = value.ToString(CultureInfo.InvariantCulture);

        public void SetBool(string key, bool value)
            => _values[key] = value ? "true" : "false";

        public void Remove(string key) => _values.Remove(key);

        public void Save()
        {
            try
            {
                var sb = new StringBuilder();

                foreach (var kv in _values)
                {
                    sb.Append(kv.Key);
                    sb.Append('=');
                    sb.Append(kv.Value);
                    sb.AppendLine();
                }

                File.WriteAllText(_storageFile, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                AppLog.Error("AppSettings.Save", ex);
            }
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_storageFile))
                    return;

                foreach (string line in File.ReadAllLines(_storageFile, Encoding.UTF8))
                {
                    if (line.IsNullOrWhiteSpace())
                        continue;

                    if (line.StartsWith("#"))
                        continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0)
                        continue;

                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1);

                    if (key.Length > 0)
                        _values.Add(key, value);
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("AppSettings.Load", ex);
            }
        }
    }
}
