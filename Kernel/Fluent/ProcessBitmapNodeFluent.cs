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

using DisplayNodes.Core.Rendering;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
    /// <summary>
    /// Fluent-расширения для настройки эффектов обработки изображений.<br/>
    /// Применяются к любым виджетам, чей компонент реализует <see cref="IEffectComponent"/>.
    /// </summary>
    public static class ProcessBitmapNodeFluent
    {
        /// <summary>Установить прозрачность (0–100).</summary>
        public static T Opacity<T>(this T node, double opacity) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Opacity = opacity;
            return node;
        }

        /// <summary>Установить яркость (-100…100).</summary>
        public static T Brightness<T>(this T node, double brightness) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Brightness = brightness;
            return node;
        }

        /// <summary>Установить контраст (-100…100).</summary>
        public static T Contrast<T>(this T node, double contrast) where T : WidgetNode
        {
            if (node.Component is IEffectComponent p) p.Contrast = contrast;
            return node;
        }
    }
}