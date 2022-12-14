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
    public class TrimEndFilter : INoParamFilter
    {
        public TrimEndFilter()
        {
            IsEnabled = false;
            Name = "TrimEnd";
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

        public byte[] FilterInput(byte[] input, Encoding enc)
        {
            if (!IsEnabled) { return input; }

            return ArrayHelper.TrimEnd(input, enc);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
