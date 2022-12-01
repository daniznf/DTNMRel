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

using DTNMRel.Filters;

namespace DTNMRel.DataTemplateSelectors
{
    public class FilterSelector : DataTemplateSelector
    {
        public DataTemplate NoParamTemplate { get; set; }
        public DataTemplate OneStringTemplate { get; set; }
        public DataTemplate OneIntTemplate { get; set; }
        public DataTemplate TwoStringTemplate { get; set; }
        public DataTemplate OneStringOneIntTemplate { get; set; }
        public DataTemplate OneStringOneBoolTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                INoParamFilter => NoParamTemplate,
                IOneStringFilter => OneStringTemplate,
                IOneIntFilter => OneIntTemplate,
                ITwoStringFilter => TwoStringTemplate,
                IOneStringOneIntFilter => OneStringOneIntTemplate,
                IOneStringOneBoolFilter => OneStringOneBoolTemplate,
                _ => null
            };
        }
    }
}
