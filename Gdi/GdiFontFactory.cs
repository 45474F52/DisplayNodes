using DisplayNodes.Core.Rendering;
using System.Drawing;

namespace DisplayNodes.Gdi
{
    /// <summary>
    /// GDI-реализация <see cref="IFontFactory"/>. Создаёт шрифт из необходимых аргументов.
    /// </summary>
    public sealed class GdiFontFactory : IFontFactory
    {
        /// <inheritdoc/>
        public IFont Create(string family, float size, bool bold = false, bool italic = false)
        {
            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            return new GdiFont(new Font(family, size, style, GraphicsUnit.Point));
        }
    }
}
