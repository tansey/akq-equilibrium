using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeducEvolution
{
    public static class CollectionExtensions
    {
        public static void Normalize(this double[] vector)
        {
            double sum = vector.Sum();
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= sum;
        }

        public static string Flatten<T>(this IEnumerable<T> list, string separator = " ")
        {
            if (list.Count() == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            int last = list.Count() - 1;
            int cur = 0;
            foreach (var element in list)
            {
                sb.Append(element.ToString());
                if (cur == last)
                    break;
                sb.Append(separator);
                cur++;
            }
            return sb.ToString();
        }

        public static double Product(this IEnumerable<double> list)
        {
            double result = 1;
            foreach (var d in list)
                result *= d;
            return result;
        }
    }
}
