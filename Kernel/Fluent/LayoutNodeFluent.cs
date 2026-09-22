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

		/// <summary>Добавляет дочерний узел в коллекцию Children.</summary>
		public static T Add<T>(this T node, LayoutNode child) where T : LayoutNode
		{ node.Children.Add(child); return node; }

		/// <summary>Устанавливает распределение пространства вдоль главной оси (для StackLayoutNode).</summary>
		public static StackLayoutNode MainAlignment(this StackLayoutNode node, MainAxisAlignment a)
		{ node.MainAxisAlignment = a; return node; }

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