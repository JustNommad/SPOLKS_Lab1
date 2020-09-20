using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using FileClass;
using SPOLKS_Lab1;

namespace Server_Lab1
{
    class ClientClass : ServerClass
    {
        const int bufferSize = 8192;
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public CustomFile File { get; set; }
        public Socket Client { get; set; }

        public void Process()
        {
            try
            {
                while (true)
                {
                    var answer = ReceivingString(Client);
                    
                    switch (answer)
                    {
                        case "TCPProtocol":
                            {
                                ReceivingAllData(answer, Client.Available);
                                break;
                            }
                        case "UDPProtocol":
                            {
                                ReceivingAllData(answer, Client.Available);
                                break;
                            }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            #region input file process
            //int bytes = 0;
            //CustomFile file;
            //int size = 0;
            //byte[] b = new byte[256];

            //while (size == bytes)
            //{
            //    Client.Receive(b);
            //    size = BitConverter.ToInt32(b);
            //    Console.WriteLine($"Check: {size} : {b.Length}");
            //}
            //if (size > 0)
            //{
            //    int answer = 202;
            //    Client.Send(BitConverter.GetBytes(answer));
            //}
            //using (MemoryStream memStream = new MemoryStream())
            //{
            //    byte[] buffer = new byte[bufferSize];
            //    ReceivingBytes(memStream, buffer, size);

            //    file = new CustomFile(memStream.ToArray());
            //    size = 0;
            //}
            //Console.WriteLine($"Size of received data: {file.Data.Length.ToString()} bytes.");

            //Client.Send(file.ToArray());

            //Client.Shutdown(SocketShutdown.Both);
            //Client.Close();
            //bytes = 0;
            #endregion
        }

        private void SendTCP(byte[] file, Socket client)
        {
            SendStringBytes(client, "TCPData");
            client.Send(file);
        }
        private void SendUDP()
        {

        }
        private string SendDataInfo(DataInfo info, Socket client)
        {
            string answer = "";

            byte[] dataInfo = info.ToArray();
            string comand = "Client:" + dataInfo.Length.ToString();
            SendStringBytes(client, comand);
            client.Send(dataInfo);

            while (true)
            {
                answer = ReceivingString(client);
                if(answer != "")
                    return answer;
            }
        }
        /*1. После поступления на сервер команды с типом используемого протокола, осуществляется прием размера DataInfo
          2. После приема DataInfo осуществить прием файла от клиента
          3. Отправка запроса на прием пользователю адресату
          4. После полуения положительного ответа осуществить передачу файла по установленному протоколу*/
        private void ReceivingAllData(string protocol, int size)
        {
            while (true)
            {
                DataInfo fileInfo;
                byte[] file;
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    ReceivingBytes(memStream, buffer, size);
                    fileInfo = new DataInfo(memStream.ToArray());
                }
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    ReceivingBytes(memStream, buffer, fileInfo.FileSize);
                    file = memStream.ToArray();
                }
                Socket destination = clientList.Where(x => x.Name == fileInfo.DestinationName) as Socket;
                var choise = SendDataInfo(fileInfo, destination);

                switch (choise)
                {
                    case "YES":
                        {
                            if (protocol == "TCPProtocol")
                                SendTCP(file, destination);
                            else if (protocol == "UDPProtocol")
                                SendUDP();
                            break;
                        }
                    case "NO":
                        {
                            SendStringBytes(Client, "NO");
                            break;
                        }
                }
            }
        }
        private void ReceivingBytes(MemoryStream memStream, byte[] buffer, int size)
        {
            int bytes = 0;
            do
            {
                int v = Client.Receive(buffer);
                memStream.Write(buffer, 0, v);
                bytes += v;
            } while (size > bytes);
        }
        private void SendStringBytes(Socket client, string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            client.Send(data);
        }
        private string ReceivingString(Socket client)
        {
            byte[] buffer = new byte[256];
            var size = 0;
            var answer = new StringBuilder();
            do
            {
                size = client.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            } while (client.Available > 0);
            return answer.ToString();
        }
    }
}
