using DisplayNodes.Core.Rendering;
using System.Drawing;
using System.IO;

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-реализация <see cref="IImageFactory"/>. Создаёт изображение.
    /// </summary>
    public sealed class GdiImageFactory : IImageFactory
    {
        /// <inheritdoc/>
        public IImage CreateFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (Bitmap safetyCopy = new Bitmap(ms))
                {
                    return new GdiImage(new Bitmap(safetyCopy));
                }
            }
        }

        /// <inheritdoc/>
        public IImage CreateFromFile(string path)
        {
            return new GdiImage(new Bitmap(path));
        }

        /// <inheritdoc/>
        public IImage CreateFromStream(Stream stream)
        {
            using (Bitmap safetyCopy = new Bitmap(stream))
            {
                return new GdiImage(new Bitmap(safetyCopy));
            }
        }
    }
}
