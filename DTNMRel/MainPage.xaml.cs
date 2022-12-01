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

using System.Net;
using System.Text;
using DTNMRel.Filters;

namespace DTNMRel;

public partial class MainPage : ContentPage
{
	public CommunicationLink CommLink { get; }
    
	public MainPage()
	{
        InitializeComponent();
        LblVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        
        CommLink = new();
        this.BindingContext = CommLink;
        LoadCommLink();
        LoadFilters();
        LoadDevices(CommunicationDeviceRole.Source);
        LoadDevices(CommunicationDeviceRole.Destination);

        if (CommLink.Sources.Count == 0)
        {
            CommLink.Sources.Add(new CommunicationDevice(CommunicationDeviceType.Server, CommunicationDeviceRole.Source)
            {
                Name = "Adam Source Device",
                LocalIPAddress = CommunicationDevice.LocalIPAddresses[0],
                LocalPort = 12345,
                Protocol = CommunicationProtocol.TCP
            });
        }
        if (CommLink.Destinations.Count == 0)
        {
            CommLink.Destinations.Add(new CommunicationDevice(CommunicationDeviceType.Client, CommunicationDeviceRole.Destination)
            {
                Name = "Davis Destination Device",
                LocalIPAddress = CommunicationDevice.LocalIPAddresses[0],
                RemotePort = 12345,
                Protocol = CommunicationProtocol.TCP
            });
        }
        if (CommLink.Filters.Count == 0)
        {
            CommLink.Filters.Add(new DelayFilter()
            {
                Param1 = 1000,
                IsEnabled = true
            });

            CommLink.Filters.Add(new AppendFilter()
            {
                Param1 = @"\r\n"
            });
        }

        PkrSourceDevices.SelectedIndex = 0;
        PkrDestinationDevices.SelectedIndex = 1;
        PkrFilter.SelectedIndex = 0;
        PkrTestEncoding.SelectedItem = Encoding.UTF8.HeaderName;
    }

    #region Devices
    #region Sources
    private void BtnAddSource_Pressed(object sender, EventArgs e)
    {
        if (PkrSourceDevices.SelectedItem != null)
        {
            switch ((CommunicationDeviceType)PkrSourceDevices.SelectedItem)
            {
                case (CommunicationDeviceType.Server):
                    CommLink.Sources.Add(
                        new CommunicationDevice(CommunicationDeviceType.Server, CommunicationDeviceRole.Source));
                    break;
                case (CommunicationDeviceType.Client):
                    CommLink.Sources.Add(
                        new CommunicationDevice(CommunicationDeviceType.Client, CommunicationDeviceRole.Source));
                    break;
                default:
                    return;
            }
            SaveDevices(CommunicationDeviceRole.Source);
        }
    }

    private void BtnRemoveSource_Pressed(object sender, EventArgs e)
    {
        if (LstSources.SelectedItem is CommunicationDevice dev)
        {
            CommLink.Sources.Remove(dev);
            SaveDevices(CommunicationDeviceRole.Source);
        }
    }
    #endregion

    #region Destinations
    private void BtnAddDestination_Pressed(object sender, EventArgs e)
    {
        if (PkrDestinationDevices.SelectedItem != null)
        {
            switch ((CommunicationDeviceType)PkrDestinationDevices.SelectedItem)
            {
                case (CommunicationDeviceType.Server):
                    CommLink.Destinations.Add(
                        new CommunicationDevice(CommunicationDeviceType.Server, CommunicationDeviceRole.Destination));
                    break;
                case (CommunicationDeviceType.Client):
                    CommLink.Destinations.Add(
                        new CommunicationDevice(CommunicationDeviceType.Client, CommunicationDeviceRole.Destination));
                    break;
                default:
                    return;
            }
            SaveDevices(CommunicationDeviceRole.Destination);
        }
    }

    private void BtnRemoveDestination_Pressed(object sender, EventArgs e)
    {
        if (LstDestinations.SelectedItem is CommunicationDevice dev)
        {
            CommLink.Destinations.Remove(dev);
            SaveDevices(CommunicationDeviceRole.Destination);
        }
    }

