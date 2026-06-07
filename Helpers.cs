using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKSim
{
    public class Helpers
    {
        public static void Print<T>(T parameter)
        {
            Console.WriteLine(parameter.ToString());
        }

        /// <summary>
        /// Print a one-dimensional array in a rectangular (rows/columns) form to the console.
        /// If the caller does not provide a column count, an attempt is made to print as a square
        /// when the length is a perfect square; otherwise the array is printed as a single row.
        /// </summary>
        public static void PrintSquare<T>(T[] arr)
        {
            if (arr == null)
            {
                Console.WriteLine("(null)");
                return;
            }

            int len = arr.Length;
            if (len == 0)
            {
                Console.WriteLine("(empty)");
                return;
            }

            int cols = (int)MathF.Sqrt(len);
            if (cols * cols != len)
            {
                // fallback to single row when not perfect square
                cols = len;
            }

            PrintSquare(arr, cols);
        }

        /// <summary>
        /// Print a one-dimensional array in rows having the specified number of columns.
        /// The last row may be shorter if the array length is not a multiple of columns.
        /// </summary>
        public static void PrintSquare<T>(T[] arr, int columns)
        {
            if (arr == null)
            {
                Console.WriteLine("(null)");
                return;
            }

            if (columns <= 0)
                columns = arr.Length == 0 ? 1 : arr.Length;

            int rows = (arr.Length + columns - 1) / columns;
            for (int r = 0; r < rows; r++)
            {
                int start = r * columns;
                int end = Math.Min(start + columns, arr.Length);
                var sb = new StringBuilder();
                sb.Append("[ ");
                for (int i = start; i < end; i++)
                {
                    // format floats/doubles with higher readability
                    if (arr[i] is float f)
                        sb.Append(f.ToString("0.0000"));
                    else if (arr[i] is double d)
                        sb.Append(d.ToString("0.0000"));
                    else
                        sb.Append(arr[i]);

                    if (i < end - 1)
                        sb.Append(", ");
                }
                sb.Append(" ]");
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Return the rectangular representation of the array as a single string instead of printing it.
        /// Columns behavior matches the void overload: if columns <= 0 the array length is used.
        /// </summary>
        public static string PrintSquare<T>(T[] arr, bool asString)
        {
            if (arr == null)
                return "(null)";

            int columns = (int)MathF.Sqrt(arr.Length);
            if (columns * columns != arr.Length)
                columns = arr.Length == 0 ? 1 : arr.Length;

            var sbTotal = new StringBuilder();
            int rows = (arr.Length + columns - 1) / columns;
            for (int r = 0; r < rows; r++)
            {
                int start = r * columns;
                int end = Math.Min(start + columns, arr.Length);
                var sb = new StringBuilder();
                sb.Append("[ ");
                for (int i = start; i < end; i++)
                {
                    if (arr[i] is float f)
                        sb.Append(f.ToString("0.0000"));
                    else if (arr[i] is double d)
                        sb.Append(d.ToString("0.0000"));
                    else
                        sb.Append(arr[i]);

                    if (i < end - 1)
                        sb.Append(", ");
                }
                sb.Append(" ]");
                sbTotal.AppendLine(sb.ToString());
            }

            return sbTotal.ToString();
        }
    }
}
