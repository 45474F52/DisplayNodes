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
using System.Drawing;
using System.Windows.Forms;

using DisplayNodes.Core;
using DisplayNodes.Fluent;
using DisplayNodes.Gdi;

using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Реализация <see cref="IPreviewHost"/> для <c>WinFormsAdapter</c>.
    /// Рендерит дерево как набор WinForms-контролов внутри переданной панели.
    /// </summary>
    internal sealed class WinFormsPreviewHost : IPreviewHost
    {
        private readonly Panel _panel;
        private readonly DisplayRoot _displayRoot;
        private LayoutNode _currentRoot;

        private bool _disposed;

        /// <summary>
        /// Создаёт хост, использующий переданную панель как область предпросмотра.
        /// </summary>
        /// <param name="panel">
        /// Панель для отображения. Хост не становится её владельцем —
        /// вызывающий код управляет жизненным циклом панели.
        /// </param>
        /// <exception cref="ArgumentNullException">Если <paramref name="panel"/> равен <c>null</c>.</exception>
        public WinFormsPreviewHost(Panel panel)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));

            _displayRoot = new DisplayRoot(
                new WinFormsAdapter.RenderRootFactory(_panel));
        }

        /// <inheritdoc/>
        public Size CurrentSize
        {
            get
            {
                Size clientSize = _panel.ClientSize.FromGdi();
                return new Size(
                    Math.Max(1, clientSize.Width),
                    Math.Max(1, clientSize.Height));
            }
        }

        /// <inheritdoc/>
        public void Build(LayoutNode root)
        {
            _currentRoot = root ?? throw new ArgumentNullException(nameof(root));
            Size size = CurrentSize;
            _displayRoot.Build(root, Point.Empty, size);
        }

        /// <inheritdoc/>
        public void Resize(Size size)
        {
            if (_currentRoot == null)
                return;

            if (_displayRoot.Root != null)
                _displayRoot.Root.Size = size;

            _currentRoot.Measure(size);
            _currentRoot.Arrange(new Rect(Point.Empty, size));

            _displayRoot.Root?.Refresh();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _displayRoot.Clear();
            _currentRoot = null;
        }

        /// <inheritdoc/>
        public Bitmap ExportToBitmap()
        {
            if (_panel.Width <= 0 || _panel.Height <= 0)
                return null;

            var bmp = new Bitmap(_panel.Width, _panel.Height);
            try
            {
                _panel.DrawToBitmap(bmp, new Rectangle(0, 0, _panel.Width, _panel.Height));
                return bmp;
            }
            catch
            {
                bmp.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _displayRoot?.Dispose();
        }
    }
}