    private void BtnTestString_Pressed(object sender, EventArgs e)
    {
        if (sender is Button but)
        {
            if (but.BindingContext is CommunicationDevice cDev)
            {
                string toSendSendReplaced = ICommunicationFilter.ReplaceSpecialCharacters(cDev.TestString);
                cDev.Send(cDev.Encoding.GetBytes(toSendSendReplaced));
            }
        }
    }
    #endregion

    #region LoadSave
    private void SaveDevices(CommunicationDeviceRole devRole)
    {
        int count;

        count = Preferences.Get($"{devRole}_Count", 0);
        for (int i = 0; i < count; i++)
        {
            Preferences.Remove($"{devRole}_{i}_Name");
            Preferences.Remove($"{devRole}_{i}_IsCollapsed");
            Preferences.Remove($"{devRole}_{i}_CommunicationDeviceType");
            Preferences.Remove($"{devRole}_{i}_Encoding");
            Preferences.Remove($"{devRole}_{i}_LocalIPAddress");
            Preferences.Remove($"{devRole}_{i}_Port");
            Preferences.Remove($"{devRole}_{i}_Protocol");
            Preferences.Remove($"{devRole}_{i}_RemoteIPAddress");
            Preferences.Remove($"{devRole}_{i}_RemotePort");
        }

        count = devRole == CommunicationDeviceRole.Source ? CommLink.Sources.Count : CommLink.Destinations.Count;

        Preferences.Set($"{devRole}_Count", count);
        for (int i = 0; i < count; i++)
        {
            CommunicationDevice dev = devRole == CommunicationDeviceRole.Source ? CommLink.Sources[i] : CommLink.Destinations[i];

            Preferences.Set($"{devRole}_{i}_Name", dev.Name);
            Preferences.Set($"{devRole}_{i}_IsCollapsed", dev.IsCollapsed);
            Preferences.Set($"{devRole}_{i}_CommunicationDeviceType", dev.CommunicationDeviceType.ToString());
            Preferences.Set($"{devRole}_{i}_Encoding", dev.Encoding.HeaderName);
            Preferences.Set($"{devRole}_{i}_LocalIPAddress", dev.LocalIPAddress?.ToString());
            Preferences.Set($"{devRole}_{i}_Port", dev.LocalPort);
            Preferences.Set($"{devRole}_{i}_Protocol", dev.Protocol.ToString());
            Preferences.Set($"{devRole}_{i}_RemoteIPAddress", dev.RemoteIPAddress?.ToString());
            Preferences.Set($"{devRole}_{i}_RemotePort", dev.RemotePort);
        }
    }

    private void LoadDevices(CommunicationDeviceRole devRole)
    {
        if (devRole == CommunicationDeviceRole.Source)
        {
            CommLink.Sources.Clear();
        }
        else
        {
            CommLink.Destinations.Clear();
        }

        int count = Preferences.Get($"{devRole}_Count", 0);
        for (int i = 0; i < count; i++)
        {
            try
            {
                string devName = Preferences.Get($"{devRole}_{i}_Name", String.Empty);
                bool isCollapsed= Preferences.Get($"{devRole}_{i}_IsCollapsed", true);
                Enum.TryParse<CommunicationDeviceType>(Preferences.Get($"{devRole}_{i}_CommunicationDeviceType", String.Empty),
                    out CommunicationDeviceType devType);
                Encoding enc = Encoding.GetEncoding(Preferences.Get($"{devRole}_{i}_Encoding", "utf-8"));
                IPAddress.TryParse(Preferences.Get($"{devRole}_{i}_LocalIPAddress", String.Empty), out IPAddress localIPAddress);
                int localPort = Preferences.Get($"{devRole}_{i}_Port", 0);
                Enum.TryParse<CommunicationProtocol>(Preferences.Get($"{devRole}_{i}_Protocol", String.Empty),
                    out CommunicationProtocol devProtocol);
                IPAddress.TryParse(Preferences.Get($"{devRole}_{i}_RemoteIPAddress", String.Empty), out IPAddress remoteIPAddress);
                int remotePort = Preferences.Get($"{devRole}_{i}_RemotePort", 0);

                if (devRole == CommunicationDeviceRole.Source)
                {
                    CommLink.Sources.Add(new CommunicationDevice(devType, devRole)
                    {
                        Name = devName,
                        IsCollapsed = isCollapsed,
                        Encoding = enc,
                        LocalIPAddress = localIPAddress,
                        LocalPort = localPort,
                        Protocol = devProtocol,
                        RemoteIPAddress = remoteIPAddress,
                        RemotePort = remotePort
                    });
                }
                else
                {
                    CommLink.Destinations.Add(new CommunicationDevice(devType, devRole)
                    {
                        Name = devName,
                        IsCollapsed = isCollapsed,
                        Encoding = enc,
                        LocalIPAddress = localIPAddress,
                        LocalPort = localPort,
                        Protocol = devProtocol,
                        RemoteIPAddress = remoteIPAddress,
                        RemotePort = remotePort
                    });
                }
            }
            catch { }
        }
    }

