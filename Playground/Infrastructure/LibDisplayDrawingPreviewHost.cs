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

using DisplayNodes.Core;
using DisplayNodes.Fluent;
using DisplayNodes.Gdi;
using DisplayNodes.LibDisplayDrawingAdapter;
using LibDisplayDrawing;
using System;
using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Playground.Infrastructure
{
    /// <summary>
    /// Реализация <see cref="IPreviewHost"/> для <c>LibDisplayDrawingAdapter</c>.
    /// Рендерит дерево в Bitmap и отображает его на панели.
    /// </summary>
    /// <remarks>
    /// <para>LibDisplayDrawing возвращает отрисованный Bitmap через событие
    /// <c>ManagerDisplays.onPaint</c>. Этот Bitmap отображается на панели
    /// через <see cref="PictureBox"/>.</para>
    /// <para>Хост владеет внутренним <see cref="Timer"/> для обновления
    /// <c>ManagerTimers.CurrentTime</c> — иначе таймеры не работают.</para>
    /// </remarks>
    internal sealed class LibDisplayPreviewHost : IPreviewHost
    {
        private readonly Panel _panel;
        private readonly PictureBox _picture;
        private readonly ManagerTimers _timers;
        private readonly ManagerDisplays _displays;
        private readonly DisplayRoot _displayRoot;

        private readonly Timer _updateTimer;
        private DateTime _lastTime;

        private Bitmap _lastBitmap;
        private bool _disposed;

        private const string DisplayId = "preview";

        public LibDisplayPreviewHost(Panel panel, int updateIntervalMs = 16)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));

            // PictureBox внутри панели для отображения Bitmap.
            _picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            _panel.Controls.Add(_picture);

            _timers = new ManagerTimers();
            _displays = new ManagerDisplays(_timers, new[] { DisplayId });
            _displays.onPaint += OnDisplayPaint;

            var preview = _displays[DisplayId];
            Size size = CurrentSize;
            preview.Size = new Size2D(size.Width, size.Height);

            _displayRoot = new DisplayRoot(
                new RenderRootFactory(preview, _timers));

            _updateTimer = new Timer { Interval = updateIntervalMs };
            _updateTimer.Tick += OnUpdateTimerTick;
            _updateTimer.Start();
            _lastTime = DateTime.Now;
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
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            Size size = CurrentSize;
            _displays[DisplayId].Size = new Size2D(size.Width, size.Height);
            _displayRoot.Build(root, Point.Empty, size);
            _displays[DisplayId].Refresh();
        }

        /// <inheritdoc/>
        public void Resize(Size size)
        {
            if (_displayRoot.Root == null)
                return;

            _displays[DisplayId].Size = new Size2D(size.Width, size.Height);
            _displayRoot.Root.Size = size;
            _displays[DisplayId].Refresh();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _displayRoot.Clear();
            _lastBitmap?.Dispose();
            _lastBitmap = null;
            _picture.Image = null;
        }

        /// <inheritdoc/>
        public Bitmap ExportToBitmap()
        {
            if (_lastBitmap == null)
                return null;
            return new Bitmap(_lastBitmap);
        }

        private void OnDisplayPaint(string id, Bitmap bitmap)
        {
            if (_disposed || bitmap == null)
                return;

            // Событие может прийти из другого потока.
            if (_panel.IsHandleCreated && _panel.InvokeRequired)
            {
                try
                {
                    _panel.BeginInvoke(new Action<string, Bitmap>(OnDisplayPaint), id, bitmap);
                }
                catch (InvalidOperationException)
                {
                    // Handle destroyed — игнорируем.
                }
                return;
            }

            // Клонируем, чтобы не зависеть от того, кто владеет bitmap.
            _lastBitmap?.Dispose();
            _lastBitmap = new Bitmap(bitmap);

            _picture.Image?.Dispose();
            _picture.Image = new Bitmap(_lastBitmap);
        }

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            float delta = (float)(now - _lastTime).TotalSeconds;
            _lastTime = now;
            _timers.CurrentTime = delta;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            _displays.onPaint -= OnDisplayPaint;

            _displayRoot?.Dispose();
            _lastBitmap?.Dispose();
            _lastBitmap = null;

            _picture?.Dispose();
        }
    }
}