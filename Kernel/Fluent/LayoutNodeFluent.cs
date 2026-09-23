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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Helpers;
using DisplayNodes.Widgets;

namespace DisplayNodes.Fluent
{
	/// <summary>
	/// Fluent-расширения для настройки и применения деревьев узлов <see cref="LayoutNode"/>.
	/// Управляет жизненным циклом созданных <see cref="IRenderComponent"/> (Apply, Rebuild, Dispose).
	/// </summary>
	public static class LayoutNodeFluent
	{
		/// <summary>Устанавливает минимальную ширину узла.</summary>
		public static T MinWidth<T>(this T node, int minWidth) where T : LayoutNode
		{ node.MinWidth = minWidth; return node; }

		/// <summary>Устанавливает максимальную ширину узла.</summary>
		public static T MaxWidth<T>(this T node, int maxWidth) where T : LayoutNode
		{ node.MaxWidth = maxWidth; return node; }

		/// <summary>Устанавливает минимальную высоту узла.</summary>
		public static T MinHeight<T>(this T node, int minHeight) where T : LayoutNode
		{ node.MinHeight = minHeight; return node; }

		/// <summary>Устанавливает максимальную высоту узла.</summary>
		public static T MaxHeight<T>(this T node, int maxHeight) where T : LayoutNode
		{ node.MaxHeight = maxHeight; return node; }

		/// <summary>Устанавливает и минимальную, и максимальную ширину узла.</summary>
		public static T WidthRange<T>(this T node, int minWidth, int maxWidth) where T : LayoutNode
		{ node.MinWidth = minWidth; node.MaxWidth = maxWidth; return node; }

		/// <summary>Устанавливает и минимальную, и максимальную высоту узла.</summary>
		public static T HeightRange<T>(this T node, int minHeight, int maxHeight) where T : LayoutNode
		{ node.MinHeight = minHeight; node.MaxHeight = maxHeight; return node; }

		/// <summary>Устанавливает внешние отступы (Margin).</summary>
		public static T Margin<T>(this T node, Thickness m) where T : LayoutNode
		{ node.Margin = m; return node; }

		/// <summary>Устанавливает равномерные внешние отступы.</summary>
		public static T Margin<T>(this T node, int all) where T : LayoutNode
		{ node.Margin = new Thickness(all); return node; }

		/// <summary>Устанавливает внешние отступы (горизонтальные, вертикальные).</summary>
		public static T Margin<T>(this T node, int h, int v) where T : LayoutNode
		{ node.Margin = new Thickness(h, v); return node; }

		/// <summary>Устанавливает внутренние отступы (Padding).</summary>
		public static T Padding<T>(this T node, Thickness p) where T : LayoutNode
		{ node.Padding = p; return node; }

		/// <summary>Устанавливает равномерные внутренние отступы.</summary>
		public static T Padding<T>(this T node, int all) where T : LayoutNode
		{ node.Padding = new Thickness(all); return node; }

		/// <summary>Устанавливает внутренние отступы (горизонтальные, вертикальные).</summary>
		public static T Padding<T>(this T node, int h, int v) where T : LayoutNode
		{ node.Padding = new Thickness(h, v); return node; }

		/// <summary>Устанавливает горизонтальное выравнивание внутри слота.</summary>
		public static T HAlignment<T>(this T node, Alignment a) where T : LayoutNode
		{ node.HAlignment = a; return node; }

		/// <summary>Устанавливает вертикальное выравнивание внутри слота.</summary>
		public static T VAlignment<T>(this T node, Alignment a) where T : LayoutNode
		{ node.VAlignment = a; return node; }

		/// <summary>
		/// Устанавливает вес flex вдоль главной оси родителя (для <see cref="StackLayoutNode"/>).
		/// </summary>
		public static T Flex<T>(this T node, double weight = 1d) where T : LayoutNode
		{ node.FlexWeight = weight; return node; }

		/// <summary>Добавляет дочерний узел в коллекцию Children.</summary>
		public static T Add<T>(this T node, LayoutNode child) where T : LayoutNode
		{ node.Children.Add(child); return node; }

