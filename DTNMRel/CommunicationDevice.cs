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
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DTNMRel
{
    public enum CommunicationProtocol { TCP, UDP }
    public enum CommunicationDeviceType { Server, Client }
    public enum CommunicationDeviceRole { Source, Destination }
    public enum CommunicationDeviceStatus { Default, Waiting, Connected, Receiving, Sending, Error }
    
    public delegate void DataReceivedEventHandler(CommunicationDevice sender, byte[] dataReceived);

    public class CommunicationDevice : INotifyPropertyChanged
    {
        private Socket socket;
        private Socket tmpSocket;
        private bool cancel; 
        private readonly object lockObject;

        public CommunicationDevice(CommunicationDeviceType deviceType, CommunicationDeviceRole deviceRole)
        {
            lockObject = new object();
            Encoding = Encoding.UTF8;
            LocalIPAddress = LocalIPAddresses?[0] ?? IPAddress.Loopback;
            RemoteIPAddress = IPAddress.None;
            CommunicationDeviceType = deviceType;
            CommunicationDeviceRole = deviceRole;
            TestString = String.Empty;
            IsEnabled = false;
            IsCollapsed = true;
            ReceivedString = String.Empty;
            SentString = String.Empty;

            DeviceStatus = CommunicationDeviceStatus.Default;
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

        public static List<CommunicationProtocol> CommunicationProtocols =>
            new(Enum.GetValues<CommunicationProtocol>());

        public static List<CommunicationDeviceType> DeviceTypes =>
            new(Enum.GetValues<CommunicationDeviceType>());

        public CommunicationDeviceType CommunicationDeviceType { get; }

        public CommunicationDeviceRole CommunicationDeviceRole { get; }

        private IPAddress localIPAddress;
        public IPAddress LocalIPAddress
        {
            get => localIPAddress;
            set
            {
                localIPAddress = LocalIPAddresses.Contains(value) ? value : IPAddress.Loopback;
                OnPropertyChanged(nameof(LocalIPAddress));
            }
        }

        private int localPort;
        public int LocalPort
        {
            get => localPort;
            set
            {
                localPort = value;
                OnPropertyChanged(nameof(LocalPort));
            }
        }

        private IPAddress remoteIPAddress;
        public IPAddress RemoteIPAddress
        {
            get => remoteIPAddress;
            set
            {
                remoteIPAddress = value;
                OnPropertyChanged(nameof(RemoteIPAddress));
            }
        }

        private int remotePort;
        public int RemotePort
        {
            get => remotePort;
            set
            {
                remotePort = value;
                OnPropertyChanged(nameof(RemotePort));
            }
        }

        private CommunicationProtocol protocol;
        public CommunicationProtocol Protocol
        {
            get => protocol;
            set
            {
                protocol = value;
                OnPropertyChanged(nameof(Protocol));
            }
        }

        private CommunicationDeviceStatus deviceStatus;
        public CommunicationDeviceStatus DeviceStatus
        {
            get => deviceStatus;
            private set
            {
                lock (lockObject)
                {
                    deviceStatus = value;
                    OnPropertyChanged(nameof(DeviceStatus));
                }
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value && (!isEnabled))
                {
                    Start();
                }
                if ((!value) && isEnabled)
                {
                    Stop();
                }

                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private Encoding encoding;
        public Encoding Encoding
        {
            get => encoding;
            set
            {
                encoding = value;
                OnPropertyChanged(nameof(Encoding));
            }
        }

        public static List<IPAddress> LocalIPAddresses { get; private set; }

        public static List<string> Encodings
        {
            get
            {
                List<string> toReturn = new();
                EncodingInfo[] encs = Encoding.GetEncodings();
                for (int i = 0; i < encs.Length; i++)
                {
                    toReturn.Add(encs[i].GetEncoding().HeaderName);
                }
                return toReturn;
            }
        }

        public event DataReceivedEventHandler DataReceived;

        private byte[] receivedData;
        public byte[] ReceivedData
        {
            get => receivedData;
            private set
            {
                lock (lockObject)
                {
                    receivedData = value;
                    ReceivedString = Encoding.GetString(value);
                    if (DataReceived != null)
                    {
                        new Task(() => { DataReceived.Invoke(this, value); }).Start();
                    }
                }
            }
        }

        private string receivedString;
        public string ReceivedString
        {
            get => receivedString;
            private set
            {
                receivedString = value;
                OnPropertyChanged(nameof(ReceivedString));
            }
        }

        private string sentString;
        public string SentString
        {
            get => sentString;
            private set
            {
                sentString = value;
                OnPropertyChanged(nameof(SentString));
            }
        }

        private bool isCollapsed;
        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                isCollapsed = value;
                OnPropertyChanged(nameof(IsCollapsed));
            }
        }

        public string TestString { get; set; }

        public void Start()
        {
            bool pending;
            cancel = false;
            DeviceStatus = CommunicationDeviceStatus.Default;
            ReceivedData = Array.Empty<byte>();

            if (Protocol == CommunicationProtocol.TCP)
            {
                tmpSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (Protocol == CommunicationProtocol.UDP)
            {
                tmpSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }

            try
            {
                socket = null;
                if (CommunicationDeviceType == CommunicationDeviceType.Server)
                {
                    RemoteIPAddress = IPAddress.None;
                    RemotePort = 0;

                    tmpSocket.Bind(new IPEndPoint(LocalIPAddress, LocalPort));
                    IPEndPoint localEP = (IPEndPoint) tmpSocket.LocalEndPoint;
                    LocalIPAddress = localEP.Address;
                    LocalPort = localEP.Port;
                     
                    if (Protocol == CommunicationProtocol.TCP)
                    {
                        tmpSocket.Listen();
                        SocketAsyncEventArgs acceptEventArgs = new();
                        acceptEventArgs.Completed += AcceptEventArgs_Completed;

                        pending = tmpSocket.AcceptAsync(acceptEventArgs);
                        if (pending) 
                        { 
                            DeviceStatus = CommunicationDeviceStatus.Waiting;
                        }
                        else
                        {
                            CheckStatus();
                            AcceptEventArgs_Completed(tmpSocket, acceptEventArgs);
                        }
                    }
                    else if (Protocol == CommunicationProtocol.UDP)
                    {
                        socket = tmpSocket;

                        StartReceiving();

                        // Connectionless Server will not reply back because a connection does not exist
                        // and the server does not know who to reply to.
                    }
                }
                else //(CommunicationDeviceType == CommunicationDeviceType.Client)
                {
                    tmpSocket.Bind(new IPEndPoint(LocalIPAddress, 0));
                    IPEndPoint localEP = (IPEndPoint)tmpSocket.LocalEndPoint;
                    LocalIPAddress = localEP.Address;
                    LocalPort = localEP.Port;

                    SocketAsyncEventArgs connectedEventArgs = new();
                    connectedEventArgs.RemoteEndPoint = new IPEndPoint(RemoteIPAddress, RemotePort);
                    connectedEventArgs.Completed += ConnectedEventArgs_Completed;
                    
                    // TCP or UDP
                    pending = tmpSocket.ConnectAsync(connectedEventArgs);
                    if (pending)
                    {
                        DeviceStatus = CommunicationDeviceStatus.Waiting;
                    }
                    else
                    {
                        CheckStatus();
                        ConnectedEventArgs_Completed(tmpSocket, connectedEventArgs);
                    }
                }
            }
            catch (Exception E)
            {
                ReceivedString = E.ToString();
                DeviceStatus = CommunicationDeviceStatus.Error;

                Release(tmpSocket);
                Release(socket);
            }
            finally
            {
                // Do not release here.
                // Release(tmpSocket);
            }
        }

        public void Stop()
        {
            cancel = true;
            Release(socket);
            Release(tmpSocket);
            Thread.Sleep(100);
            DeviceStatus = CommunicationDeviceStatus.Default;
        }

        private void AcceptEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    socket = e.AcceptSocket;
                    IPEndPoint localEP = (IPEndPoint)socket.LocalEndPoint;
                    LocalIPAddress = localEP.Address;
                    LocalPort = localEP.Port;
                    IPEndPoint remoteEP = (IPEndPoint)socket.RemoteEndPoint;
                    RemoteIPAddress = remoteEP.Address;
                    RemotePort = remoteEP.Port;

                    if (cancel) { return; }

                    CheckStatus();
                    StartReceiving();
                }
                else
                {
                    throw new Exception(e.SocketError.ToString());
                }
            }
            catch (Exception E)
            {
                ReceivedString = E.ToString();
                DeviceStatus = CommunicationDeviceStatus.Error;
                
                Release(socket);
            }
            finally
            {
                // Release here or will get Exception: Only one usage permitted [...] when starting.
                Release(tmpSocket);
            }
        }

        private void Restart()
        {
            Thread.Sleep(500);
            if (!cancel)
            {
                Start();
            }
        }

        private void StartReceiving()
        { 
            SocketAsyncEventArgs receiveEventArgs = new();
            byte[] buf = new byte[socket.ReceiveBufferSize];
            receiveEventArgs.SetBuffer(buf, 0, buf.Length);
            receiveEventArgs.Completed += Receive_Completed;

            bool pending = socket.ReceiveAsync(receiveEventArgs);
            if (pending)
            {
                CheckStatus();
            }
            else
            {
                throw new Exception(receiveEventArgs.SocketError.ToString());
            }
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {            
            try
            {
                if (cancel) { return; }
                
                DeviceStatus = CommunicationDeviceStatus.Receiving;

                ReceivedData = e.Buffer.Take(e.BytesTransferred).ToArray();

                bool pending = socket.ReceiveAsync(e);  
                if (pending)
                {
                    CheckStatus();
                }
                else
                {
                    throw new Exception(e.SocketError.ToString());
                }
            }
            catch (Exception E)
            {
                ReceivedString = E.ToString();
                DeviceStatus = CommunicationDeviceStatus.Error;
                new Task(Restart).Start();

                Release(socket);
            }
        }

        private void ConnectedEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (cancel) { return; }

                if (e.SocketError == SocketError.Success)
                {
                    socket = e.ConnectSocket;

                    CheckStatus();
                    // Receive replies from server
                    StartReceiving();
                }
                else
                {
                    throw new Exception(e.SocketError.ToString());
                }
            }
            catch (Exception E)
            {
                ReceivedString = E.ToString();
                DeviceStatus = CommunicationDeviceStatus.Error;
                
                Release(socket);
                Release(tmpSocket);

                if (!cancel)
                {
                    new Task(Restart).Start();
                }
            }
            finally
            {
                // Do not release here or will get Disposed Object Exception when sending.
                // Release(tmpSocket);
            }
        }

        public void Send(byte[] buffer)
        {
            try
            {
                if (cancel) { return; }
                
                DeviceStatus = CommunicationDeviceStatus.Sending;

                ReceivedData = Array.Empty<byte>();

                SocketAsyncEventArgs sendEventArgs = new();
                sendEventArgs.Completed += SendEventArgs_Completed;
                sendEventArgs.SetBuffer(buffer, 0, buffer.Length);
                
                bool pending = socket.SendAsync(sendEventArgs);
                if (pending)
                {
                    //CheckStatus();
                }
                else
                {
                    SendEventArgs_Completed(socket, sendEventArgs);
                }
            }
            catch (Exception E)
            {
                ReceivedString = E.ToString();
                DeviceStatus = CommunicationDeviceStatus.Error;
                
                Release(socket);
            }
        }

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            SentString = Encoding.GetString(e.Buffer);
            CheckStatus();
        }

        private void CheckStatus()
        {
            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        DeviceStatus = CommunicationDeviceStatus.Connected;
                    }
                    else
                    {
                        DeviceStatus = CommunicationDeviceStatus.Default;
                    }
                }
                catch
                {
                    // Catches also disposed socket.
                    DeviceStatus = CommunicationDeviceStatus.Error;
                    Release(socket);
                }
            }
            else
            {
                // This happens at the very start.
                DeviceStatus = CommunicationDeviceStatus.Default;
            }
        }

        private void Release(Socket rSocket)
        {
            lock (lockObject)
            {
                try
                {
                    if (rSocket != null)
                    {
                        if (rSocket.Connected)
                        {
                            rSocket.Shutdown(SocketShutdown.Both);
                        }
                        rSocket.Close();
                        rSocket.Dispose();
                    }
                }
                catch (Exception E)
                {
                    ReceivedString = E.ToString();
                    DeviceStatus = CommunicationDeviceStatus.Error;
                }
            }
        }

        public static void RetrieveLocalIPAddresses()
        {
            List<IPAddress> ips = new();

            try
            {
                ips.AddRange(Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork));
            }
            catch
            { }
            ips.Add(IPAddress.Loopback);

            LocalIPAddresses = ips;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
