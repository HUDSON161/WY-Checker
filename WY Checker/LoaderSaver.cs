using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WY_Checker
{
    static class LoaderSaver
    {

        public static void CorrectStart(string Path0, ref long VariantsC,ref ProgressBar Progress, ref TextBox TextBoxC, ref TextBox TextBoxR, ref TextBox TP, ref TextBox TextBoxL, ref TextBox TextBoxP)
        {
            string[] PS=new string[1];
            string Ttext1="";
            string Ttext2="";
            string Ttext3 = "";
            string Ttext4 = "";
            int StartX = 7;
            ReadProgress(Path0, ref PS);

            if ( PS.Length > 5)
            {
                VariantsC = Convert.ToInt32(Filter.CleanMask(PS[4], "0123456789"));
                Progress.Value = Convert.ToInt32(Filter.CleanMask(PS[5], "0123456789"));
                for (int i = StartX; i < PS.Length; i++)
                {
                    if (PS[i] == "***")
                    {
                        for (int j = StartX; j < i; j++)
                        {
                            if (PS[j] != "" && j != i - 1)
                            {
                                Ttext1 += PS[j] + Environment.NewLine;
                            }
                            if (PS[j] != "" && j == i - 1)
                            {
                                Ttext1 += PS[j];
                            }
                        }
                        StartX = i + 1;
                        break;
                    }
                }
                for (int i = StartX; i < PS.Length; i++)
                {
                    if (PS[i] == "***")
                    {
                        for (int j = StartX; j < i; j++)
                        {
                            if (PS[j] != "" && j != i - 1)
                            {
                                Ttext2 += PS[j] + Environment.NewLine;
                            }
                            if (PS[j] != "" && j == i - 1)
                            {
                                Ttext2 += PS[j];
                            }
                        }
                        StartX = i + 1;
                        break;
                    }
                }
                for (int i = StartX; i < PS.Length; i++)
                {
                    if (PS[i] == "***")
                    {
                        for (int j = StartX; j < i; j++)
                        {
                            if (PS[j] != "" && j != i - 1)
                            {
                                Ttext3 += PS[j] + Environment.NewLine;
                            }
                            if (PS[j] != "" && j == i - 1)
                            {
                                Ttext3 += PS[j];
                            }
                        }
                        StartX = i + 1;
                        break;
                    }
                }
                for (int i = StartX; i < PS.Length; i++)
                {
                    for (int j = i ; j < PS.Length ; j++)
                    {
                        if (PS[j] != "" && PS[j] != Environment.NewLine && j != PS.Length - 1)
                        {
                            Ttext4 += PS[j] + Environment.NewLine;
                        }
                        if (PS[j] != "" && PS[j] != Environment.NewLine && j == PS.Length - 1)
                        {
                            Ttext4 += PS[j];
                        }
                    }
                    break;
                }
                TextBoxC.Text = Ttext1;
                TextBoxR.Text = Ttext2;
                TextBoxL.Text = Ttext3;
                TextBoxP.Text = Ttext4;
                TP.Text = PS[0].Replace("$", Environment.NewLine);
                PortScanner.RezTextBuffer = Ttext2;
                //MessageBox.Show("erbet");
            }
        }

        public static void SaveData(ref long VariantsC, ref ProgressBar Progress, ref TextBox TextBoxC,ref TextBox TextBoxR,ref TextBox TP, ref TextBox TextBoxL, ref TextBox TextBoxP)//сохраним все данные
        {
            string[] PS = new string[6 + 1 + TextBoxC.Lines.Length + 1 + TextBoxR.Lines.Length + 1 + TextBoxL.Lines.Length + 1 + TextBoxP.Lines.Length];
            PS[0] = TP.Text.Replace(Environment.NewLine,"$");
            int IndStart;
            for (int i = 1; i < 4; i++)
            {
                PS[i]="^^^^";
            }
            PS[4] = VariantsC.ToString();
            PS[5] = Progress.Value.ToString();

            IndStart = 7;
            PS[IndStart-1] = "***";
            for (int i = 0; i < TextBoxC.Lines.Length; i++)
            {
                PS[i+ IndStart] = TextBoxC.Lines[i].ToString();
            }

            IndStart += TextBoxC.Lines.Length+1;
            PS[IndStart-1] = "***";
            for (int i = 0; i < TextBoxR.Lines.Length; i++)
            {
                PS[i + IndStart] = TextBoxR.Lines[i].ToString();
            }

            IndStart += TextBoxR.Lines.Length+1;
            PS[IndStart-1] = "***";
            for (int i = 0; i < TextBoxL.Lines.Length; i++)
            {
                PS[i + IndStart] = TextBoxL.Lines[i].ToString();
            }

            IndStart += TextBoxL.Lines.Length+1;
            PS[IndStart - 1] = "***";
            for (int i = 0; i < TextBoxP.Lines.Length; i++)
            {
                PS[i + IndStart] = TextBoxP.Lines[i].ToString();
            }
            WriteProgress("Save.txt", ref PS);
        }

        private static void WriteProgress(string path,ref string[] ProgressStrings)//запишем строки в файл
        {
            if (File.Exists(path) != true)
            {  //проверяем есть ли такой файл, если его нет, то создаем
                using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
                {
                    for (int i = 0; i < ProgressStrings.Length; i++)
                    {
                        sw.WriteLine(ProgressStrings[i]);             //пишем строчку, или пишем что хотим
                    }
                }
            }
            else
            {                              //если файл есть, то откываем его и пишем в него 
                using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Truncate, FileAccess.Write)))
                {
                    //(sw.BaseStream).Seek(0, SeekOrigin.End);         //идем в конец файла и пишем строку или пишем то, что хотим
                    for (int i = 0; i < ProgressStrings.Length; i++)
                    {
                        sw.WriteLine(ProgressStrings[i]);             //пишем строчку, или пишем что хотим
                    }
                }
            }
        }

        private static void ReadProgress(string path,ref string[] ProgressStrings)//прочитаем строки из файла
        {
            if (File.Exists(path) == true)
            {  //проверяем есть ли такой файл, если его нет, то создаем
                try
                {                                  //чтение айла
                    ProgressStrings = File.ReadAllLines(path);         //чтение всех строк файла в массив строк
                }
                catch (FileNotFoundException e)
                {

                }
            }
        }



    }
}