    private void LytDevice_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is StackLayout stack)
        {
            if (stack.BindingContext is CommunicationDevice dev)
            {
                SaveDevices(dev.CommunicationDeviceRole);
            }
        }
    }
    #endregion

    private void EntRemoteAddress_Unfocused(object sender, FocusEventArgs e)
    {
        // workaround for binding being too aggressive when editing Entry
        if (sender is Entry entry)
        {
            if (entry.BindingContext is CommunicationDevice cDev)
            {
                // binding mode OneWay does not update Entry
                if (IPAddress.TryParse(entry.Text, out IPAddress addr))
                {
                    cDev.RemoteIPAddress = addr;
                }
                entry.Text = cDev.RemoteIPAddress?.ToString();
            }
        }
    }

    private void BtnMinimizeDevice_Clicked(object sender, EventArgs e)
    {
        if (sender is Button but)
        {
            if (but.BindingContext is CommunicationDevice dev)
            {
                dev.IsCollapsed = ! dev.IsCollapsed;
            }
        }
    }
    #endregion

    #region Filters
    private void BtnAddFilter_Pressed(object sender, EventArgs e)
    {
        if (PkrFilter.SelectedItem != null)
        {
            string selected = PkrFilter.SelectedItem + "Filter";
            CommLink.Filters.Add( selected switch
            {
                nameof(AppendFilter) => new AppendFilter(),
                nameof(PrependFilter) => new PrependFilter(),
                nameof(RemoveFirstFilter) => new RemoveFirstFilter(),
                nameof(RemoveLastFilter) => new RemoveLastFilter(),
                nameof(ReplaceFilter) => new ReplaceFilter(),
                nameof(SplitFilter) => new SplitFilter(),
                nameof(TrimFilter) => new TrimFilter(),
                nameof(TrimStartFilter) => new TrimStartFilter(),
                nameof(TrimEndFilter) => new TrimEndFilter(),
                nameof(ContainsFilter) => new ContainsFilter(),
                nameof(DelayFilter) => new DelayFilter(),
                nameof(StartOverFilter) => new StartOverFilter(),
                _ => new AppendFilter()
            });
            SaveFilters();
        }
    }

    private void BtnRemoveFilter_Pressed(object sender, EventArgs e)
    {
        if (CvwFilters.SelectedItem is ICommunicationFilter fil)
        {
            CommLink.Filters.Remove(fil);
            EntTestString_TextChanged(sender, null);
            SaveFilters();
        }
    }

    private void RefreshTestFilteredString()
    {
        CommLink.TestString = CommLink.TestString;
    }

    private void EntTestString_TextChanged(object sender, TextChangedEventArgs e) => RefreshTestFilteredString();

    private void StpParam_ValueChanged(object sender, ValueChangedEventArgs e) => RefreshTestFilteredString();

    private void SwitchFilter_Toggled(object sender, ToggledEventArgs e) => RefreshTestFilteredString();

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e) => RefreshTestFilteredString();

    private void CvwFilters_Unfocused(object sender, FocusEventArgs e) => SaveFilters();

    private void SaveFilters()
    {
        int count;

        count = Preferences.Get("Filter_Count", 0);
        for (int i = 0; i < count; i++)
        {
            Preferences.Remove($"Filter_{i}_Type");
            Preferences.Remove($"Filter_{i}_Name");
            Preferences.Remove($"Filter_{i}_IsEnabled");
            Preferences.Remove($"Filter_{i}_Param1");
            Preferences.Remove($"Filter_{i}_Param2");
        }

        count = CommLink.Filters.Count;
        Preferences.Set($"Filter_Count", count);
        for (int i = 0; i < count; i++)
        {
            ICommunicationFilter filt = CommLink.Filters[i];
            Preferences.Set($"Filter_{i}_Type", filt.GetType().Name);
            Preferences.Set($"Filter_{i}_Name", filt.Name);
            Preferences.Set($"Filter_{i}_IsEnabled", filt.IsEnabled);

            switch (filt)
            {
                case IOneStringFilter f:
                    Preferences.Set($"Filter_{i}_Param1", f.Param1);
                    break;
                case IOneIntFilter f:
                    Preferences.Set($"Filter_{i}_Param1", f.Param1);
                    break;
                case ITwoStringFilter f:
                    Preferences.Set($"Filter_{i}_Param1", f.Param1);
                    Preferences.Set($"Filter_{i}_Param2", f.Param2);
                    break;
                case IOneStringOneIntFilter f:
                    Preferences.Set($"Filter_{i}_Param1", f.Param1);
                    Preferences.Set($"Filter_{i}_Param2", f.Param2);
                    break;
                case IOneStringOneBoolFilter f:
                    Preferences.Set($"Filter_{i}_Param1", f.Param1);
                    Preferences.Set($"Filter_{i}_Param2", f.Param2);
                    break;
                default:
                    break;
            }
        }
    }

    private void LoadFilters()
    {
        int count;

        count = Preferences.Get("Filter_Count", 0);
        for (int i = 0; i < count; i++)
        {
            ICommunicationFilter filt;
            string filtType = Preferences.Get($"Filter_{i}_Type", String.Empty);

            filt = filtType switch
            {
                nameof(AppendFilter) => new AppendFilter(),
                nameof(PrependFilter) => new PrependFilter(),
                nameof(RemoveFirstFilter) => new RemoveFirstFilter(),
                nameof(RemoveLastFilter) => new RemoveLastFilter(),
                nameof(ReplaceFilter) => new ReplaceFilter(),
                nameof(SplitFilter) => new SplitFilter(),
                nameof(TrimFilter) => new TrimFilter(),
                nameof(TrimStartFilter) => new TrimStartFilter(),
                nameof(TrimEndFilter) => new TrimEndFilter(),
                nameof(ContainsFilter) => new ContainsFilter(),
                nameof(DelayFilter) => new DelayFilter(),
                nameof(StartOverFilter) => new StartOverFilter(),
                _ => new AppendFilter()
            };

            filt.Name = Preferences.Get($"Filter_{i}_Name", String.Empty);
            filt.IsEnabled = Preferences.Get($"Filter_{i}_IsEnabled", false);

            switch (filt)
            {
                case IOneStringFilter f:
                    f.Param1 = Preferences.Get($"Filter_{i}_Param1", String.Empty);
                    break;
                case IOneIntFilter f:
                    f.Param1 = Preferences.Get($"Filter_{i}_Param1", 0);
                    break;
                case ITwoStringFilter f:
                    f.Param1 = Preferences.Get($"Filter_{i}_Param1", String.Empty);
                    f.Param2 = Preferences.Get($"Filter_{i}_Param2", String.Empty);
                    break;
                case IOneStringOneIntFilter f:
                    f.Param1 = Preferences.Get($"Filter_{i}_Param1", String.Empty);
                    f.Param2 = Preferences.Get($"Filter_{i}_Param2", 0);
                    break;
                case IOneStringOneBoolFilter f:
                    f.Param1 = Preferences.Get($"Filter_{i}_Param1", String.Empty);
                    f.Param2 = Preferences.Get($"Filter_{i}_Param2", false);
                    break;
                default:
                    break;
            }

            if (filt != null)
            {
                CommLink.Filters.Add(filt);
            }
        }
    }

    private void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        SaveCommLink();
    }

    private void SaveCommLink()
    {
        Preferences.Set("CommLink_1", CommLink.Name);
    }

    private void LoadCommLink()
    {
        CommLink.Name = Preferences.Get("CommLink_1", String.Empty);
    }
    #endregion
}

