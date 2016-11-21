using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QMK {

    static class Program {
        [DllImport("user32.dll")]
        static extern int GetForegroundWindow(); //Retrieves a handle to the foreground window

        [DllImport("user32.dll")]
        static extern int GetWindowText(int hWnd, StringBuilder text, int count); //Copies the text of the specified window's title bar (if it has one) into a buffer

        [DllImport("user32")]
        static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);

        static public Options optionsWindow;

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show the system tray icon.					
            using (ProcessIcon pi = new ProcessIcon()) {
                Keyboard.FindKeyboards();
                pi.Display();

                //System.Timers.Timer aTimer = new System.Timers.Timer();
                //aTimer.Elapsed += new ElapsedEventHandler(tmrHandle_Tick);
                //aTimer.Interval = 1000;
                //aTimer.Enabled = true;
                
                Application.Run();
            }
        }
        static private void GetWindowDetails() {

            const int intCharCount = 256; //Number Of Characters For String Buffer

            int intWindowHandle = 0; //Window Handle

            StringBuilder strWindowText = new StringBuilder(intCharCount); //Set Up String Builder To Hold Text From GWT API

            intWindowHandle = GetForegroundWindow(); //get Current Active Window

            if (GetWindowText(intWindowHandle, strWindowText, intCharCount) > 0) //If It Has A Caption
            {
                string appProcessName = System.Diagnostics.Process.GetProcessById(GetWindowProcessID(intWindowHandle)).ProcessName;
                //System.Diagnostics.Debug.WriteLine(appProcessName);
                List<ApplicationMapping> list = JsonConvert.DeserializeObject<List<ApplicationMapping>>(Properties.Settings.Default.ApplicationDictionary);
                if (list.Exists(x => x.Application == appProcessName)) {
                    //System.Diagnostics.Debug.WriteLine(appProcessName + " matches " + list.Find(x => x.Application == appProcessName).Layer);
                }
            }

        }

        static private void tmrHandle_Tick(object sender, EventArgs e) {
            GetWindowDetails(); //Call Get Window Details Sub
        }

        static private Int32 GetWindowProcessID(Int32 hwnd) {
            Int32 pid = 1;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }
    }

}