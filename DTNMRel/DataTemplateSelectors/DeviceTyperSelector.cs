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

namespace DTNMRel.DataTemplateSelectors
{
    public class DeviceTypeSelector : DataTemplateSelector
    {
        public DataTemplate ServerTemplate { get; set; }
        public DataTemplate ClientTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is CommunicationDevice dev)
            {
                return dev.CommunicationDeviceType switch
                {
                    CommunicationDeviceType.Server => ServerTemplate,
                    CommunicationDeviceType.Client => ClientTemplate,
                    _ => null
                };
            }
            return null;
        }
    }
}
