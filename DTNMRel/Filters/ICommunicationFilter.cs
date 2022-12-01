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

using System.ComponentModel;
using System.Text;

namespace DTNMRel.Filters
{
    public interface ICommunicationFilter : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public static Dictionary<string, string> Variables;

        /// <summary>
        /// Filters input arrays with parameter(s) set in properties.
        /// </summary>
        /// <param name="input">Array to filter.</param>
        /// <param name="enc">Encoding to use to convert parameter(s) into string.</param>
        /// <returns>Filtered array.</returns>
        public byte[] FilterInput(byte[] input, Encoding enc);

        public static List<string> FilterTypes =>
            new()
            {
                nameof(AppendFilter).Replace("Filter", String.Empty),
                nameof(PrependFilter).Replace("Filter", String.Empty),
                nameof(RemoveFirstFilter).Replace("Filter", String.Empty),
                nameof(RemoveLastFilter).Replace("Filter", String.Empty),
                nameof(ReplaceFilter).Replace("Filter", String.Empty),
                nameof(SplitFilter).Replace("Filter", String.Empty),
                nameof(ContainsFilter).Replace("Filter", String.Empty),
                nameof(TrimFilter).Replace("Filter", String.Empty),
                nameof(TrimStartFilter).Replace("Filter", String.Empty),
                nameof(TrimEndFilter).Replace("Filter", String.Empty),
                nameof(DelayFilter).Replace("Filter", String.Empty),
                nameof(StartOverFilter).Replace("Filter", String.Empty),
            };

        // Special characters should be replaced in Param1 or Param2, because
        // they were born as strings and user writes \n to obtain newline.
        // Instead, received data should only be filtered with regular filters,
        // or be relaunched untouched.
        /// <summary>
        /// Replaces special characters (E.g.: \t) if not escaped (E.g.: \\t).
        /// </summary>
        /// <param name="input">String to parse.</param>
        /// <returns>New string with all special characters replaced.</returns>
        public static string ReplaceSpecialCharacters(string input)
        {
            if (input == null) { return String.Empty; }

            string test;
            string toReturn = String.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                test = input[i].ToString();
                if (i < input.Length - 1)
                {
                    test += input[i + 1].ToString();
                }

                switch (test)
                {
                    case "\\\\":
                        toReturn += "\\";
                        i++;
                        break;
                    case "\\a":
                        toReturn += "\a";
                        i++;
                        break;
                    case "\\b":
                        toReturn += "\b";
                        i++;
                        break;
                    case "\\f":
                        toReturn += "\f";
                        i++;
                        break;
                    case "\\n":
                        toReturn += "\n";
                        i++;
                        break;
                    case "\\r":
                        toReturn += "\r";
                        i++;
                        break;
                    case "\\t":
                        toReturn += "\t";
                        i++;
                        break;
                    case "\\v":
                        toReturn += "\v";
                        i++;
                        break;
                    default:
                        toReturn += input[i];
                        break;
                }
            }
            return toReturn;
        }

        // Variables are replaced in Param1 or Param2, not in received data.
        /// <summary>
        /// Replaces all variables written as $Variable from 
        /// static <paramref name="Variables"/> dictionary.
        /// </summary>
        /// <param name="input">String into which replace variables.</param>
        /// <returns>New string with variables replaced.</returns>
        public static string ReplaceVariables(string input)
        {
            int pos, subPos;
            string toReturn = input;
            KeyValuePair<string, string>[] arrVariables = Variables.ToArray();
            
            // input == "" or input == "$"
            if (input.Length <= 1) { return toReturn; }

            string test, iVariable, subVar;
            pos = input.IndexOf('$');
            if (pos == -1) { return toReturn; }

            if ((pos == 0) || (input[pos - 1] != '\\'))
            {
                // Test all variables to find if one matches.
                for (int i = 0; i < arrVariables.Length; i++)
                {
                    iVariable = arrVariables[i].Key;

                    // If input is shorter than iVariable, e.g.: $Split_1 < Contains_1
                    if (input.Length < (pos + 1 + iVariable.Length)) { continue; }
                    test = input.Substring(pos + 1, iVariable.Length);

                    // Split_1.2
                    subPos = pos + 1 + test.Length;
                    if (input.Length > subPos)
                    {
                        if (input[subPos] == '.')
                        {
                            subVar = String.Empty;
                            for (int j = subPos + 1; j < input.Length; j++)
                            {
                                // .123 or .other
                                if (Int32.TryParse(input[j].ToString(), out int outInt))
                                {
                                    subVar += input[j];
                                }
                                else { break; }
                            }
                            if (subVar.Length > 0)
                            {
                                test += "." + subVar;
                            }
                        }
                    }

                    if (iVariable == test)
                    {
                        toReturn = toReturn.Substring(0, pos) + arrVariables[i].Value + toReturn.Substring(pos + test.Length + 1);

                        return ReplaceVariables(toReturn);
                    }
                }
            }

            return toReturn;
        }

        public void OnPropertyChanged(string name);
    }
}
