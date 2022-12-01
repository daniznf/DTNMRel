/*
    This file is part of DTNMRel.

    DTNMRel - Daniele's Tools Network Message Relauncher
    Copyright (C) 2022 daniznf

    DTNMRel is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    DTNMRel is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
    
    https://github.com/daniznf/DTNMRel
 */

using System.Text;
using System.Text.RegularExpressions;

namespace DTNMRel.Filters
{
    public static class ArrayHelper
    {
        /// <summary>
        /// Joins two arrays.
        /// </summary>
        /// <param name="arr1">First array that will be put into returned array.</param>
        /// <param name="arr2">Second array that will be put into returned array .</param>
        /// <returns>A new array.</returns>
        public static byte[] JoinArray(byte[] arr1, byte[] arr2)
        {
            int a = arr1.Length;
            int b = arr2.Length;
            byte[] toReturn = new byte[a + b];

            for (int i = 0; i < a; i++)
            {
                toReturn[i] = arr1[i];
            }
            for (int i = 0; i < b; i++)
            {
                toReturn[i + a] = arr2[i];
            }
            return toReturn;
        }

        /// <summary>
        /// Extracts <paramref name="count"/> elements from <paramref name="arr"/> starting from <paramref name="start"/>.
        /// </summary>
        /// <param name="arr">Array from where to extract.</param>
        /// <param name="start">Starting position to extract.</param>
        /// <param name="count">Number of elements to extract.</param>
        /// <returns>A new array.</returns>
        public static byte[] ExtractArray(byte[] arr, int start, int count)
        {
            byte[] toReturn = new byte[count];
            for (int i = 0; i < count; i++)
            {
                toReturn[i] = arr[i + start];
            }
            return toReturn;
        }

        /// <summary>
        /// Extracts elements from <paramref name="arr"/> starting from <paramref name="start"/> to the end.
        /// </summary>
        /// <param name="arr">Array from where to extract.</param>
        /// <param name="start">Starting position to extract.</param>
        /// <returns>A new array.</returns>
        public static byte[] ExtractArray(byte[] arr, int start)
        {
            byte[] toReturn = new byte[arr.Length - start];
            for (int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = arr[i + start];
            }
            return toReturn;
        }

        /// <summary>
        /// Searches for an array into another array.
        /// </summary>
        /// <param name="arr">The array to search into.</param>
        /// <param name="toFind">The array to search for.</param>
        /// <returns>The position of the searched array into the first array,
        /// -1 if not found.</returns>
        public static int IndexOfArray(byte[] arr, byte[] toFind)
        {
            int a = arr.Length;
            int f = toFind.Length;
            if (a == 0 || f == 0) { return -1; }

            if (a >= f)
            {
                for (int i = 0; i <= a - f; i++)
                {
                    byte[] test = ExtractArray(arr, i, f);

                    if (EqualsArray(test, toFind)) { return i; }
                }
            }

            return -1;
        }

