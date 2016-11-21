using System;
using System.Diagnostics;
using System.Windows.Forms;
using QMK.Properties;
using System.Drawing;

namespace QMK
{
	class ContextMenus
	{
		public ContextMenuStrip Create()
		{
			ContextMenuStrip menu = new ContextMenuStrip();
			ToolStripMenuItem item;
			ToolStripSeparator sep;

			item = new ToolStripMenuItem();
			item.Text = "Open Helper";
			item.Click += new EventHandler(Open_Click);
			item.Image = Resources.psi;
			menu.Items.Add(item);

			sep = new ToolStripSeparator();
			menu.Items.Add(sep);

			item = new ToolStripMenuItem();
			item.Text = "Exit";
			item.Click += new System.EventHandler(Exit_Click);
			item.Image = Resources.Exit;
			menu.Items.Add(item);

			return menu;
		}

        void Open_Click(object sender, EventArgs e) {
            if (Program.optionsWindow == null)
                Program.optionsWindow = new Options();
            if (!Program.optionsWindow.Visible)
                Program.optionsWindow.ShowDialog();
        }

		void Exit_Click(object sender, EventArgs e)
		{
			// Quit without further ado.
			Application.Exit();
		}
	}
}