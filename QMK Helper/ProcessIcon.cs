using System;
using System.Diagnostics;
using System.Windows.Forms;
using QMK.Properties;

namespace QMK
{
	/// <summary>
	/// 
	/// </summary>
	class ProcessIcon : IDisposable
    {
        NotifyIcon ni;

		public ProcessIcon()
		{
			ni = new NotifyIcon();
		}

		public void Display()
		{
			ni.MouseClick += new MouseEventHandler(ni_MouseClick);
			ni.Icon = Resources.QMK;
			ni.Text = "QMK Helper";
			ni.Visible = true;

			ni.ContextMenuStrip = new ContextMenus().Create();
		}

		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			ni.Dispose();
		}

		void ni_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
                if (Program.optionsWindow == null)
                    Program.optionsWindow = new Options();
                if (!Program.optionsWindow.Visible)
                    Program.optionsWindow.ShowDialog();
                Program.optionsWindow.BringToFront();
            }
		}
	}
}