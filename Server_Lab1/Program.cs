using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using FileClass;

namespace Server_Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerClass server = new ServerClass();
            server.Initialiaze();
        }
    }
}
