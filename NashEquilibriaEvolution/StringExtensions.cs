using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NashEquilibriaEvolution
{
    public static class StringExtensions
    {
        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static double ToDouble(this string s)
        {
            return double.Parse(s);
        }
    }
}
