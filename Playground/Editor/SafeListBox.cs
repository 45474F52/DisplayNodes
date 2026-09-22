using System.Windows.Forms;

namespace DisplayNodes.Playground.Editor
{
	internal class SafeListBox : ListBox
	{
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_LBUTTONDBLCLK = 0x0203;

		protected override void WndProc(ref Message m)
		{
			// Подавляем клики по пустому списку — именно они вызывают
			// ArgumentException в accessibility-коде WinForms.
			if (Items.Count == 0 &&
				(m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK))
			{
				return;
			}

			base.WndProc(ref m);
		}
	}
}
