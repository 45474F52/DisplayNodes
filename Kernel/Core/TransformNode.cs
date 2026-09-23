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

namespace DisplayNodes.Core
{
    /// <summary>
    /// Контейнер, применяющий аффинное преобразование к единственному дочернему элементу.
    /// </summary>
    /// <remarks>
    /// <para>Трансформация <b>учитывается в layout</b>: <see cref="LayoutNode.DesiredSize"/>
    /// равен bounding box трансформированного дочернего элемента, а не его исходному размеру.
    /// Это гарантирует, что родительские контейнеры корректно резервируют место.</para>
    /// <para><b>Ограничение.</b> В текущей версии адаптеры не поддерживают
    /// трансформации — при попытке применить узел бросается <see cref="System.NotSupportedException"/>.
    /// API подготовлен для будущей реализации.</para>
    /// </remarks>
    public class TransformNode : LayoutNode
    {
        /// <summary>
        /// Описание трансформации
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Единственный дочерний элемент. Может быть <c>null</c>
        /// </summary>
        public LayoutNode Child
        {
            get => Children.Count > 0 ? Children[0] : null;
            set
            {
                Children.Clear();
                if (value != null)
                    Children.Add(value);
            }
        }

        /// <summary>
        /// Создаёт узел трансформации с заданным описанием
        /// </summary>
        /// <param name="transform">Описание трансформации</param>
        public TransformNode(Transform transform)
        {
            Transform = transform;
        }

        /// <summary>
        /// Создаёт узел с идентичной трансформацией
        /// </summary>
        public TransformNode() : this(Transform.Identity) { }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size available)
        {
            if (Child == null)
                return new Size(Padding.Horizontal, Padding.Vertical);

            Size inner = available.Deflate(Padding);
            Size childSize = Child.Measure(inner);

            BoundingBox bbox = ComputeBoundingBox(childSize, Transform);

            return new Size(
                (int)Math.Ceiling(bbox.Width) + Padding.Horizontal,
                (int)Math.Ceiling(bbox.Height) + Padding.Vertical);
        }

        /// <summary>
        /// Контейнеры всегда занимают весь предоставленный слот (с учётом Margin)
        /// </summary>
        public override void Arrange(Rect finalRect)
        {
            Rect inner = finalRect.Deflate(Margin);
            Bounds = inner;
            ArrangeOverride(inner);
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect)
        {
            if (Child == null)
                return;

            Rect slot = finalRect.Deflate(Padding);

            // Слот — bounding box. Дочерний элемент в нём позиционируется так,
            // чтобы центр трансформации (origin) совпал с целевой точкой.

            // 1. Размер дочернего элемента (до трансформации).
            Size childSize = Child.DesiredSize;

            // 2. bounding box (после трансформации).
            BoundingBox bbox = ComputeBoundingBox(childSize, Transform);

            // Левый верхний угол untransformed прямоугольника в slot:
            //   dx = -originPx.X - bbox.MinX
            //   dy = -originPx.Y - bbox.MinY
            // Это гарантирует, что трансформированный контент заполнит bounding box
            // от (slot.X, slot.Y) до (slot.X + bbox.Width, slot.Y + bbox.Height).

            float ox = childSize.Width * Transform.Origin.X / 100f;
            float oy = childSize.Height * Transform.Origin.Y / 100f;

            int dx = (int)Math.Round(-ox - bbox.MinX);
            int dy = (int)Math.Round(-oy - bbox.MinY);

            // Дочерний элемент получает прямоугольник внутри slot с учётом сдвига.
            Rect childRect = new Rect(
                slot.Point.X + dx,
                slot.Point.Y + dy,
                childSize.Width,
                childSize.Height);

            Child.Arrange(childRect);
        }

        /// <summary>
		/// Вычисляет bounding box трансформированного прямоугольника.
		/// </summary>
		/// <remarks>
		/// Порядок применения: scale → skew → rotate. Origin компенсируется
		/// через перенос углов относительно origin перед трансформацией.
		/// </remarks>
		private static BoundingBox ComputeBoundingBox(Size size, Transform t)
        {
            float w = size.Width;
            float h = size.Height;

            float ox = w * t.Origin.X / 100f;
            float oy = h * t.Origin.Y / 100f;

            // Углы относительно origin.
            float[] xs = { -ox, w - ox, w - ox, -ox };
            float[] ys = { -oy, -oy, h - oy, h - oy };

            // Подготовка коэффициентов.
            float rad = t.Rotation * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float tanSkewX = (float)Math.Tan(t.SkewX * Math.PI / 180f);
            float tanSkewY = (float)Math.Tan(t.SkewY * Math.PI / 180f);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                // 1. Scale.
                float x = xs[i] * t.ScaleX;
                float y = ys[i] * t.ScaleY;

                // 2. Skew.
                float xSkew = x + y * tanSkewX;
                float ySkew = x * tanSkewY + y;

                // 3. Rotate.
                float xRot = xSkew * cos - ySkew * sin;
                float yRot = xSkew * sin + ySkew * cos;

                if (xRot < minX) minX = xRot;
                if (xRot > maxX) maxX = xRot;
                if (yRot < minY) minY = yRot;
                if (yRot > maxY) maxY = yRot;
            }

            // Нейтрализуем микроскопические шумы float-математики
            // (например, cos(π/2) ≈ -4.37e-8 вместо 0).
            return new BoundingBox(
                RoundNearInteger(minX),
                RoundNearInteger(minY),
                RoundNearInteger(maxX),
                RoundNearInteger(maxY));
        }

        /// <summary>
        /// Округляет значение к ближайшему целому, если оно близко к нему
        /// (в пределах <paramref name="epsilon"/>). Иначе возвращает без изменения.
        /// </summary>
        /// <remarks>
        /// Нейтрализует шумы float-математики: например, <c>cos(π/2)</c> даёт
        /// <c>-4.37e-8</c> вместо <c>0</c>, что приводит к погрешности размеров
        /// bounding box в микро-долях пикселя.
        /// </remarks>
        private static float RoundNearInteger(float value, float epsilon = 1e-4f)
        {
            float rounded = (float)Math.Round(value);
            if (Math.Abs(value - rounded) < epsilon)
                return rounded;
            return value;
        }
    }
}
