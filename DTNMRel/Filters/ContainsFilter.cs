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
using ICF = DTNMRel.Filters.ICommunicationFilter;

namespace DTNMRel.Filters
{
    public class ContainsFilter : IOneStringOneBoolFilter
    {
        public ContainsFilter()
        {
            IsEnabled = false;
            Param1 = String.Empty;
            Name = "Contains";
            Param2 = false;
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private string param1;
        public string Param1
        {
            get => param1;
            set
            {
                param1 = value;
                OnPropertyChanged(nameof(Param1));
            }
        }

        // Not
        private bool param2;
        public bool Param2
        {
            get => param2;
            set
            {
                param2 = value;
                OnPropertyChanged(nameof(Param2));
            }
        }

        public byte[] FilterInput(byte[] input, Encoding enc)
        {
            if (!IsEnabled) { return input; }

            byte[] p1 = enc.GetBytes(ICF.ReplaceSpecialCharacters(ICF.ReplaceVariables(Param1)));

            if (Param2)
            {
                return ArrayHelper.IndexOfArray(input, p1) >= 0 ? Array.Empty<byte>() : input;
            }
            else
            {
                return ArrayHelper.IndexOfArray(input, p1) >= 0 ? input : Array.Empty<byte>();
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
