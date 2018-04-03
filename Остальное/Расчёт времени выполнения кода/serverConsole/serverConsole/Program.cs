using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace serverConsole
{
    class Program
    {
        static string serverIp = "127.0.0.1";
        static TcpListener server;
        static TcpClient cl;
        static int totaltimeout = 0;
        static int maxtimeout = 9000;


        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(serverIp);
            server = new TcpListener(ip, 9595);
            Console.WriteLine("server Start");
            server.Start();
            cl = server.AcceptTcpClient();
            Console.WriteLine("client add");
            for (int i = 0; i < 10; i++)
            {
                readMessage();
            }

            Console.WriteLine("Затраченное время: " + totaltimeout);
            if (totaltimeout>maxtimeout)
            {
                Console.WriteLine("Превышен общий лимит времени");
            }
           
           // test();
            Console.ReadKey();
        }




        public static void readMessage()
        {
            //      BinaryFormatter fr = new BinaryFormatter();
            // string message = "";

            NetworkStream stream = cl.GetStream();

            WriteStream(stream);


            string client_message = ReadStream(stream);
          
            if (client_message != "s")
            {
                Console.WriteLine("Ошибка, сообщение о начале считывания неверно");
            }

            client_message = ReadStream(stream);

            Console.WriteLine("Время задержки клиента: " + client_message);
            totaltimeout += int.Parse(client_message);
        }


        public static string ReadStream(NetworkStream str)
        {
            string result = "";
            try
            {
                byte[] data = new byte[512];

                int bytes = str.Read(data, 0, data.Length);

                result = Encoding.Default.GetString(data, 0, bytes);
            }
            catch
            {
                Console.WriteLine("ERROR: stream read");
            }

            return result;

        }


        public static void WriteStream(NetworkStream str)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes("next");

                str.Write(data, 0, data.Length);
            }
            catch
            {
                Console.WriteLine("ERROR: stream write");
            }
        }
    }
}
