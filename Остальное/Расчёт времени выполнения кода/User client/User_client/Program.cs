using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace User_client
{
    class Program
    {
        static string serverIp = "127.0.0.1";
        static TcpClient server;
        static bool connected;

        static void Main(string[] args)
        {           
            Connect();
           // test();
            CommunicateWithServer();           
        }

        static void Connect()
        {
            try
            {                 
                server = new TcpClient(serverIp, 9595);
                Console.WriteLine("Удалось подключиться к серверу");
                connected = true;              
            }
            catch 
            {
                Thread.Sleep(500);
                Console.WriteLine("Не удалось подключиться к серверу, повторная попытка");
                Connect();
            }
        }


        static void CommunicateWithServer()
        {

            Random rn = new Random();
            int i = 4;

            while (connected)
            {
                try
                {
                    string serverMessage = readStream();

                    if (serverMessage != "next")
                    {
                        continue;
                    }

                    int rnumber = 0;
                    if (i >2)
                    {
                        rnumber = 990;
                    }
                    else
                    {
                        rnumber = 1050;
                    }
                    i--;
                    //     Console.WriteLine(rnumber);
                    //      Thread.Sleep(rnumber);
                   

                    writeStream("start");

                    Thread.Sleep(rnumber);

                    writeStream(rnumber.ToString());
                              

                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    connected = false;
                    server.Close();
               //     Application.Exit();

                }
            }
        }


        static void writeStream(string message)
        {
            NetworkStream stream = server.GetStream();
            Byte[] data = Encoding.Default.GetBytes(message);

            stream.Write(data, 0, data.Length);
        }

        static string readStream()
        {
            NetworkStream stream = server.GetStream();

            Byte[] serverData = new Byte[512];
            Int32 bytes = stream.Read(serverData, 0, serverData.Length);

            string serverMessage = Encoding.Default.GetString(serverData, 0, bytes);
            return serverMessage;
        }
    }
}
