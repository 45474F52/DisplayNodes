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
using System.Diagnostics;

namespace DisplayNodes.Core
{
    /// <summary>
    /// Описание аффинного преобразования компонента.
    /// </summary>
    /// <remarks>
	/// <para>Трансформация применяется <b>при отрисовке</b> и <b>учитывается в layout</b>:
	/// при использовании внутри <see cref="TransformNode"/> размер bounding box
	/// трансформированного элемента вычисляется в <see cref="TransformNode.MeasureOverride"/>.</para>
	/// <para><b>Порядок применения:</b> scale → skew → rotate, относительно точки
	/// <see cref="Origin"/>. Перенос в origin и обратно компенсируется матрицей.</para>
    /// <b>Ограничение.</b> В текущей версии адаптеры не поддерживают
    /// трансформации — при попытке её установить бросается <see cref="NotSupportedException"/>.
    /// API подготовлен для будущей реализации.
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("Transform(scale=({ScaleX}, {ScaleY}), rotation={Rotation}°, origin={Origin})")]
    public readonly struct Transform
    {
        /// <summary>Масштаб по X. <c>1.0</c> — без изменения.</summary>
        public readonly float ScaleX;

        /// <summary>Масштаб по Y. <c>1.0</c> — без изменения.</summary>
        public readonly float ScaleY;

        /// <summary>Поворот в градусах (по часовой стрелке). <c>0</c> — без поворота.</summary>
        public readonly float Rotation;

        /// <summary>Скос по X (в градусах). <c>0</c> — без скоса.</summary>
        public readonly float SkewX;

        /// <summary>Скос по Y (в градусах). <c>0</c> — без скоса.</summary>
        public readonly float SkewY;

        /// <summary>
        /// Точка, относительно которой применяется трансформация, в нормализованных
        /// координатах (0..100). <c>Point(50, 50)</c> — центр, <c>Point(0, 0)</c> — левый верхний угол.
        /// </summary>
        public readonly Point Origin;

        /// <summary>
        /// Создаёт описание трансформации.
        /// </summary>
        /// <param name="scaleX">Масштаб по X (по умолчанию 1).</param>
        /// <param name="scaleY">Масштаб по Y (по умолчанию 1).</param>
        /// <param name="rotation">Поворот в градусах (по умолчанию 0).</param>
        /// <param name="origin">
        /// Точка трансформации в нормализованных координатах (0..100).
        /// Если не задана — центр (50, 50).
        /// </param>
        public Transform(float scaleX, float scaleY, float rotation, Point origin)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            Rotation = rotation;
            SkewX = 0f;
            SkewY = 0f;
            Origin = origin;
        }

        /// <summary>
        /// Создаёт описание трансформации со скосом.
        /// </summary>
        /// <param name="scaleX">Масштаб по X (по умолчанию 1).</param>
        /// <param name="scaleY">Масштаб по Y (по умолчанию 1).</param>
        /// <param name="rotation">Поворот в градусах (по умолчанию 0).</param>
        /// <param name="skewX">Скос по X</param>
        /// <param name="skewY">Скос по Y</param>
        /// <param name="origin">
        /// Точка трансформации в нормализованных координатах (0..100).
        /// Если не задана — центр (50, 50).
        /// </param>
        public Transform(float scaleX, float scaleY, float rotation, float skewX, float skewY, Point origin)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            Rotation = rotation;
            SkewX = skewX;
            SkewY = skewY;
            Origin = origin;
        }

        /// <summary>
        /// Трансформация по умолчанию — без изменений (identity).
        /// </summary>
        public static Transform Identity => new Transform(1f, 1f, 0f, new Point(50, 50));

        /// <inheritdoc/>
        public override string ToString()
            => $"Transform(scale=({ScaleX}, {ScaleY}), rotation={Rotation}°, origin={Origin})";
    }
}
