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
    /// Контейнер, отображающий одно из двух поддеревьев в зависимости
    /// от значения <see cref="Observable{T}"/> типа <see cref="bool"/>.
    /// </summary>
    /// <remarks>
    /// <para>Оба поддерева (<see cref="TrueNode"/> и <see cref="FalseNode"/>) хранятся
    /// в <see cref="LayoutNode.Children"/> всегда. Неактивное поддерево пропускается
    /// в <see cref="MeasureOverride"/> и <see cref="ArrangeOverride"/> — оно не занимает
    /// место в layout и не рендерится.</para>
    /// <para><b>Ограничение.</b> Компоненты рендерера для обоих поддеревьев создаются
    /// один раз при первом <c>Apply</c> и живут до <c>Dispose</c>. Переключение
    /// <see cref="Condition"/> не пересоздаёт компоненты — только меняет активное
    /// поддерево. Чтобы изменения отразились на экране, нужно запросить
    /// перерисовку у <see cref="Rendering.IRenderRoot"/> (например, через
    /// <see cref="Fluent.DisplayRoot.Build"/> или <c>IRenderRoot.Root.Refresh()</c>).</para>
    /// <para>Аналог <c>v-if</c> в Vue, <c>{#if}</c> в Svelte, <c>Conditional</c> в SwiftUI.</para>
    /// </remarks>
    public class ConditionalNode : LayoutNode, IDisposable
    {
        private readonly Observable<bool> _condition;
        private IDisposable _subscription;

        private bool _disposed;

        /// <summary>
        /// Реактивный источник условия.
        /// </summary>
        public Observable<bool> Condition => _condition;

        /// <summary>
        /// Узел, отображаемый при <see cref="Condition"/> == <c>true</c>. Может быть <c>null</c>.
        /// </summary>
        public LayoutNode TrueNode { get; }

        /// <summary>
        /// Узел, отображаемый при <see cref="Condition"/> == <c>false</c>. Может быть <c>null</c>.
        /// </summary>
        public LayoutNode FalseNode { get; }

        /// <summary>
		/// Событие, вызываемое при переключении условия. Аргумент — новое значение.
		/// Используется <see cref="Fluent.LayoutNodeFluent"/> для инвалидации рендера.
        /// </summary>
        internal event Action<bool> ConditionChanged;

        /// <summary>
        /// Создаёт условный контейнер.
        /// </summary>
        /// <param name="condition">Реактивное условие. Не может быть <c>null</c>.</param>
        /// <param name="trueNode">Узел для <c>true</c>. Может быть <c>null</c> — тогда при <c>true</c> отображается пустота.</param>
        /// <param name="falseNode">Узел для <c>false</c>. Может быть <c>null</c> — тогда при <c>false</c> отображается пустота.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="condition"/> равен <c>null</c>.</exception>
        public ConditionalNode(Observable<bool> condition, LayoutNode trueNode, LayoutNode falseNode)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));

            TrueNode = trueNode;
            FalseNode = falseNode;

            if (TrueNode != null)
                Children.Add(TrueNode);
            if (FalseNode != null)
                Children.Add(FalseNode);

            _subscription = _condition.Subscribe(OnConditionChanged);
        }

        /// <summary>
        /// Активное поддерево в текущий момент. Может быть <c>null</c>.
        /// </summary>
        public LayoutNode ActiveNode => _condition.Value ? TrueNode : FalseNode;

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size available)
        {
            LayoutNode active = ActiveNode;
            if (active == null)
                return new Size(Padding.Horizontal, Padding.Vertical);

            Size inner = available.Deflate(Padding);
            Size childSize = active.Measure(inner);

            return new Size(
                childSize.Width + Padding.Horizontal,
                childSize.Height + Padding.Vertical);
        }

        /// <summary>
        /// Контейнер всегда занимает весь предоставленный слот (с учётом Margin).
        /// Активный ребёнок получает слот целиком и сам решает, растягиваться или нет
        /// (через <see cref="LayoutNode.HAlignment"/> / <see cref="LayoutNode.VAlignment"/>).
        /// </summary>
        public sealed override void Arrange(Rect finalRect)
        {
            Rect inner = finalRect.Deflate(Margin);
            Bounds = inner;
            ArrangeOverride(inner);
        }

        /// <inheritdoc/>
        protected override void ArrangeOverride(Rect finalRect)
        {
            LayoutNode active = ActiveNode;
            if (active == null)
                return;

            Rect slot = finalRect.Deflate(Padding);
            active.Arrange(slot);
        }

        private void OnConditionChanged(bool value)
        {
            if (_disposed)
                return;

            ConditionChanged?.Invoke(value);
        }

        /// <summary>
		/// Отписывается от <see cref="Condition"/> и освобождает подписки.
		/// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
