using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;


namespace WY_Checker
{
    static class Program
    {
        public static WYForm MF;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists("AxInterop.MSTSCLib.dll"))
            {
                File.WriteAllBytes("AxInterop.MSTSCLib.dll", Properties.Resources.AxInterop_MSTSCLib);
            }
            if (!File.Exists("Interop.MSTSCLib.dll"))
            {
                File.WriteAllBytes("Interop.MSTSCLib.dll", Properties.Resources.Interop_MSTSCLib);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MF = new WYForm();
            Application.Run(MF);
        }
    }
}
