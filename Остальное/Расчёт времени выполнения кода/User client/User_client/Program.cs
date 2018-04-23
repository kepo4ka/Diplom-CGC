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
using System.Diagnostics;

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
            while (connected)
            {
                try
                {                   
                    string serverMessage = readStream();
                    Console.WriteLine(serverMessage);

                    if (serverMessage != "n")
                    {
                        continue;
                    }


                    //     Console.WriteLine(rnumber);
                    //      Thread.Sleep(rnumber);

                    int timelimit = 1000;
               
                    Thread thr = new Thread(() =>
                    {
                        Stopwatch a = new Stopwatch();

                        a.Start();
                        generatePause();
                        a.Stop();

                        long sleeptime = a.ElapsedMilliseconds;
                        Console.WriteLine(sleeptime);
                        if (sleeptime > timelimit)
                        {
                            sleeptime = timelimit;
                        }
                        writeStream(sleeptime.ToString());
                    });

                    writeStream("s");
                    thr.Start();
                    Thread.Sleep(timelimit);

                    if (thr.ThreadState == System.Threading.ThreadState.Running)
                    {
                        thr.Abort();
                        writeStream(timelimit.ToString());                        
                        continue;
                    }

                //    writeStream(sleeptime.ToString());

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

        static int generatePause()
        {
            Random rn = new Random();
            int sleeptime= rn.Next(100, 1500);
            Thread.Sleep(sleeptime);            
            return sleeptime;
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

            Byte[] serverData = new Byte[2];
            Int32 bytes = stream.Read(serverData, 0, serverData.Length);

            string serverMessage = Encoding.Default.GetString(serverData, 0, bytes);           
            return serverMessage;
        }
    }
}
