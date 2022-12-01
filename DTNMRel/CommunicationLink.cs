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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using DTNMRel.Filters;

namespace DTNMRel
{
    public enum CommunicationLinkStatus { Default, On, Waiting, Working, Error }
    public class CommunicationLink : INotifyPropertyChanged
    {
        private readonly object lockObject;
        public CommunicationLink()
        {
            lockObject = new object();
            CommunicationDevice.RetrieveLocalIPAddresses();
            Sources = new();
            Sources.CollectionChanged += Sources_CollectionChanged;
            Destinations = new();
            Filters = new();
            Filters.CollectionChanged += Filters_CollectionChanged;
            ICommunicationFilter.Variables = new();
            TestEncoding = Encoding.UTF8;

            LinkStatus = CommunicationLinkStatus.Default;
        }

        public string Name { get; set; }

        bool isEnabled;
        public bool IsEnabled 
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));

                LinkStatus = (value) ? CommunicationLinkStatus.On : CommunicationLinkStatus.Default;
            }
        }

        public CommunicationLinkStatus linkStatus;
        public CommunicationLinkStatus LinkStatus 
        {
            get => linkStatus;
            set
            {
                linkStatus = value;
                OnPropertyChanged(nameof(LinkStatus));
            }
        }
        
        public ObservableCollection<CommunicationDevice> Sources { get; }
        public ObservableCollection<CommunicationDevice> Destinations { get; }
        public ObservableCollection<ICommunicationFilter> Filters { get; }
        
        private Encoding testEncoding;
        public Encoding TestEncoding
        {
            get => testEncoding;
            set
            {
                testEncoding = value;
                OnPropertyChanged(nameof(TestEncoding));
            }
        }

        private string testString;
        public string TestString
        {
            get => testString;
            set
            {
                testString = value;
                OnPropertyChanged(nameof(TestString));

                if (value != null)
                {
                    TestFilteredString = TestEncoding.GetString(
                        FilterInput(TestEncoding.GetBytes(
                            value), TestEncoding));
                }
            }
        }

        private string testFilteredString;
        public string TestFilteredString
        {
            get => testFilteredString;
            set
            {
                testFilteredString = value;
                OnPropertyChanged(nameof(TestFilteredString));
            }
        }

        private void Sources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    if (e.NewItems[i] is CommunicationDevice dev)
                    {
                        // All (and only) Sources must invoke DataReceived.
                        dev.DataReceived += Dev_DataReceived;
                    }
                }
            }
        }

        private void Dev_DataReceived(CommunicationDevice sender, byte[] dataReceived)
        {
            if (IsEnabled)
            {
                lock (lockObject)
                {
                    byte[] data;
                    CommunicationDevice dest;
                    
                    LinkStatus = CommunicationLinkStatus.Working;

                    if (dataReceived.Length > 0)
                    {
                        for (int i = 0; i < Destinations.Count; i++)
                        {
                            dest = Destinations[i];
                            if (dest.IsEnabled)
                            {
                                data = sender.Encoding != dest.Encoding ? 
                                    Encoding.Convert(sender.Encoding, dest.Encoding, dataReceived) : 
                                    dataReceived;

                                data = FilterInput(data, Destinations[i].Encoding);
                                dest.Send(data);
                            }
                        }
                    }
                    if (IsEnabled) { LinkStatus = CommunicationLinkStatus.On; }
                    else { LinkStatus = CommunicationLinkStatus.Default; }
                }
            }
        }

        private void Filters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    // This event is fired (with e.Action == Add) even when reordering CollectionView's items.
                    // Freshly added filters do not have '_' in name.
                    if ((e.NewItems[i] is ICommunicationFilter filter) && (!filter.Name.Contains('_')))
                    {
                        filter.Name = GetUniqueFilterName(filter.Name);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    if (e.OldItems[i] is ICommunicationFilter filter)
                    {
                        ICommunicationFilter.Variables.Remove(filter.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Applies all filters present in <paramref name="Filters"/> list.
        /// </summary>
        /// <param name="input">Data to filter.</param>
        /// <param name="enc">Encoding to use.</param>
        /// <returns>Filtered data.</returns>
        public byte[] FilterInput(byte[] input, Encoding enc)
        {
            ICommunicationFilter filter;
            byte[] toReturn = input;

            ICommunicationFilter.Variables.Clear();
            ICommunicationFilter.Variables["Input"] = enc.GetString(input);
            
            for (int i = 0; i < Filters.Count; i++)
            {
                filter = Filters[i];
                
                toReturn = filter.FilterInput(toReturn, enc);
                ICommunicationFilter.Variables[filter.Name] = enc.GetString(toReturn);

                if (filter is SplitFilter split)
                {
                    for (int j = 0; j < split.AdditionalOutputs.Length; j++)
                    {
                        ICommunicationFilter.Variables[filter.Name + "." + (j + 1)] = enc.GetString(split.AdditionalOutputs[j]);
                    }
                }
            }
            return toReturn;
        }
        
        private string GetUniqueFilterName(string search)
        {
            // A filter with this name has already been added to collection.
            string filterName;
            int num = 0;
            for (int i = 0; i < Filters.Count; i++)
            {
                filterName = Filters[i].Name;
                if (filterName == search)
                {
                    int pos = filterName.LastIndexOf('_');
                    if (pos > 0)
                    {
                        // Filter_5 or My_filter or My_Filter_1
                        string strNum = filterName.Substring(pos + 1);
                        if (int.TryParse(strNum, out num))
                        {
                            filterName = filterName.Substring(0, pos);
                        }
                    }
                    num++;
                    return GetUniqueFilterName(filterName + "_" + num);
                }
            }
            return search;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
