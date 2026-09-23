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

using DisplayNodes.Fluent;
using DisplayNodes.Gdi;

namespace DisplayNodes.WinFormsAdapter
{
    /// <summary>
    /// Точка входа для инициализации DisplayNodes с бэкендом WinForms.
    /// </summary>
    public static class Adapter
    {
        /// <summary>
        /// Инициализирует UI-фабрики WinForms-реализациями.
        /// Должен быть вызван один раз перед использованием UI.*
        /// </summary>
        public static void Initialize()
        {
            UI.Factory = new WidgetFactory();
            UI.Measurer = new GdiTextMeasurer();
            UI.BrushFactory = new GdiBrushFactory();
            UI.FontFactory = new GdiFontFactory();
            UI.ImageFactory = new GdiImageFactory();
        }
    }
}