		/// <summary>Устанавливает распределение пространства вдоль главной оси (для StackLayoutNode).</summary>
		public static StackLayoutNode MainAlignment(this StackLayoutNode node, MainAxisAlignment a)
		{ node.MainAxisAlignment = a; return node; }

        /// <summary>
        /// Устанавливает тень узла.
        /// </summary>
        /// <remarks>
        /// Тень применяется к <see cref="IRenderComponent"/> узла. Поддерживается
        /// только виджетами (<see cref="WidgetNode"/>), у которых компонент реализует
        /// <see cref="IEffectComponent"/>. В текущей версии адаптеры не поддерживают тень —
        /// при вызове будет брошено <see cref="NotSupportedException"/>.
        /// </remarks>
        public static T Shadow<T>(this T node, Shadow shadow) where T : LayoutNode
        {
            if (node is WidgetNode widget && widget.Component is IEffectComponent effect)
                effect.Shadow = shadow;
            return node;
        }

        /// <summary>
        /// Устанавливает тень с явными параметрами.
        /// </summary>
        public static T Shadow<T>(this T node, int offsetX, int offsetY, int blurRadius, Color color) where T : LayoutNode
			=> Shadow(node, new Shadow(offsetX, offsetY, blurRadius, color));

        /// <summary>
        /// Устанавливает тень со стандартным цветом (полупрозрачный чёрный).
        /// </summary>
        public static T Shadow<T>(this T node, int offsetX, int offsetY, int blurRadius) where T : LayoutNode
			=> Shadow(node, new Shadow(offsetX, offsetY, blurRadius));

        /// <summary>
        /// Выполняет Measure и Arrange для дерева узлов и рекурсивно привязывает
        /// созданные <see cref="IRenderComponent"/> к указанному <paramref name="parent"/>.
        /// </summary>
        public static void Apply(this LayoutNode root, IRenderComponent parent, Point location, Size size)
		{
			_ = root.Measure(size);
			root.Arrange(new Rect(location, size));
			var components = new List<IRenderComponent>();
			ApplyRecursive(root, parent, components);
			root.AppliedComponents = components;
		}

		private static void ApplyRecursive(LayoutNode root, IRenderComponent parent, ICollection<IRenderComponent> components)
		{
			if (root is WidgetNode widget)
			{
				widget.Component.Parent = parent;
				components.Add(widget.Component);
				return;
			}

			if (root is ClipNode clip)
			{
				clip.Mask.Parent = parent;
				components.Add(clip.Mask);
				foreach (LayoutNode child in root.Children)
					ApplyRecursive(child, clip.Mask, components);
				return;
            }

            if (root is TransformNode)
            {
                throw new NotSupportedException(
                    "TransformNode is not supported by current adapters yet. " +
                    "The node and its children were laid out, but rendering cannot be performed.");
            }

            foreach (LayoutNode child in root.Children)
				ApplyRecursive(child, parent, components);
		}

		/// <summary>
		/// Удаляет старые компоненты и строит новое дерево поверх того же root-узла.
		/// </summary>
		public static void Rebuild(this LayoutNode root, IRenderComponent parent, Point location, Size size)
		{
			if (root.AppliedComponents != null)
			{
				foreach (var c in root.AppliedComponents)
				{
					c.Parent = null;
					(c as IDisposable)?.Dispose();
				}
				root.AppliedComponents.Clear();
			}
			root.Apply(parent, location, size);
		}

		/// <summary>
		/// Полностью удаляет UI-дерево, освобождает все связанные <see cref="IRenderComponent"/>
		/// и очищает внутреннее хранилище ссылок.
		/// </summary>
		public static void Dispose(this LayoutNode root)
		{
			if (root.AppliedComponents != null)
			{
				foreach (var c in root.AppliedComponents)
				{
					c.Parent = null;
					(c as IDisposable)?.Dispose();
				}
				root.AppliedComponents.Clear();
				root.AppliedComponents = null;
			}
			root.DisposeTree();
		}
	}
}