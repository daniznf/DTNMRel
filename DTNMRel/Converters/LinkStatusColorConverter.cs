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

using System.Globalization;

namespace DTNMRel.Converters
{
    public class LinkStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CommunicationLinkStatus stat)
            {
                return stat switch
                {
                    CommunicationLinkStatus.Default=> Colors.Gray,
                    CommunicationLinkStatus.On => Colors.Green,
                    CommunicationLinkStatus.Error => Colors.Red,
                    CommunicationLinkStatus.Waiting => Colors.Yellow,
                    CommunicationLinkStatus.Working => Colors.Wheat,
                    _ => Colors.Black
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
