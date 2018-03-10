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



        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(serverIp);
            server = new TcpListener(ip, 9595);
            server.Start();
            cl = server.AcceptTcpClient();
            Console.WriteLine("server Start");
              readMessage();
           // test();
            Console.ReadKey();
        }



        public static void test()
        {
            byte[] data = new byte[256];

            var str = cl.GetStream();

            int i = str.Read(data, 0, data.Length);

            string result = Encoding.Default.GetString(data);
            Console.WriteLine(result);
        }

        public static void readMessage()
        {
            //      BinaryFormatter fr = new BinaryFormatter();
           // string message = "";

            NetworkStream stream = cl.GetStream();

            string[] res_array = new string[4];


            for (int i = 0; i < res_array.Length; i++)
            {
                WriteStream(stream);


                string client_message = ReadStream(stream);
                if (client_message != "start")
                {
                    Console.WriteLine("Ошибка, сообщение о начале считывания неверно");
                }

                res_array[i] = "";
                Thread thr = new Thread(() =>
                {
                    res_array[i] = ReadStream(stream);
                });

                thr.Start();
                Thread.Sleep(1000);

                if (thr.ThreadState == System.Threading.ThreadState.Running)
                {
                    //thr.Abort();
                    //while (true)
                    //{
                    //    if (thr.ThreadState == System.Threading.ThreadState.Stopped)
                    //    {
                    //        break;
                    //    }
                    //}

                    Console.WriteLine((i + 1) + " - долго");
                }
                else
                {
                    Console.WriteLine((i + 1) + " - быстро " + res_array[i] + ", " + thr.ThreadState.ToString());
                }
            }
        }


        public static string ReadStream(NetworkStream str)
        {
            byte[] data = new byte[512];

            int bytes = str.Read(data, 0, data.Length);

            string result = Encoding.Default.GetString(data, 0, bytes);

            return result;
        }


        public static void WriteStream(NetworkStream str)
        {
            byte[] data = Encoding.Default.GetBytes("next");

            str.Write(data, 0, data.Length);
        }
    }
}
