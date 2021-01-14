using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FileClass;
using Microsoft.Win32;

namespace SPOLKS_Lab1
{
    class ClientClass
    {
        public string FilePath { get; set; }
        public string CompName { get; set; }
        private Socket _tcpSocket;
        private Socket _udpSocket;
        private IPEndPoint _tcpEndPoint;
        private IPEndPoint _udpEndPoint;
        private const string _ip = "192.168.31.192";
        private const int _port = 8080;
        private const string _pathTo = "C:\\to\\";
        public ObservableCollection<string> clientList;

        public ClientClass()
        {
            _tcpEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientList = new ObservableCollection<string>();
        }
        public string ConnectToServer(string compName)
        {
            if(compName != null && compName != "")
            {
                CompName = compName;
                var sendClientObject = new { Name = CompName, IP = _ip, Port = _port };
                try
                {
                    _tcpSocket.Connect(_tcpEndPoint);
                    string client = JsonSerializer.Serialize(sendClientObject);
                    SendStringBytes(client);

                    var answer = ReceivingString();
                    if (answer != "" || answer == "UserList")
                    {   
                        var list = ReceivingString();
                        clientList = JsonSerializer.Deserialize<ObservableCollection<string>>(list);
                        return "Connection complite.";
                    }
                    else
                        throw new Exception($"Wrong answer from the server.");
                }catch (Exception ex)
                {
                    return $"Fatal Error! {ex.Message}";
                }
            }
            return "Enter a computer name.";
        }
        public string SendFile(string protocol, string userName)
        {
            switch(protocol)
            {
                case "TCP":
                    {
                        Thread tcpSendThread = new Thread(new ParameterizedThreadStart(TCPSend));
                        tcpSendThread.IsBackground = true;
                        tcpSendThread.Start(userName);
                        break;
                    }
                case "UDP":
                    {
                        break;
                    }
                default:
                        return "Select a type protocol.";
            }
            return "Done!";
        }

        private void TCPSend(object name)
        {
            CustomFile file = new CustomFile();
            string userName = name as string;
            if(userName != null)
            {
                using (FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[stream.Length];
                    var length = BitConverter.GetBytes(stream.Read(data, 0, data.Length));
                    file.FileName = Path.GetFileName(stream.Name);
                    file.Data = data;
                    DataInfo info = new DataInfo
                    {
                        ClientName = this.CompName,
                        FileName = file.FileName,
                        FileSize = BitConverter.ToInt32(length),
                        ProtocolType = "TCP",
                        DestinationName = userName
                    };
                    SendStringBytes("TCPProtocol");

                    byte[] i = info.ToArray();
                    _tcpSocket.Send(i);
                    Thread.Sleep(10);
                    byte[] to = file.ToArray();
                    _tcpSocket.Send(to);
                }
            }
        }

        public void Process()
        {
            while (true)
            {
                var answer = ReceivingString();

                string typeData = "";
                string sizeData = "";
                bool s = false;
                DataInfo info = new DataInfo();


                foreach (var c in answer)
                {
                    if (c == ':')
                        s = true;
                    else if (!s)
                        typeData += c;
                    else if (s)
                        sizeData += c;
                }

                switch (typeData)
                {
                    case "Client":
                        {
                            byte[] buffer = new byte[256];
                            info = ReceivingClientData(ref buffer, int.Parse(sizeData));
                            break;
                        }
                    case "UDPData":
                        {
                            break;
                        }
                    case "TCPData":
                        {
                            TCPReceivingData(info);
                            break;
                        }
                    case "UserList":
                        {
                            var list = ReceivingString();
                            clientList = JsonSerializer.Deserialize<ObservableCollection<string>>(list);
                            break;
                        }
                }
            }

        }
        private void TCPReceivingData(DataInfo info)
        {
            try
            {
                const int bufferSize = 8192;
                CustomFile file = new CustomFile();
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    ReceivingBytes(memStream, buffer, info.FileSize);

                    file = new CustomFile(memStream.ToArray());
                }

                using (FileStream stream = new FileStream(_pathTo + file.FileName, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(file.Data, 0, file.Data.Length);
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"TCP Receiving: {ex.Message}");
            }
            
        }
        private DataInfo ReceivingClientData(ref byte[] buffer, int size)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                ReceivingBytes(memStream,  buffer, size);
                DataInfo clientInfo = new DataInfo(memStream.ToArray());

                string sMessageBoxInfo = "\tMessage from: " + clientInfo.ClientName +
                    "\n\tFile name: " + clientInfo.FileName +
                    "\nt File size: " + clientInfo.FileSize.ToString();
                string sCaption = "Incomming message";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage imgWarnign = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxInfo, sCaption, btnMessageBox, imgWarnign);

                switch(rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        {
                            SendStringBytes("YES");
                            return clientInfo;
                        }
                    case MessageBoxResult.No:
                        {
                            SendStringBytes("NO");
                            return null;
                        }
                }
                return null;
            }
        }
        private void ReceivingBytes(MemoryStream memStream, byte[] buffer, int size)
        {
            int bytes = 0;
            do
            {
                int v = _tcpSocket.Receive(buffer);
                memStream.Write(buffer, 0, v);
                bytes += v;
            } while (size > bytes);
        }
        private void SendStringBytes(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            _tcpSocket.Send(data);
        }
        private string ReceivingString()
        {
            byte[] buffer = new byte[256];
            var size = 0;
            var answer = new StringBuilder();
            do
            {
                size = _tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            } while (_tcpSocket.Available > 0);
            return answer.ToString();
        }
        public bool OpenDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == true)
            {
                FilePath = open.FileName;
                return true;
            }
            return false;
        }
    }
}
