using FileClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Server_Lab1
{
    class ServerClass
    {
        protected List<ClientClass> clientList;
        private const string _ip = "192.168.31.163";
        private const int _port = 8080;

        private IPEndPoint _tcpEndPoint;
        private Socket _tcpSocket;

        public ServerClass()
        {
            clientList = new List<ClientClass>();
            _tcpEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Initialiaze()
        {
            try
            {
                _tcpSocket.Bind(_tcpEndPoint);
                _tcpSocket.Listen(10);
                Console.WriteLine("Server is running...");
                while (true)
                {
                    Socket listener = _tcpSocket.Accept();
                    int index = 0;
                    if (!CheckClientIP(listener, ref index))
                    {
                        index = AddNewClient(listener);
                        SendUserList();
                    }
                    Thread clientThread = new Thread(new ThreadStart(clientList[index].Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server: {ex.Message}");
                Console.ReadLine();
            }
        }

        private bool CheckClientIP(Socket client, ref int index)
        {
            if (clientList == null)
                return false;
            var address = (client.RemoteEndPoint as IPEndPoint).Address.ToString();
            foreach (ClientClass c in clientList)
            {
                if (c.IP.Contains(address))
                {
                    index = clientList.IndexOf(c);
                    return true;
                }
            }
            return false;
        }
        private int AddNewClient(Socket listener)
        {
            var size = 0;
            byte[] buffer = new byte[256];
            var data = new StringBuilder();
            do
            {
                size = listener.Receive(buffer);
                data.Append(Encoding.UTF8.GetString(buffer, 0, size));
            } while (listener.Available > 0);

            if (data.ToString() != "")
            {
                var client = JsonSerializer.Deserialize<ClientClass>(data.ToString());
                foreach (var n in clientList)
                {
                    if (n.Name.Contains(client.Name))
                        client.Name += "(1)";
                }
                client.Client = listener;
                clientList.Add(client);
                return clientList.IndexOf(client);
            }
            return 0;
        }
        private void SendUserList()
        {
            string[] list = new string[clientList.Count];
            for (int i = 0; i < clientList.Count; i++)
                list[i] = clientList[i].Name;

            var code = Encoding.UTF8.GetBytes("UserList");
            foreach (ClientClass c in clientList)
            {
                c.Client.Send(code);

                var json = JsonSerializer.Serialize(list);
                var data = Encoding.UTF8.GetBytes(json);
                c.Client.Send(data);
            }
        }
    }
}
