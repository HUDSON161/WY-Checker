using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace WY_Checker
{
    public static class RDP_Initializer
    {
        public static void Init(ref bool[] axMsRdpClient7NotSafeForScriptingIsFree, ref AxMSTSCLib.AxMsRdpClient7NotSafeForScripting[] axMsRdpClient7NotSafeForScripting,WYForm FormR)
        {
            //MessageBox.Show("fgfgfgfg");
            for (int i = 0; i < 500; i++)//составим массив ссылок на наши RDP модули на форме
            {
                FieldInfo fi = typeof(WYForm).GetField("axMsRdpClient7NotSafeForScripting" + (i+1).ToString());
                //MessageBox.Show("uiuiuiui");
                axMsRdpClient7NotSafeForScripting[i] = (AxMSTSCLib.AxMsRdpClient7NotSafeForScripting)fi.GetValue(FormR);
                axMsRdpClient7NotSafeForScriptingIsFree[i] = true;
                //MessageBox.Show("mmmmmmm");ё+
            }
        }
        public static int FreeNum(ref bool[] axMsRdpClient7NotSafeForScriptingIsFree)
        {
            for (int i = 0; i < 500; i++)//найдем свободный клиент
            {
                if (axMsRdpClient7NotSafeForScriptingIsFree[i] == true)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
