using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using MSTSCLib;


namespace WY_Checker
{
    public static class PortScanner
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static string RezTextBuffer="";
        public static long Variants = 0;
        public static long VariantsC = 0;
        public static long VariantsS = 0;
        public static long VariantsB = 0;
        public static Thread[] ScanThreads;//потоки
        public static bool[] ScanDone;//статус потоков
        public static bool[] ScanDonePrev;//статус потоков
        public static TextBox TBL;
        public static TextBox TBR;
        public static bool SimpleIP = true;
        public static bool bJustLoginned;
        public static AxMSTSCLib.AxMsRdpClient7NotSafeForScripting TT;
        public static bool[] Loginned = new bool[500];

        public static ComboBox CB1;
        public static ComboBox CB2;
        public static ComboBox CB3;

        public static void Restart()
        {
            RezTextBuffer = "";
            Variants = 0;
            VariantsC = 0;
            VariantsS = 0;

        }

        public static int ChoiseThread(ref TextBox T1)//выбираем неактивный поток
        {
            int rez = -1;

            while (rez == -1)//держим поток пока другие потоки не отработают и освободят слот под новый поток
            {
                //MessageBox.Show(ScanThreads.Length.ToString());
                for (int i = 0; i < ScanThreads.Length; i++)
                {
                    if (ScanThreads[i].IsAlive == false )
                    {
                        //MessageBox.Show(i.ToString());
                        return i;
                    }
                }
                Thread.Sleep(1);
            }
            return -1;
        }

        public static void AnalyseRez(string rez0,string url,string timeout, bool bProxy,bool bRDP,int RdpT,string pluser)//проанализируем наш результат
        {
            string Checked0="";

            if ( bRDP == true )
            {
                //MessageBox.Show(rez0);
                if (rez0 != "" && !RezTextBuffer.Contains(rez0 + pluser) )//если такого нет то анализируем все сочетания
                {
                    //MessageBox.Show(rez0 + pluser);
                    Checked0 = CheckRDP(rez0 + pluser, RdpT,"","",false);
                    if (Checked0 != "")
                    RezTextBuffer += Checked0 + Environment.NewLine;
                }
            }
            else
            {
                if (bProxy == true)
                {
                    //MessageBox.Show("vdhe");
                    if (rez0 != "" && !RezTextBuffer.Contains(rez0) && Checker.Check2(url, rez0, timeout, ref Checked0) != "")//если такого нет то добавляем в общий список
                    {
                        RezTextBuffer += Checked0 + Environment.NewLine;
                    }
                }
                else
                {
                    //MessageBox.Show("vdhe");
                    if (rez0 != "" && !RezTextBuffer.Contains(rez0))//если такого нет то добавляем в общий список
                    {
                        RezTextBuffer += rez0 + Environment.NewLine;
                    }
                }
            }
        }

