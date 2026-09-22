using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	internal class BufferedPanel : Panel
	{
		public BufferedPanel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint, true);
			UpdateStyles();
		}
	}
}