        /// <summary>
        /// Compares elements of <paramref name="arr1"/> with <paramref name="arr2"/> using Equals().
        /// </summary>
        /// <param name="arr1">First element to compare.</param>
        /// <param name="arr2">Second element to compare.</param>
        /// <returns>True if arrays contain equal elements.</returns>
        public static bool EqualsArray(byte[] arr1, byte[] arr2)
        {
            int a = arr1.Length;
            int b = arr2.Length;
            if (a == 0 || b == 0) { return false; }
            if (a != b) { return false; }

            for (int i = 0; i < a; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Counts all occurrencies of <paramref name="toFind"/> into <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">The array to search into.</param>
        /// <param name="toFind">The array to find.</param>
        /// <returns>Number of occurrencies of <paramref name="toFind"/>.</returns>
        public static int OccurrenciesOfArray(byte[] arr, byte[] toFind)
        {
            int occurrencies = 0;
            int a = arr.Length;
            int f = toFind.Length;
            if (a == 0 || f == 0) { return -1; }

            if (a >= f)
            {
                for (int i = 0; i < a - f; i++)
                {
                    byte[] test = ExtractArray(arr, i, f);
                    if (EqualsArray(test, toFind))
                    {
                        occurrencies++;
                    }
                }
            }
            return occurrencies;
        }

        /// <summary>
        /// Removes first <paramref name="count"/> elements from <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">Array from which to remove elements.</param>
        /// <param name="count">Number of elements to remove.</param>
        /// <returns>A new array without first <paramref name="count"/> elements.</returns>
        public static byte[] RemoveFirst(byte[] arr, int count)
        {
            int a = arr.Length;
            if (count > a) { return arr; }

            byte[] toReturn = new byte[a - count];
            for (int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = arr[i + count];
            }
            return toReturn;
        }

        /// <summary>
        /// Removes last <paramref name="count"/> elements from <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">Array from which to remove elements.</param>
        /// <param name="count">Number of elements to remove.</param>
        /// <returns>A new array without last <paramref name="count"/> elements.</returns>
        public static byte[] RemoveLast(byte[] arr, int count)
        {
            int a = arr.Length;
            if (count > a) { return arr; }
            byte[] toReturn = new byte[a - count];
            for (int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = arr[i];
            }
            return toReturn;
        }

        /// <summary>
        /// Replaces first occurency of <paramref name="oldArr"/> with <paramref name="newArr"/> in <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">The array that contains <paramref name="oldArr"/>.</param>
        /// <param name="oldArr">The array to be replaced.</param>
        /// <param name="newArr">The array that replaces <paramref name="oldArr"/>.</param>
        /// <returns>A new array.</returns>
        public static byte[] ReplaceFirstArray(byte[] arr, byte[] oldArr, byte[] newArr)
        {
            byte[] toReturn;
            int a = arr.Length;
            int o = oldArr.Length;
            if (a == 0 || o == 0) { return arr; }

            int pos = IndexOfArray(arr, oldArr);
            if (pos >= 0)
            {
                toReturn = JoinArray(ExtractArray(arr, 0, pos), newArr);
                toReturn = JoinArray(toReturn, ExtractArray(arr, pos + o));
                return toReturn;
            }
            return arr;
        }

        /// <summary>
        /// Replaces all occurencies of <paramref name="oldArr"/> with <paramref name="newArr"/> in <paramref name="newArr"/>.
        /// </summary>
        /// <param name="arr">The array that contains <paramref name="oldArr"/>.</param>
        /// <param name="oldArr">The array to be replaced.</param>
        /// <param name="newArr">The array that replaces <paramref name="oldArr"/>.</param>
        /// <returns>A new array.</returns>
        public static byte[] ReplaceAllArrays(byte[] arr, byte[] oldArr, byte[] newArr)
        {
            byte[] toReturn = arr;
            int a = arr.Length;
            int o = oldArr.Length;
            if (a == 0 || o == 0) { return arr; }

            int pos = IndexOfArray(toReturn, oldArr);
            while (pos >= 0)
            {
                toReturn = ReplaceFirstArray(toReturn, oldArr, newArr);
                pos = IndexOfArray(toReturn, oldArr);
            }

            return toReturn;
        }

        /// <summary>
        /// Splits <paramref name="arr"/> at every occurrency of <paramref name="separator"/>.
        /// </summary>
        /// <param name="arr">Array to split.</param>
        /// <param name="separator">Separator to use to split <paramref name="arr"/>.</param>
        /// <returns>An array of arrays.</returns>
        public static byte[][] SplitArray(byte[] arr, byte[] separator)
        {
            int a = arr.Length;
            int s = separator.Length;

            if (a == 0 || s == 0) { return new byte[1][] { arr }; }

            int o = 0;
            int n = OccurrenciesOfArray(arr, separator) + 1;
            byte[][] toReturn = new byte[n][];

            byte[] searchArr = arr;
            int pos = IndexOfArray(arr, separator);
            while (pos >= 0)
            {
                toReturn[o] = ExtractArray(searchArr, 0, pos);
                searchArr = ExtractArray(searchArr, pos + s);
                pos = IndexOfArray(searchArr, separator);
                o++;
            }

            toReturn[n - 1] = searchArr;
            return toReturn;
        }

        /// <summary>
        /// Removes non-characters from the end of <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">Array from which to remove elements.</param>
        /// <returns>A new array without last non-characters.</returns>
        public static byte[] TrimEnd(byte[] arr, Encoding enc)
        {
            int n = 0;

            for (int i = arr.Length-1; i >= 0; i--)
            {
                if (Regex.Match(enc.GetString(arr, i, 1), @"\w").Success)
                {
                    break;
                }
                else
                {
                    n++;
                }
            }

            return RemoveLast(arr, n);
        }

        /// <summary>
        /// Removes non-characters from the beginning of <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">Array from which to remove elements.</param>
        /// <returns>A new array without first non-characters.</returns>
        public static byte[] TrimStart(byte[] arr, Encoding enc)
        {
            int n = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (Regex.Match(enc.GetString(arr, i, 1), @"\w").Success)
                {
                    break;
                }
                else
                {
                    n++;
                }
            }

            return RemoveFirst(arr, n);
        }

        /// <summary>
        /// Removes non-characters from beginning and ending of <paramref name="arr"/>.
        /// </summary>
        /// <param name="arr">Array from which to remove elements.</param>
        /// <returns>A new array without first and last non-characters.</returns>
        public static byte[] Trim(byte[] arr, Encoding enc)
        {
            return TrimEnd(TrimStart(arr, enc), enc);
        }
    }
}
