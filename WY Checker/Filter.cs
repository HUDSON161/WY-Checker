using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WY_Checker
{
    public static class Filter
    {
        public static string CleanMask(string Source, string Mask)//очистка строки по маске
        {
            for (int i = 0; i < Source.Length; i++)
            {
                for (int j = 0; j < Mask.Length; j++)
                {
                    if (Source[i] == Mask[j])
                    {
                        break;
                    }
                    if (j == Mask.Length - 1)
                    {
                        Source.Remove(i, 1);
                        i--;
                    }
                }
            }

            return Source;
        }

        public static string RemoveAfterSpace(string Source)//удалить все после пробела включая пробел
        {
            for (int i = 0; i < Source.Length; i++)
            {
                if (Source[i] == ' ')
                {
                    return Source.Remove(i);
                }
            }
            return Source;
        }
        public static string RemoveBeforeSpace(string Source)//удалить все до пробела включая пробел
        {
            int lastspace = -1;
            for (int i = 0; i < Source.Length; i++)
            {
                if (Source[i] == ' ')
                {
                    lastspace = i;
                }
            }
            if (lastspace != -1)
            {
                return Source.Remove(0, lastspace+1);
            }
            return Source;
        }
    }
}
