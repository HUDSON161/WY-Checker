using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace WY_Checker
{
    public static class MessageCloser
    {
        public static TextBox TM;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 msg, ushort wParam, int lParam);


        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        public static void CloseAll()
        {
            foreach (Form form in Application.OpenForms)
            {
                if ( !form.Equals(Form.ActiveForm) )
                {
                    IntPtr handle = FindWindow(null, TM.Text);
                    //MessageBox.Show(handle.ToString());
                    if ( (int)handle != 0 )
                    {
                        //MessageBox.Show(handle.ToString());
                        //DestroyWindow(handle);
                        SendMessage(handle, WM_SYSCOMMAND, SC_CLOSE, 0);
                    }
                    Thread.Sleep(50);
                    break;
                }
            }
        }
    }
}
