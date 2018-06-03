using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SimonRolfe.Common
{
    public static class EnumExtensions
    {
        /// <summary> 
        /// Get the description attribute for an enumerated type.
        /// </summary> 
        /// <param name="e">The enum to retrieve the decription for.</param> 
        /// <returns>A description for the enumerated type.</returns> 
        /// <remarks>Used by the Tasks system.</remarks>
        public static string Description(this Enum e)
        {
            var da = (DescriptionAttribute[])(e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false));

            return da.Length > 0 ? da[0].Description : e.ToString();
        }

        public static T Parse<T>(string s) where T : IComparable, IFormattable, IConvertible /* as much of a constraint as we can apply */
        {
            return (T)System.Enum.Parse(typeof(T), s);
        }
    }

    public static class StringExtensions
    {
        public static string ToInitialCase(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }
            char[] a = source.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            for (int x = 1; x < a.Length; x++)
            {
                a[x] = char.ToLower(a[x]);
            }
            return new string(a);
        }
    }

    public static class NumberExtensions
    {
        public static int Clamp(this int source, int min, int max)
        {
            if (source < min)
                return min;
            if (source > max)
                return max;

            return source;
        }

        public static long Clamp(this long source, long min, long max)
        {
            if (source < min)
                return min;
            if (source > max)
                return max;

            return source;
        }

        public static float Clamp(this float source, float min, float max)
        {
            if (source < min)
                return min;
            if (source > max)
                return max;

            return source;
        }

        public static double Clamp(this double source, double min, double max)
        {
            if (source < min)
                return min;
            if (source > max)
                return max;

            return source;
        }

        public static decimal Clamp(this decimal source, decimal min, decimal max)
        {
            if (source < min)
                return min;
            if (source > max)
                return max;

            return source;
        }

        public static bool IsWhole(this float source)
        {
            return (source != Math.Floor(source));
        }

        public static bool IsWhole(this double source)
        {
            return (source != Math.Floor(source));
        }

        public static bool IsWhole(this decimal source)
        {
            return (source != Math.Floor(source));
        }
    }

    public class Common
    {
        /// <summary>
        /// Turns a DataTable into a CSV format for output
        /// </summary>
        /// <param name="table">The DataTable to parse and output.</param>
        /// <param name="IncludeHeader">Include a header row with field names (default true)?</param>
        /// <param name="HeaderDelimiter">The string delimiter to use for headers (default none).</param>
        /// <param name="Separator">The separator character for the file (default comma).</param>
        /// <param name="BodyDelimiter">The string delimiter to use for the body of the file (default double quote).</param>
        /// <returns>A CSV-formatted string containing the data from the DataTable.</returns> 
        public static string DataTableToCSV(System.Data.DataTable table, bool IncludeHeader = true, string HeaderDelimiter = "", string Separator = ",", string BodyDelimiter = "\"")
        {
            System.Text.StringBuilder sbCSV = new System.Text.StringBuilder();

            if (IncludeHeader)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    sbCSV.Append(HeaderDelimiter).Append(col.ColumnName.Replace(HeaderDelimiter, HeaderDelimiter + HeaderDelimiter)).Append(HeaderDelimiter).Append(Separator);
                }
                sbCSV.Length = sbCSV.Length - Separator.Length;
                sbCSV.AppendLine();
            }
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    sbCSV.Append(BodyDelimiter + (item ?? "").ToString().Replace(BodyDelimiter, BodyDelimiter + BodyDelimiter)).Append(BodyDelimiter).Append(Separator);
                }
                sbCSV.Length = sbCSV.Length - Separator.Length;
                sbCSV.AppendLine();
            }
            return sbCSV.ToString();
        }
    }
}
