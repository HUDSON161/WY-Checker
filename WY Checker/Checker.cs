using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using MSTSCLib;

namespace WY_Checker
{
    static class Checker
    {
        public static long Variants = 0;
        public static long VariantsC = 0;
        public static Thread[] ScanThreads;//потоки
        public static bool[] ScanDone;//статус потоков
        public static string RezTextBuffer = "";
        public static string InvalidTextBuffer = "";

        private static int ChoiseThread()//выбираем неактивный поток
        {
            int ind = -1;
            while ( ind == -1 )//держим поток пока другие потоки не отработают и освободят слот под новый поток
            {
                //MessageBox.Show(ScanThreads.Length.ToString());
                for (int i = 0; i < ScanThreads.Length; i++)
                {
                    if (ScanThreads[i].IsAlive == false) 
                    {
                        //MessageBox.Show(i.ToString());
                        //ScanDone[i] = false;
                        ScanDone[i] = false;
                        ind =i;
                        break;
                    }
                }
                Thread.Sleep(1);
            }
            return ind;
        }

        public static string Check(string url, string ProxyLine,ref TextBox TextBoxT,ref string Checked0)//проверка 
        {
            string Filtered = "";
            bool bTry = false;

            try
            {
                Filtered = Filter.CleanMask(ProxyLine, "0123456789.:");
                Checked0 = Filtered;
                string[] Proxy = Filtered.Split(':');
                WebRequest req = WebRequest.Create(url);
                req.Proxy = new WebProxy(Proxy[0], Convert.ToInt32(Proxy[1]));
                req.Timeout = Convert.ToInt32(TextBoxT.Text);
                req.GetResponse();
                bTry = true;
            }
            catch
            {

            }
            if (bTry)
                return Filtered;
            else
                return "";
        }

        public static string Check2(string url, string ProxyLine, string timeout, ref string Checked0)//проверка 
        {
            string Filtered = "";
            bool bTry = false;
            //MessageBox.Show("ggggg");
            try
            {
                Filtered = Filter.CleanMask(ProxyLine, "0123456789.:");
                Checked0 = Filtered;
                string[] Proxy = Filtered.Split(':');
                WebRequest req = WebRequest.Create(url);
                req.Proxy = new WebProxy(Proxy[0], Convert.ToInt32(Proxy[1]));
                req.Timeout = Convert.ToInt32(timeout);
                req.GetResponse();
                bTry = true;
            }
            catch
            {

            }
            if (bTry)
                return Filtered;
            else
                return "";
        }

        public static void CleanAllProxies(ref TextBox TextBoxL ,ref TextBox TextBoxR, ref TextBox TextBoxU, ref TextBox TextBoxT,ref ProgressBar PrB,ref CheckBox CB0,ref TextBox HelpBox,ref TextBox TTB,bool RDPt,ref bool[] axMsRdpClient7NotSafeForScriptingIsFree,ref AxMSTSCLib.AxMsRdpClient7NotSafeForScripting[] axMsRdpClient7NotSafeForScripting,int timeoutX,string RemoteProgramString,string RemoteArgs, bool bRPEnabled)//пройдем по всем строкам и запишем валидные в текстбокс результата
        {
            string Utext = TextBoxU.Text;
            string TimeLine = TextBoxT.Text;
            bool CBCheck = CB0.Checked;
            bool bTry = false;
            string TlineT;
            //string TlineT;
            int NumT;
            for (int i = 0; i < TextBoxL.Lines.Length; i++)
            {
                NumT = ChoiseThread();
                //Tline = TextBoxL.Lines[i];
                bTry = false;
                ScanThreads[NumT] = null;
                ScanDone[NumT] = false;
                try
                {
                    TlineT = TextBoxL.Lines[i];
                    ScanThreads[NumT] = new Thread(delegate (object TlineTObj)
                    {
                        string Checked = "";
                        string CheckedT = "";
                        if ( RDPt == false)
                        {
                            Checked = Check2(Utext, (string)TlineTObj, TimeLine, ref CheckedT);
                            if (Checked != "")
                            {
                                RezTextBuffer += Checked + Environment.NewLine;
                            }
                            else if (CBCheck == true && CheckedT != "")
                            {
                                InvalidTextBuffer += CheckedT + Environment.NewLine;
                            }
                        }
                        else
                        {
                            //MessageBox.Show("gwgw");
                            Checked = PortScanner.CheckRDP((string)TlineTObj, timeoutX,RemoteProgramString,RemoteArgs, bRPEnabled);
                             if (Checked != "")
                            {
                                RezTextBuffer += Checked + Environment.NewLine;
                            }
                            else if (  CBCheck == true && CheckedT != "")
                            {
                                InvalidTextBuffer += CheckedT + Environment.NewLine;
                            }
                        }
                        VariantsC++;
                        ScanDone[NumT] = true;
                        Thread.Sleep(1000);
                        //MessageBox.Show(rez);
                        //................................
                    });
                    ScanThreads[NumT].Start(TlineT);
                    bTry = true;
                }
                catch
                {

                }
                if ( !bTry )
                {
                    ScanThreads[NumT] = null;
                    VariantsC++;
                    ScanDone[NumT] = true;
                }
                //MessageBox.Show(PrB.Value.ToString());
                //Thread.Sleep(1000);
            }
        }
    }
}
