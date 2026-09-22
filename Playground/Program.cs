using System;
using System.Windows.Forms;

using DisplayNodes.LibDisplayDrawingAdapter;

namespace DisplayNodes.Playground
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Adapter.Initialize();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new AppForm());
		}
	}
}
