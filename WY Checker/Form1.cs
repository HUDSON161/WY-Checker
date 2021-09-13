using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MSTSCLib;
using System.IO;

namespace WY_Checker
{
    public partial class WYForm : Form
    {
        public WYForm()
        {
            InitializeComponent();
        }
        public Thread fg;
        public Thread t;
        public Thread h;
        public Thread io;
        public Thread ioM;
        public Thread hp;
        public Thread tn;
        public static AxMSTSCLib.AxMsRdpClient7NotSafeForScripting[] axMsRdpClient7NotSafeForScripting=new AxMSTSCLib.AxMsRdpClient7NotSafeForScripting[500];
        public static bool[] axMsRdpClient7NotSafeForScriptingIsFree = new bool[500];//статус текущего RDP клиента
        public static AxMSTSCLib.AxMsRdpClient7NotSafeForScripting TT;
        private static bool bEndScan = true;

        private void button1_Click(object sender, EventArgs e)//кнопка прокси и RDP чекера
        {
            if (fg != null)
            {
                fg.Abort();
            }
            textBox2.Clear();
            textBox7.Clear();
            progressBar1.Value = 0;
            Checker.Variants = textBox1.Lines.Length;
            Checker.VariantsC = 0;
            Checker.RezTextBuffer = "";
            Checker.InvalidTextBuffer = "";

            Checker.ScanThreads = new Thread[Convert.ToInt32(textBox11.Text)];
            Checker.ScanDone = new bool[Convert.ToInt32(textBox11.Text)];

            for (int i = 0; i < Checker.ScanThreads.Length; i++)
            {
                Checker.ScanDone[i] = true;
                Checker.ScanThreads[i] = new Thread(delegate () { });
            }


            tn = new Thread(delegate ()
            {
                RDP_Initializer.Init(ref axMsRdpClient7NotSafeForScriptingIsFree, ref axMsRdpClient7NotSafeForScripting, this);//заполним наш массив ссылками на RDP терминалы на форме (их 500)
                for (int i = 0; i < 500; i++)//составим массив ссылок на наши RDP модули на форме
                {
                    axMsRdpClient7NotSafeForScripting[i].OnLoginComplete += RNRDYN_OnLoginComplete;
                    //MessageBox.Show("mmmmmmm");
                }

                /*
                axMsRdpClient7NotSafeForScripting[0].Server = "49.249.226.138";
                axMsRdpClient7NotSafeForScripting[0].UserName = "manjunath";

                axMsRdpClient7NotSafeForScripting[0].AdvancedSettings8.DisplayConnectionBar = true;
                axMsRdpClient7NotSafeForScripting[0].AdvancedSettings8.ClearTextPassword = "admin@123";
                axMsRdpClient7NotSafeForScripting[0].AdvancedSettings8.EncryptionEnabled = -1;

                //// Start connection
                axMsRdpClient7NotSafeForScripting[0].Connect();

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                MessageBox.Show("Connection Status = " + axMsRdpClient7NotSafeForScripting[0].Connected.ToString());
                while (sw.ElapsedMilliseconds < 10000)
                {
                    int o = 0;
                }
                MessageBox.Show("Connection Status = " + axMsRdpClient7NotSafeForScripting[0].Connected.ToString());
                */
                int TmT = 10000;
                string Fs = Filter.CleanMask(textBox22.Text, "0123456789");
                if ( Fs != "" )
                {
                    TmT = Convert.ToInt32(Filter.CleanMask(textBox22.Text, "0123456789"));
                }

                Checker.CleanAllProxies(ref textBox1, ref textBox2, ref textBox4, ref textBox3, ref progressBar1, ref checkBox1, ref textBox7,ref textBox11,checkBox5.Checked,ref axMsRdpClient7NotSafeForScriptingIsFree,ref axMsRdpClient7NotSafeForScripting, TmT,textBox9.Text,textBox27.Text,checkBox8.Checked);
                //................................
            });
            tn.Start();

            hp = new Thread(delegate () //поток для синхронизации с прогресс баром
            {
                progressBar1.Value = 0;
                if ( Checker.Variants > 0)
                {
                    while (progressBar1.Value < 100)
                    {
                        int Value = (int)(100.0 * ((double)Checker.VariantsC / (double)(Checker.Variants)));
                        if (Value <= 100)
                        {
                            progressBar1.Value = Value;
                            textBox20.Text = Checker.VariantsC.ToString();
                            textBox2.Text = Checker.RezTextBuffer;
                            if (checkBox1.Checked == true)
                            {
                                textBox7.Text = Checker.InvalidTextBuffer;
                            }
                        }
                        else
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                //................................
            });
            hp.Start();

            fg = new Thread(delegate () //поток для уничтожения сообщений
            {
                while ( true )
                {
                    MessageCloser.CloseAll();
                    Thread.Sleep(50);
                }
                //................................
            });
            fg.Start();

        }


        private void button6_Click(object sender, EventArgs e)
        {
            if (tn != null)
            {
                tn.Abort();
            }
            if (hp != null)
            {
                hp.Abort();
            }
            if (fg != null)
            {
                fg.Abort();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox5.Text = textBox1.Lines.Length.ToString();
            textBox19.Text = textBox1.Lines.Length.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox6.Text = textBox2.Lines.Length.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBox1.Checked == true )
            {
                textBox7.Enabled = true;
            }
            else
            {
                textBox7.Enabled = false;
                textBox7.Clear();
            }
        }


        private void button4_Click(object sender, EventArgs e)//кнопка прерывания потока
        {
            bEndScan = false;
            if ( t != null )
            {
                t.Abort();
            }
            if (h != null)
            {
                h.Abort();
            }
            if (io != null)
            {
                io.Abort();
            }
            if (ioM != null)
            {
                ioM.Abort();
            }
            if (fg != null)
            {
                fg.Abort();
            }
        }

        private void button2_Click(object sender, EventArgs e)//кнопка запуска
        {
            bEndScan = false;
            if (t != null)
            {
                t.Abort();
            }
            if (h != null)
            {
                h.Abort();
            }
            if (io != null)
            {
                io.Abort();
            }
            if (ioM != null)
            {
                ioM.Abort();
            }
            if (fg != null)
            {
                fg.Abort();
            }
            PortScanner.ScanThreads = new Thread[Convert.ToInt32(textBox11.Text)];
            PortScanner.ScanDone = new bool[Convert.ToInt32(textBox11.Text)];
            PortScanner.ScanDonePrev = new bool[Convert.ToInt32(textBox11.Text)];

            for (int i = 0; i < PortScanner.ScanThreads.Length; i++)
            {
                PortScanner.ScanDone[i] = true;
                PortScanner.ScanDonePrev[i] = false;
                PortScanner.ScanThreads[i]=new Thread(delegate () { });
            }
            //LoaderSaver.CorrectStart(ref PortScanner.VariantsB, ref progressBar2, ref textBox10, ref textBox12);//*******
            RDP_Initializer.Init(ref axMsRdpClient7NotSafeForScriptingIsFree, ref axMsRdpClient7NotSafeForScripting, this);//заполним наш массив ссылками на RDP терминалы на форме (их 500)
            t = new Thread(delegate ()
            {
                PortScanner.Restart();
                textBox14.Text = "Caculating...";
                textBox15.Text = "0";
                PortScanner.CalcAllDiapazons(ref textBox10, ref textBox12, ref textBox11,ref textBox13);
                if ( checkBox5.Checked )
                {
                    int Multiplier = 0;
                    for (int h = 0; h < PortScanner.TBL.Lines.Length; h++)
                    {
                        if (PortScanner.TBL.Lines[h] != "")
                        {
                            for (int j = 0; j < PortScanner.TBR.Lines.Length; j++)
                            {
                                Multiplier++;
                            }
                        }

                    }
                    if (Multiplier == 0)
                        Multiplier=1;
                    PortScanner.Variants = PortScanner.Variants * Multiplier;
                }
                //MessageBox.Show(PortScanner.Variants.ToString());
                textBox14.Text = PortScanner.Variants.ToString();
                for (int i = 0; i < 500; i++)//составим массив ссылок на наши RDP модули на форме
                {
                    axMsRdpClient7NotSafeForScripting[i].OnLoginComplete += RNRDYN_OnLoginComplete;
                    //MessageBox.Show("mmmmmmm");
                }
                PortScanner.ScanAllDiapazons(ref textBox10, ref textBox12, ref textBox11,textBox4.Text,textBox3.Text,checkBox2.Checked,true,ref progressBar2, ref textBox13,checkBox3.Checked,textBox21.Text,checkBox5.Checked,Convert.ToInt32(textBox22.Text));
                //................................
            });
            t.Start();



            h = new Thread(delegate () //поток для синхронизации с прогресс баром
            {
                progressBar2.Value = 0;
                
                while ( progressBar2.Value < 100 )
                {
                    if (PortScanner.Variants != 0)
                    {
                        int Value =(int)(100.0 * ((double)PortScanner.VariantsC / (double)(PortScanner.Variants)));
                        if ( Value <= 100 )
                        {
                            progressBar2.Value = Value;
                            textBox15.Text = PortScanner.VariantsC.ToString();
                            textBox12.Text = PortScanner.RezTextBuffer;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Thread.Sleep(100);
                    
                }
                //................................
            });
            h.Start();

            
            ioM = new Thread(delegate () //поток для синхронизации с текст боксом
            {
                try
                {
                    while (progressBar2.Value < 100)
                    {
                        LoaderSaver.SaveData(ref PortScanner.VariantsC, ref progressBar2, ref textBox10, ref textBox12, ref textBox13,ref textBox23, ref textBox24);//*******
                        Thread.Sleep(1000);
                    }
                    if (progressBar2.Value >= 100)
                    {
                        LoaderSaver.SaveData(ref PortScanner.VariantsC, ref progressBar2, ref textBox10, ref textBox12, ref textBox13, ref textBox23, ref textBox24);//*******
                        Thread.Sleep(1000);
                    }
                }
                catch
                {

                }
                //................................
            });
            ioM.Start();

            fg = new Thread(delegate () //поток для уничтожения сообщений
            {
                while (true)
                {
                    MessageCloser.CloseAll();
                    Thread.Sleep(50);
                }
                //................................
            });
            fg.Start();
            bEndScan = true;
        }

        private void button5_Click(object sender, EventArgs e)//нажатие кнопки восстановления данных
        {
            io = new Thread(delegate () //поток для восстановления данных
            {
                LoaderSaver.CorrectStart("Save.txt",ref PortScanner.VariantsB, ref progressBar2, ref textBox10, ref textBox12, ref textBox13, ref textBox23, ref textBox24);//*******
            });
            io.Start();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBox3.Checked == false )
            {
                checkBox2.Enabled = true;
            }
            else
            {
                checkBox2.Enabled = false;
            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            textBox16.Text = textBox10.Lines.Length.ToString();
        }
        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            textBox17.Text = textBox12.Lines.Length.ToString();
        }
        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            textBox18.Text = textBox13.Lines.Length.ToString();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBox4.Checked == true )
            {
                checkBox5.Checked = false;
            }
            else
            {
                checkBox5.Checked = true;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox7.Enabled = true;
                checkBox6.Enabled = true;
            }
            else
            {
                checkBox4.Checked = true;
                checkBox7.Enabled = false;
                checkBox6.Enabled = false;
            }

            if (checkBox5.Checked == true)
            {
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
            }
            else
            {
                if ( checkBox3.Checked == false )
                {
                    checkBox2.Enabled = true;
                }
                checkBox3.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBox4.Checked = true;
            MessageCloser.TM = textBox8;
            PortScanner.TBL = textBox23;
            PortScanner.TBR = textBox24;
            PortScanner.CB1 = comboBox1;
            PortScanner.CB2 = comboBox3;
            PortScanner.CB3 = comboBox2;
            if (checkBox5.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox7.Enabled = true;
                checkBox6.Enabled = true;
            }
            else
            {
                checkBox4.Checked = true;
                checkBox7.Enabled = false;
                checkBox6.Enabled = false;
            }
            if (checkBox8.Checked)
            {
                textBox9.Enabled = true;
                textBox27.Enabled = true;
            }
            else
            {
                textBox9.Enabled = false;
                textBox27.Enabled = false;
            }
            PortScanner.SimpleIP = checkBox7.Checked;
            PortScanner.bJustLoginned = !checkBox7.Checked;
        }

        public void RNRDYN_OnLoginComplete(object sender, EventArgs e)
        {
            //MessageBox.Show("brftn");
            for (int i = 0; i < 500; i++)//составим массив ссылок на наши RDP модули на форме
            {
                if ( sender.Equals(axMsRdpClient7NotSafeForScripting[i]) )
                {
                    PortScanner.Loginned[i] = true;
                    break;
                }
            }
            //TT = (AxMSTSCLib.AxMsRdpClient7NotSafeForScripting)sender;
            //TT.StartConnected = 1;
            //MessageBox.Show("rrrrr");
            throw new NotImplementedException();
        }

        private void textBox23_TextChanged(object sender, EventArgs e)
        {
            textBox25.Text = textBox23.Lines.Length.ToString();
        }

        private void textBox24_TextChanged(object sender, EventArgs e)
        {
            textBox26.Text = textBox24.Lines.Length.ToString();
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            PortScanner.SimpleIP = checkBox7.Checked;
            PortScanner.bJustLoginned = !checkBox7.Checked;
            checkBox6.Checked = !checkBox7.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            PortScanner.SimpleIP = !checkBox6.Checked;
            PortScanner.bJustLoginned = checkBox6.Checked;
            checkBox7.Checked = !checkBox6.Checked;
        }

        private void WYForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (t != null)
            {
                t.Abort();
            }
            if (h != null)
            {
                h.Abort();
            }
            if (io != null)
            {
                io.Abort();
            }
            if (ioM != null)
            {
                ioM.Abort();
            }
            if (fg != null)
            {
                fg.Abort();
            }
            if (tn != null)
            {
                tn.Abort();
            }
            if (hp != null)
            {
                hp.Abort();
            }
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bEndScan == true )
            {
                string Path0;
                openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog1.ShowDialog();
                Path0 = openFileDialog1.FileName;
                if (Path0 != "" && Path0.Length > 4 && Path0.Substring(Path0.Length - 4, 4) == ".txt")
                {
                    io = new Thread(delegate () //поток для восстановления данных
                    {
                        LoaderSaver.CorrectStart(Path0, ref PortScanner.VariantsB, ref progressBar2, ref textBox10, ref textBox12, ref textBox13, ref textBox23, ref textBox24);//*******
                    });
                    io.Start();
                }

            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                textBox9.Enabled = true;
                textBox27.Enabled = true;
            }
            else
            {
                textBox9.Enabled = false;
                textBox27.Enabled = false;
            }
        }
    }
}