         public static string CheckRDP(string basicway,int Timeout0,string RemoteString,string RemoteArgs,bool bRPEnabled)//проверка текущего RDP соединения используя логин , домен и пароль
        {
            string rez = "";
            //string BV = "";
            int Client=-1;
            string BUFIP="";
            string BUFPort="";
            //MessageBox.Show("nnjr");
            while ( Client < 0 )
            {
                Client = RDP_Initializer.FreeNum(ref WYForm.axMsRdpClient7NotSafeForScriptingIsFree);
                if ( Client == -1 )
                {
                    Thread.Sleep(100);
                }
            }
            //MessageBox.Show("jjjj");
            WYForm.axMsRdpClient7NotSafeForScriptingIsFree[Client] = false;
             //Form1.axMsRdpClient7NotSafeForScripting[Client].OnChannelReceivedData += delegate (object sender, AxMSTSCLib.IMsTscAxEvents_OnUserNameAcquiredEvent e) { Loginned = "true"; MessageBox.Show("asassaas"); throw new NotImplementedException(); };
            string basicwayStart = basicway;
            basicway =Filter.RemoveAfterSpace(basicway);
            basicway = Filter.RemoveBeforeSpace(basicway);
            //BV = basicway;
            int dividers = 0;
            int dividert = 0;
            int[] divider = new int[3];
            int doublepoint = 0;
            int points = 0;

            for (int i = 0; i < basicway.Length; i++)
            {
                dividert = dividers;
                if (basicway[i] == ':' && dividers == 0)
                {
                    doublepoint = i;//нашли двоеточие
                }
                if (doublepoint != 0 && dividert == 0 && (basicway[i] == '@' || basicway[i] == ';' || basicway[i] == '\\') && dividert < 3)
                {
                    divider[dividers] = i;//нашли очередной разделитель
                    dividers++;
                }
                if (doublepoint != 0 && dividert != 0 && (basicway[i] == ';' || basicway[i] == '\\') && dividert < 3)
                {
                    divider[dividers] = i;//нашли очередной разделитель
                    dividers++;
                }
            }
            for (int i = 0; i < doublepoint; i++)
            {
                if (basicway[i] == '.')
                {
                    points++;//нашли точку
                }
            }
            //MessageBox.Show(dividers.ToString());
            //MessageBox.Show("Connected = "+rdp.Connected.ToString());

            if ( points == 3 && dividers == 3 && doublepoint != 0 && divider[0] != doublepoint && divider[0] != divider[1] && divider[1] != divider[2] )//если найдены 3 разделителя и двоеточие не в начале то это корректная строка
            {
                try
                {
                    BUFIP= basicway.Substring(0, doublepoint);
                    BUFPort= basicway.Substring(doublepoint + 1, divider[0] - doublepoint - 1);
                    Loginned[Client] = false;
                    if (CB3.Text == "0")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 0;
                    }
                    if (CB3.Text == "1")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 1;
                    }
                    if (CB3.Text == "2")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 2;
                    }
                    if (CB2.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = false;
                    }
                    if (CB2.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = true;
                    }
                    if (CB1.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = false;
                    }
                    if (CB1.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = true;
                    }
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Server = BUFIP; //адрес удаленной машины
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort = Convert.ToInt32(BUFPort); //порт соединения
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Domain = basicway.Substring(divider[0] + 1, divider[1] - divider[0] - 1); //домен
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = basicway.Substring(divider[1] + 1, divider[2] - divider[1] - 1); //логин
                    if ( divider[2] != basicway.Length - 1 )
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = basicway.Substring(divider[2] + 1, basicway.Length - divider[2] - 1); //пароль
                    }
                    else
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                    }
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.DisplayConnectionBar = true;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EncryptionEnabled = -1;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Connect();
                    //MessageBox.Show("Connected = " + Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                    //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Server + ":" + Form1.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort + ";" + Form1.axMsRdpClient7NotSafeForScripting[Client].Domain + "\\" + Form1.axMsRdpClient7NotSafeForScripting[Client].UserName + ";" + basicway.Substring(divider[2] + 1, basicway.Length - divider[2] - 1));
                }
                catch (Exception Ex)
                {

                }

                if (bJustLoginned == true)
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while ( Loginned[Client] == false && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1 && Loginned[Client] == true)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = basicway;
                            Loginned[Client] = false;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 2 && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = BUFIP + ":" + BUFPort;
                            Loginned[Client] = false;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            if ( points == 3 && dividers == 2 && doublepoint != 0 && divider[0] != doublepoint && divider[0] != divider[1] )//если найдены 2 разделителя и двоеточие не в начале то это строка без домена
            {
                if ( divider[1] != '\\')
                {
                    try
                    {
                        BUFIP = basicway.Substring(0, doublepoint);
                        BUFPort = basicway.Substring(doublepoint + 1, divider[0] - doublepoint - 1);
                        Loginned[Client] = false;
                        if (CB3.Text == "0")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 0;
                        }
                        if (CB3.Text == "1")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 1;
                        }
                        if (CB3.Text == "2")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 2;
                        }
                        if (CB2.Text == "false")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = false;
                        }
                        if (CB2.Text == "true")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = true;
                        }
                        if (CB1.Text == "false")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = false;
                        }
                        if (CB1.Text == "true")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = true;
                        }
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].Server = BUFIP; //адрес удаленной машины
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort = Convert.ToInt32(BUFPort); //порт соединения
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = basicway.Substring(divider[0] + 1, divider[1] - divider[0] - 1); //логин
                        if (divider[1] != basicway.Length - 1)
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = basicway.Substring(divider[1] + 1, basicway.Length - divider[1] - 1); //пароль
                        }
                        else
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                        }
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EncryptionEnabled = -1;
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].Connect();
                        //MessageBox.Show("Connected = " + Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                        //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Server + ":" + Form1.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort + ";" + Form1.axMsRdpClient7NotSafeForScripting[Client].UserName + ";" + basicway.Substring(divider[1] + 1, basicway.Length - divider[1] - 1));
                    }
                    catch (Exception Ex)
                    {

                    }
                }
                else
                {
                    try
                    {
                        BUFIP = basicway.Substring(0, doublepoint);
                        BUFPort = basicway.Substring(doublepoint + 1, divider[0] - doublepoint - 1);
                        Loginned[Client] = false;
                        if (CB3.Text == "0")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 0;
                        }
                        if (CB3.Text == "1")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 1;
                        }
                        if (CB3.Text == "2")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 2;
                        }
                        if (CB2.Text == "false")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = false;
                        }
                        if (CB2.Text == "true")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = true;
                        }
                        if (CB1.Text == "false")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = false;
                        }
                        if (CB1.Text == "true")
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = true;
                        }
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].Server = BUFIP; //адрес удаленной машины
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort = Convert.ToInt32(BUFPort); //порт соединения
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].Domain = basicway.Substring(divider[0] + 1, divider[1] - divider[0] - 1); //домен
                        if (divider[1] != basicway.Length - 1)
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = basicway.Substring(divider[1] + 1, basicway.Length - divider[1] - 1); //логин
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                        }
                        else
                        {
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = "l"; //логин
                            WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                        }
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.DisplayConnectionBar = true;
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EncryptionEnabled = -1;
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].Connect();
                        //MessageBox.Show("Connected = " + Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                        //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Server + ":" + Form1.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort + ";" + Form1.axMsRdpClient7NotSafeForScripting[Client].UserName + ";" + basicway.Substring(divider[1] + 1, basicway.Length - divider[1] - 1));
                    }
                    catch (Exception Ex)
                    {

                    }
                }

                if (bJustLoginned == true)
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (Loginned[Client] == false && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1 && Loginned[Client] == true)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = basicway;
                            Loginned[Client] = false;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 2 && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = BUFIP + ":" + BUFPort;
                            Loginned[Client] = false;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            if (points == 3 && dividers == 1 && doublepoint != 0 && divider[0] != doublepoint )
            {
                try
                {
                    BUFIP = basicway.Substring(0, doublepoint);
                    BUFPort = basicway.Substring(doublepoint + 1, divider[0] - doublepoint - 1);
                    Loginned[Client] = false;
                    if (CB3.Text == "0")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 0;
                    }
                    if (CB3.Text == "1")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 1;
                    }
                    if (CB3.Text == "2")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 2;
                    }
                    if (CB2.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = false;
                    }
                    if (CB2.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = true;
                    }
                    if (CB1.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = false;
                    }
                    if (CB1.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = true;
                    }
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Server = BUFIP; //адрес удаленной машины
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort = Convert.ToInt32(BUFPort); //порт соединения
                    if (divider[0] != basicway.Length - 1)
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = basicway.Substring(divider[0] + 1, basicway.Length - divider[0] - 1); //логин
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                    }
                    else
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = "l"; //логин
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                    }
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.DisplayConnectionBar = true;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EncryptionEnabled = -1;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Connect();
                    //MessageBox.Show("Connected = " + Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                    //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Server + ":" + Form1.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort + ";" + Form1.axMsRdpClient7NotSafeForScripting[Client].Domain + "\\" + Form1.axMsRdpClient7NotSafeForScripting[Client].UserName + ";" + basicway.Substring(divider[2] + 1, basicway.Length - divider[2] - 1));
                }
                catch (Exception Ex)
                {

                }

                if (bJustLoginned == true)
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (Loginned[Client] == false && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1 && Loginned[Client] == true)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = basicway;
                            Loginned[Client] = false;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 2 && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = BUFIP + ":" + BUFPort;
                            Loginned[Client] = false;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            if (points == 3 && dividers == 0 && doublepoint != 0 && doublepoint != basicway.Length - 1)
            {
                try
                {
                    BUFIP = basicway.Substring(0, doublepoint);
                    BUFPort = basicway.Substring(doublepoint + 1, basicway.Length - doublepoint - 1);
                    Loginned[Client] = false;
                    if (CB3.Text == "0")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 0;
                    }
                    if (CB3.Text == "1")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 1;
                    }
                    if (CB3.Text == "2")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.AuthenticationLevel = 2;
                    }
                    if (CB2.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = false;
                    }
                    if (CB2.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.NegotiateSecurityLayer = true;
                    }
                    if (CB1.Text == "false")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = false;
                    }
                    if (CB1.Text == "true")
                    {
                        WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EnableCredSspSupport = true;
                    }
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Server = basicway.Substring(0, doublepoint); //адрес удаленной машины
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort = Convert.ToInt32(BUFPort); //порт соединения
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.DisplayConnectionBar = true;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.EncryptionEnabled = -1;
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].UserName = "l"; //логин
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.ClearTextPassword = "p";
                    WYForm.axMsRdpClient7NotSafeForScripting[Client].Connect();
                    //MessageBox.Show("Connected = " + Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                    //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Server + ":" + Form1.axMsRdpClient7NotSafeForScripting[Client].AdvancedSettings8.RDPPort + ";" + Form1.axMsRdpClient7NotSafeForScripting[Client].Domain + "\\" + Form1.axMsRdpClient7NotSafeForScripting[Client].UserName + ";" + basicway.Substring(divider[2] + 1, basicway.Length - divider[2] - 1));
                }
                catch (Exception Ex)
                {

                }

                if (bJustLoginned == true)
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (Loginned[Client] = false && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1 && Loginned[Client] == true)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = basicway;
                            Loginned[Client] = false;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        while (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 2 && sw.ElapsedMilliseconds < Timeout0)
                        {
                            Thread.Sleep(100);
                        }
                        sw.Stop();

                        if (WYForm.axMsRdpClient7NotSafeForScripting[Client].Connected == 1)
                        {
                            //MessageBox.Show(Form1.axMsRdpClient7NotSafeForScripting[Client].Connected.ToString());
                            //rez = basicwayStart;
                            rez = BUFIP + ":" + BUFPort;
                            Loginned[Client] = false;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            try
            {
                try
                {
                    MessageBox.Show("LOH_SIDR");
                    if (bRPEnabled) WYForm.axMsRdpClient7NotSafeForScripting[Client].RemoteProgram.ServerStartProgram(RemoteString, "", RemoteString.Remove(RemoteString.LastIndexOf('\\')), true, "", true);
                }
                catch (Exception u)
                {
                    MessageBox.Show(u.Message);
                }
                
                WYForm.axMsRdpClient7NotSafeForScripting[Client].Disconnect();
            }
            catch (Exception e)
            {

            }
            WYForm.axMsRdpClient7NotSafeForScriptingIsFree[Client] = true;

            //MessageBox.Show("bbbbb");

            return rez;
        }


        private static void ThreadScan(string Ip,string Port,ref TextBox T1,ref TextBox TextBoxR,string url,string timeout, bool bProxy,bool bSimpleProxy,string ptimeout,bool bRDP,int RDPtimeout,string pluser)
        {
            string rez;
            //MessageBox.Show(Ip+":"+Port);

            if (Port.Contains('~'))
            {
                string[] ts = Port.Split('~');
                int Start=Convert.ToInt32(ts[0]);
                int End = Convert.ToInt32(ts[1]);
                if ( Start <= End )
                {
                    for (int i = Start; i <= End; i++)
                    {
                        if (VariantsB - VariantsC <= 1)
                        {
                            //MessageBox.Show("fhhwehw");
                            int NumT = ChoiseThread(ref T1);
                            //MessageBox.Show(i.ToString());
                            ScanDonePrev[NumT] = ScanDone[NumT];
                            ScanDone[NumT] = false;
                            try
                            {
                                ScanThreads[NumT] = null;
                                ScanThreads[NumT] = new Thread(delegate ()
                                {
                                    if (bSimpleProxy == false || bRDP == true)
                                    {
                                        rez = Scan(Ip, i.ToString(), ptimeout);
                                    }
                                    else
                                    {
                                        rez = Ip + ":" + i.ToString();
                                    }
                                    AnalyseRez(rez, url, timeout, bProxy, bRDP, RDPtimeout, pluser);
                                    VariantsC++;
                                    ScanDone[NumT] = true;
                                    Thread.Sleep(1000);
                                    //MessageBox.Show(VariantsC.ToString());
                                    //................................
                                });
                                ScanThreads[NumT].Start();
                            }
                            catch
                            {
                                ScanThreads[NumT] = null;
                                VariantsC++;
                                ScanDone[NumT] = true;
                                //MessageBox.Show(VariantsC.ToString());
                            }
                            //Thread.Sleep(50);
                        }
                        else 
                        {
                            VariantsC++;
                        }
                    }
                }
                else
                {
                    for (int i = End; i <= Start; i++)
                    {
                        if (VariantsB - VariantsC <= 1)
                        {
                            int NumT = ChoiseThread(ref T1);
                            //MessageBox.Show(i.ToString());
                            ScanDonePrev[NumT] = ScanDone[NumT];
                            ScanDone[NumT] = false;
                            try
                            {
                                ScanThreads[NumT] = null;
                                ScanThreads[NumT] = new Thread(delegate ()
                                {
                                    if (bSimpleProxy == false || bRDP == true)
                                    {
                                        rez = Scan(Ip, i.ToString(), ptimeout);
                                    }
                                    else
                                    {
                                        rez = Ip + ":" + i.ToString();
                                    }
                                    //MessageBox.Show(rez);
                                    AnalyseRez(rez, url, timeout, bProxy, bRDP, RDPtimeout, pluser);
                                    VariantsC++;
                                    ScanDone[NumT] = true;
                                    Thread.Sleep(1000);
                                    //MessageBox.Show(VariantsC.ToString());
                                    //................................
                                });
                                ScanThreads[NumT].Start();
                            }
                            catch
                            {
                                ScanThreads[NumT] = null;
                                VariantsC++;
                                ScanDone[NumT] = true;
                                //MessageBox.Show(VariantsC.ToString());
                            }
                            //Thread.Sleep(50);

                        }
                        else 
                        {
                            VariantsC++;
                        }
                    }
                }
            }
            else
            {
                //MessageBox.Show(VariantsB.ToString());
                //MessageBox.Show(VariantsC.ToString());
                if (VariantsB - VariantsC <= 1)
                {
                    //MessageBox.Show("S");
                    int NumT = ChoiseThread(ref T1);
                    //MessageBox.Show(NumT.ToString());
                    ScanDonePrev[NumT] = ScanDone[NumT];
                    ScanDone[NumT] = false;
                    try
                    {
                        ScanThreads[NumT] = null;
                        ScanThreads[NumT] = new Thread(delegate ()
                        {
                            if (bSimpleProxy == false || bRDP == true )
                            {
                                rez = Scan(Ip, Port, ptimeout);
                            }
                            else
                            {
                                rez = Ip + ":" + Port;
                            }
                            //MessageBox.Show(rez);
                            AnalyseRez(rez, url, timeout, bProxy, bRDP,RDPtimeout, pluser);
                            VariantsC++;
                            ScanDone[NumT] = true;
                            Thread.Sleep(1000);
                            //MessageBox.Show(VariantsC.ToString());
                            //................................
                        });
                        ScanThreads[NumT].Start();
                    }
                    catch
                    {
                        ScanThreads[NumT] = null;
                        VariantsC++;
                        ScanDone[NumT] = true;
                        //MessageBox.Show(VariantsC.ToString());
                    }
                    //Thread.Sleep(50);

                }
                else
                {
                    VariantsC++;
                }
            }
        }

        private static void IThreadScan(string Ip, string Port, ref TextBox T1, ref TextBox TextBoxR)
        {
            //MessageBox.Show("sesbbenbebb");
            if (Port.Contains('~'))
            {
                string[] ts = Port.Split('~');
                int Start = Convert.ToInt32(ts[0]);
                int End = Convert.ToInt32(ts[1]);
                //MessageBox.Show(ts[0]);
                //MessageBox.Show(ts[1]);
                if (Start <= End)
                {
                    for (int i = Start; i <= End; i++)
                    {
                        Variants++;
                    }
                }
                else
                {
                    for (int i = End; i <= Start; i++)
                    {
                        Variants++;
                    }
                }
            }
            else
            {
                Variants++;
            }
        }

        private static void FractalChain(int index0,ref int[] A0)//поочередное изменение величины каждого измерения
        {
            if ( index0 >= 0 )
            {
                if (A0[index0] < 255)
                {
                    A0[index0]++;
                }
                else
                {
                    A0[index0] = 0;
                    FractalChain(index0 - 1, ref A0);
                }
            }
        }

        public static void ScanAllDiapazons(ref TextBox TextBoxL, ref TextBox TextBoxR, ref TextBox T1, string url, string timeout, bool bProxy,bool bLoadProgress,ref ProgressBar Progress, ref TextBox TP, bool bSimpleProxy,string ptimeout,bool bRDP,int RDPtimeout)//просканируем все диапазоны
        {
            string[] IpP;

            if ( bRDP )
            {
                uint Multiplier = 0;
                string pluser = "";
                for (int h = 0; h < TBL.Lines.Length; h++)
                {
                    if (TBL.Lines[h] != "")
                    {
                        for (int j = 0; j < TBR.Lines.Length; j++)
                        {
                            if (TBR.Lines[j] != "")
                            {
                                pluser = ";" + TBL.Lines[h] + ";" + TBR.Lines[j];
                                for (int i = 0; i < TextBoxL.Lines.Length; i++)
                                {
                                    if (TextBoxL.Lines[i].Contains("-"))
                                    {
                                        IpP = TextBoxL.Lines[i].Split('-');
                                        ScanByDiapazon(IpP[0], IpP[1], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                                    }
                                    else
                                    {
                                        ScanByDiapazon(TextBoxL.Lines[i], TextBoxL.Lines[i], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                                    }

                                }
                                Multiplier ++;
                            }
                            else
                            {
                                pluser = ";" + TBL.Lines[h];
                                for (int i = 0; i < TextBoxL.Lines.Length; i++)
                                {
                                    if (TextBoxL.Lines[i].Contains("-"))
                                    {
                                        IpP = TextBoxL.Lines[i].Split('-');
                                        ScanByDiapazon(IpP[0], IpP[1], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                                    }
                                    else
                                    {
                                        ScanByDiapazon(TextBoxL.Lines[i], TextBoxL.Lines[i], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                                    }

                                }
                                Multiplier++;
                            }
                            //MessageBox.Show(pluser);
                        }
                    }

                }
                if ( Multiplier == 0 )
                {
                    for (int i = 0; i < TextBoxL.Lines.Length; i++)
                    {
                        if (TextBoxL.Lines[i].Contains("-"))
                        {
                            IpP = TextBoxL.Lines[i].Split('-');
                            ScanByDiapazon(IpP[0], IpP[1], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                        else
                        {
                            ScanByDiapazon(TextBoxL.Lines[i], TextBoxL.Lines[i], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }

                    }
                }
            }
            else
            {

                for (int i = 0; i < TextBoxL.Lines.Length; i++)
                {
                    if (TextBoxL.Lines[i].Contains("-"))
                    {
                        IpP = TextBoxL.Lines[i].Split('-');
                        ScanByDiapazon(IpP[0], IpP[1], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, "");
                    }
                    else
                    {
                        ScanByDiapazon(TextBoxL.Lines[i], TextBoxL.Lines[i], ref T1, ref TextBoxR, url, timeout, bProxy, bLoadProgress, ref Progress, ref TextBoxL, ref TextBoxR, ref TP, bSimpleProxy, ptimeout, bRDP, RDPtimeout, "");
                    }

                }
            }
        }

        public static void CalcAllDiapazons(ref TextBox TextBoxL, ref TextBox TextBoxR, ref TextBox T1, ref TextBox TP)//просканируем все диапазоны
        {
            string[] IpP;
            for (int i = 0; i < TextBoxL.Lines.Length; i++)
            {
                if (TextBoxL.Lines[i].Contains("-") )
                {
                    IpP = TextBoxL.Lines[i].Split('-');
                    CalcVariants(IpP[0], IpP[1], ref T1, ref TextBoxR, ref TP);
                }
                else
                {
                    CalcVariants(TextBoxL.Lines[i], TextBoxL.Lines[i], ref T1, ref TextBoxR, ref TP);
                }

            }
        }

        private static void ScanByDiapazon(string StrS,string StrE, ref TextBox T1,ref TextBox TR,string url,string timeout, bool bProxy,bool bLoadProgress,ref ProgressBar Progress, ref TextBox TextBoxC, ref TextBox TextBoxR,ref TextBox TP, bool bSimpleProxy,string ptimeout,bool bRDP,int RDPtimeout,string pluser)//построить айпи по диапазону
        {
            int[] IpStartDims = new int[4];//числа соответствия измерений нижнего диапазона
            int[] IpEndDims = new int[4];//числа соответствия верхнего
            string[] PortDims = new string[1];
            int Points = 0;
            int Start=0;
            int End=0;
            string StrT;
            string PortT;

            string OriginalStrS= Filter.CleanMask(StrS, "0123456789.,:~");
            string OriginalStrE = Filter.CleanMask(StrE, "0123456789.,:~");
            string[] StrPartS= new string[2];
            string[] StrPartE = new string[2];

            if ( !OriginalStrS.Contains(':') )
            {
                StrPartS[0] = OriginalStrS;
                StrPartE[0] = OriginalStrE;
                StrPartE[1] = TP.Text;
            }
            else
            {
                StrPartS = OriginalStrS.Split(':');
                StrPartE = OriginalStrE.Split(':');
            }
            int PortStartI = 0;
            int PortEndI = 0;

            bool bDifficultPort = false;
            int Ports = 0;

            if (!OriginalStrS.Contains(':'))
            {
                if (StrPartS.Length == 2)
                {
                    //MessageBox.Show(TP.Lines.Length.ToString());
                    for (int i = 0; i < TP.Lines.Length; i++)
                    {
                        if (i == 0)
                        {
                            StrPartS[1] = Filter.CleanMask(TP.Lines[i], "0123456789,~");
                        }
                        else
                        {
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] != ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] != ',')
                            {
                                StrPartS[1] += ',' + Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] == ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] != ',')
                            {
                                StrPartS[1] += Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] != ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] == ',')
                            {
                                StrPartS[1] += Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length == 0)
                            {
                                StrPartS[1] = Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                        }
                    }
                    //MessageBox.Show(StrPartS[0]);
                }
            }

            if ( StrPartS[1].Contains(',') || StrPartS[1].Contains('~') )
            {
                for (int i = 0; i < StrPartS[1].Length; i++)
                {
                    StrT = StrPartS[1].Substring(i, 1);
                    if (StrT == ",")
                    {
                        End = i;
                        Ports++;
                        Points++;
                        Start = End + 1;
                    }

                    if (i == StrPartS[1].Length - 1)
                    {
                        End = i + 1;
                        Ports++;
                        Points++;
                        Start = End + 1;
                    }
                }

                Points = 0;
                Start = 0;
                End = 0;

                PortDims = new string[Ports];

                for (int i = 0; i < StrPartS[1].Length; i++)
                {
                    StrT = StrPartS[1].Substring(i, 1);
                    if (StrT == ",")
                    {
                        End = i;
                        PortDims[Points] = StrPartS[1].Substring(Start, End - Start);
                        Points++;
                        Start = End + 1;
                    }

                    if (i == StrPartS[1].Length - 1)
                    {
                        End = i + 1;
                        PortDims[Points] = StrPartS[1].Substring(Start, End - Start);
                        Points++;
                        Start = End + 1;
                    }
                }


                Points = 0;
                Start = 0;
                End = 0;
                bDifficultPort = true;
            }

            if ( !bDifficultPort )
            {
                PortStartI = Convert.ToInt32(StrPartS[1]);
                PortEndI = Convert.ToInt32(StrPartE[1]);
            }

            for (int i = 0; i < StrPartS[0].Length; i++)
            {
                StrT= StrPartS[0].Substring(i, 1);
                if ( StrT == "." )
                {
                    End = i;
                    if ( Points <= 3 )
                    {
                        IpStartDims[Points] = Convert.ToInt32(StrPartS[0].Substring(Start, End-Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }

                if ( i == StrPartS[0].Length - 1 )
                {
                    End = i + 1;
                    if (Points <= 3)
                    {
                        IpStartDims[Points] = Convert.ToInt32(StrPartS[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }
            }

            Points = 0;
            Start = 0;
            End = 0;

            for (int i = 0; i < StrPartE[0].Length; i++)
            {
                StrT = StrPartE[0].Substring(i, 1);
                if (StrT == ".")
                {
                    End = i;
                    if (Points <= 3)
                    {
                        IpEndDims[Points] = Convert.ToInt32(StrPartE[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }

                if (i == StrPartE[0].Length - 1)
                {
                    End = i + 1;
                    if (Points <= 3)
                    {
                        IpEndDims[Points] = Convert.ToInt32(StrPartE[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }
            }

            if ( bLoadProgress == true )
            {
                //LoaderSaver.CorrectStart(ref VariantsC,ref Progress,ref TextBoxC,ref TextBoxR);//скорректируем стартовый айпи
            }

            StrT = IpStartDims[0].ToString()+ "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
            PortT = StrPartS[1];
            //MessageBox.Show(StrT);

            while ( StrT != StrPartE[0] )
            {

                VariantsS = VariantsC;
                StrT = IpStartDims[0].ToString() + "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
                if ( StrPartS[1] == StrPartE[1])
                {
                    if (!bDifficultPort)
                    {
                        ThreadScan(StrT, PortT, ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout,bRDP, RDPtimeout, pluser);
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            ThreadScan(StrT, PortDims[b], ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                }
                else
                {
                    if (!bDifficultPort)
                    {
                        for (int i = PortStartI; i <= PortEndI; i++)
                        {
                            ThreadScan(StrT, i.ToString(), ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            ThreadScan(StrT, PortDims[b], ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                }
                FractalChain(3, ref IpStartDims);
            }

            if ( StrT == StrPartE[0] )
            {
                //MessageBox.Show(StrT);
                StrT = IpStartDims[0].ToString() + "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
                if (StrPartS[1] == StrPartE[1])
                {
                    if (!bDifficultPort)
                    {
                        ThreadScan(StrT, PortT, ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            ThreadScan(StrT, PortDims[b], ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                }
                else
                {
                    if (!bDifficultPort)
                    {
                        for (int i = PortStartI; i <= PortEndI; i++)
                        {
                            ThreadScan(StrT, i.ToString(), ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            ThreadScan(StrT, PortDims[b], ref T1, ref TR, url, timeout, bProxy, bSimpleProxy, ptimeout, bRDP, RDPtimeout, pluser);
                        }
                    }
                }

                if (Variants != 0 && bDifficultPort)
                {
                    FractalChain(3, ref IpStartDims);
                    VariantsS = VariantsC;
                }
            }
        }

        private static void CalcVariants(string StrS, string StrE, ref TextBox T1, ref TextBox TR,ref TextBox TP)//посчитать количество вариантов
        {
            int[] IpStartDims = new int[4];//числа соответствия измерений нижнего диапазона
            int[] IpEndDims = new int[4];//числа соответствия верхнего
            string[] PortDims = new string[1];
            int Points = 0;
            int Start = 0;
            int End = 0;
            string StrT;
            string PortT;

            string OriginalStrS = Filter.CleanMask(StrS, "0123456789.,:~");
            string OriginalStrE = Filter.CleanMask(StrE, "0123456789.,:~");
            string[] StrPartS = new string[2];
            string[] StrPartE = new string[2];

            if (!OriginalStrS.Contains(':'))
            {
                StrPartS[0] = OriginalStrS;
                StrPartE[0] = OriginalStrE;
                StrPartE[1] = TP.Text;
            }
            else
            {
                StrPartS = OriginalStrS.Split(':');
                StrPartE = OriginalStrE.Split(':');
            }
            int PortStartI = 0;
            int PortEndI = 0;

            bool bDifficultPort = false;
            int Ports = 0;
            //MessageBox.Show("nrr");
            if (!OriginalStrS.Contains(':'))
            {
                if (StrPartS.Length == 2)
                {
                    //MessageBox.Show(TP.Lines.Length.ToString());
                    for (int i = 0; i < TP.Lines.Length; i++)
                    {
                        if (i == 0)
                        {
                            StrPartS[1] = Filter.CleanMask(TP.Lines[i], "0123456789,~");
                        }
                        else
                        {
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] != ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] != ',')
                            {
                                StrPartS[1] += ',' + Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] == ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] != ',')
                            {
                                StrPartS[1] += Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length != 0 && StrPartS[1][StrPartS[1].Length - 1] != ',' && TP.Lines[i].Length != 0 && TP.Lines[i][0] == ',')
                            {
                                StrPartS[1] += Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                            if (StrPartS[1].Length == 0)
                            {
                                StrPartS[1] = Filter.CleanMask(TP.Lines[i], "0123456789,~");
                            }
                        }
                    }
                    //MessageBox.Show(StrPartS[0]);
                }
            }
            //MessageBox.Show(StrPartS[1]);
            if ( StrPartS[1].Contains(',') || StrPartS[1].Contains('~'))
            {
                for (int i = 0; i < StrPartS[1].Length; i++)
                {
                    StrT = StrPartS[1].Substring(i, 1);
                    if (StrT == ",")
                    {
                        End = i;
                        Ports++;
                        Points++;
                        Start = End + 1;
                    }

                    if (i == StrPartS[1].Length - 1)
                    {
                        End = i + 1;
                        Ports++;
                        Points++;
                        Start = End + 1;
                    }
                }

                Points = 0;
                Start = 0;
                End = 0;

                PortDims = new string[Ports];

                for (int i = 0; i < StrPartS[1].Length; i++)
                {
                    StrT = StrPartS[1].Substring(i, 1);
                    if (StrT == ",")
                    {
                        End = i;
                        PortDims[Points] = StrPartS[1].Substring(Start, End - Start);
                        Points++;
                        Start = End + 1;
                    }

                    if (i == StrPartS[1].Length - 1)
                    {
                        End = i + 1;
                        PortDims[Points] = StrPartS[1].Substring(Start, End - Start);
                        Points++;
                        Start = End + 1;
                    }
                }


                Points = 0;
                Start = 0;
                End = 0;
                bDifficultPort = true;
            }

            if (!bDifficultPort)
            {
                PortStartI = Convert.ToInt32(StrPartS[1]);
                PortEndI = Convert.ToInt32(StrPartE[1]);
            }
            //MessageBox.Show("beet");
            for (int i = 0; i < StrPartS[0].Length; i++)
            {
                StrT = StrPartS[0].Substring(i, 1);
                if (StrT == ".")
                {
                    End = i;
                    if (Points <= 3)
                    {
                        IpStartDims[Points] = Convert.ToInt32(StrPartS[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }

                if (i == StrPartS[0].Length - 1)
                {
                    End = i + 1;
                    if (Points <= 3)
                    {
                        IpStartDims[Points] = Convert.ToInt32(StrPartS[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }
            }

            Points = 0;
            Start = 0;
            End = 0;

            for (int i = 0; i < StrPartE[0].Length; i++)
            {
                StrT = StrPartE[0].Substring(i, 1);
                if (StrT == ".")
                {
                    End = i;
                    if (Points <= 3)
                    {
                        IpEndDims[Points] = Convert.ToInt32(StrPartE[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }

                if (i == StrPartE[0].Length - 1)
                {
                    End = i + 1;
                    if (Points <= 3)
                    {
                        IpEndDims[Points] = Convert.ToInt32(StrPartE[0].Substring(Start, End - Start));
                        //MessageBox.Show(IpStartDims[Points].ToString());
                    }
                    else
                    {
                        break;
                    }
                    Points++;
                    Start = End + 1;
                }
            }
            //MessageBox.Show("fnf");
            StrT = IpStartDims[0].ToString() + "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
            PortT = StrPartS[1];

            //MessageBox.Show(PortT);
            while (StrT != StrPartE[0])
            {

                StrT = IpStartDims[0].ToString() + "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
                if (StrPartS[1] == StrPartE[1])
                {
                    if (!bDifficultPort)
                    {
                        IThreadScan(StrT, PortT, ref T1, ref TR);
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            IThreadScan(StrT, PortDims[b], ref T1, ref TR);
                        }
                    }
                }
                else
                {
                    if (!bDifficultPort)
                    {
                        for (int i = PortStartI; i <= PortEndI; i++)
                        {
                            IThreadScan(StrT, i.ToString(), ref T1, ref TR);
                        }
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            IThreadScan(StrT, PortDims[b], ref T1, ref TR);
                        }
                    }
                }
                FractalChain(3, ref IpStartDims);
            }

            if (StrT == StrPartE[0])
            {
                StrT = IpStartDims[0].ToString() + "." + IpStartDims[1].ToString() + "." + IpStartDims[2].ToString() + "." + IpStartDims[3].ToString();
                if (StrPartS[1] == StrPartE[1])
                {
                    if (!bDifficultPort)
                    {
                        IThreadScan(StrT, PortT, ref T1, ref TR);
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            IThreadScan(StrT, PortDims[b], ref T1, ref TR);
                        }
                    }
                }
                else
                {
                    if (!bDifficultPort)
                    {
                        for (int i = PortStartI; i <= PortEndI; i++)
                        {
                            IThreadScan(StrT, i.ToString(), ref T1, ref TR);
                        }
                    }
                    else
                    {
                        for (int b = 0; b < PortDims.Length; b++)
                        {
                            IThreadScan(StrT, PortDims[b], ref T1, ref TR);
                        }
                    }
                }
                if (Variants != 0 && bDifficultPort )
                {
                    FractalChain(3, ref IpStartDims);
                }
            }
        }

        private static bool _TryPing(string strIpAddress, int intPort, int nTimeoutMsec)//попытка соединения с таймаутом
        {
            Socket socket = null;
            bool bRez=false;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);


                IAsyncResult result = socket.BeginConnect(strIpAddress, intPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(nTimeoutMsec, true);

                bRez = socket.Connected;
            }
            catch
            {
                bRez=false;
            }
            finally
            {
                if (null != socket)
                    socket.Close();
            }
            return bRez;
        }

        public static string Scan(string Ip0,string Port0,string timeout0)//сканирование выбранного сочетания айпи и порта
        {

            bool bRez = _TryPing(Filter.CleanMask(Ip0, "0123456789."), Convert.ToInt32(Filter.CleanMask(Port0, "0123456789")), Convert.ToInt32(Filter.CleanMask(timeout0, "0123456789")));
            if ( bRez )
            {
                return Ip0 + ":" + Port0;
            }
            else
            {
                return "";
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket SockClient = (Socket)ar.AsyncState;
                SockClient.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {

            }
        }
    }
}
