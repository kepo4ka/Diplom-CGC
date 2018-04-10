using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace User_client
{
    class Program
    {
        static string serverIp = "127.0.0.1";
        static TcpClient server;
        static User myUser;
        static GameBoard gameBoard;
        static bool connected;
        static byte[] data;
        static string message;
        static NetworkStream strm;
        static int TimeLimit = 1000;
        static bool sleeptimeSended;
        static object obj = new object();


        static void Main(string[] args)
        {           
            Connect();         
            CommunicateWithServer();           
        }

        /// <summary>
        /// Подключиться к серверу
        /// </summary>
        static void Connect()
        {
            try
            {        
                server = new TcpClient(serverIp, 9595);
                connected = true;              
            }
            catch 
            {
                Thread.Sleep(500);
                Console.WriteLine("Не удалось подключиться к серверу, повторная попытка");
                Connect();
            }
        }

        /// <summary>
        /// Общение с сервером
        /// </summary>
        static void CommunicateWithServer()
        {
            while (connected)
            {
                try
                {
                    strm = server.GetStream();

                    if (readStream() != "n")
                    {
                        continue;
                    }

                    GetInfo();

                    message = "";
                    sleeptimeSended = false;

                    Thread thr = CreateMyThread();
                    writeStream("s");
                    thr.Start();
                    Thread.Sleep(TimeLimit);

                    if (thr.ThreadState == System.Threading.ThreadState.Running)
                    {
                        thr.Abort();
                        if (sleeptimeSended==false)
                        {
                            lock (obj)
                            {
                                sleeptimeSended = true;
                            }
                            writeStream(TimeLimit.ToString());
                        }
                        continue;
                    }               
                    
                    SentInfo();
                    writeStream("e");             
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    connected = false;
                    server.Close();
                    Environment.Exit(0);
                }
            }
        }     

        static Thread CreateMyThread()
        {
            Thread thr = new Thread(() =>
            {
                Stopwatch a = new Stopwatch();

                a.Start();

                // Получить Команду, которую хочет выполнить Игровая Стратегия
                myUser.ACTION = myUser.Play(gameBoard);

                a.Stop();

                long sleeptime = a.ElapsedMilliseconds;

                Console.WriteLine("Strategy work Time: " + sleeptime);

                if (sleeptime > TimeLimit)
                {
                    sleeptime = TimeLimit;
                }

                if (sleeptimeSended == false)
                {
                    lock (obj)
                    {
                        sleeptimeSended = true;
                    }
                    writeStream(sleeptime.ToString());
                }
            });
            return thr;
        }


        
        /// <summary>
        /// Получить данные от сервера
        /// </summary>
        static void GetInfo()
        {
            IFormatter formatter = new BinaryFormatter();           
            gameBoard = (GameBoard)formatter.Deserialize(strm);
            myUser = (User)formatter.Deserialize(strm);
        }  


        /// <summary>
        /// Отправить данные на сервер
        /// </summary>
        static void SentInfo()
        {
            switch (myUser.ACTION)
            {
                case PlayerAction.Wait:
                    message = "0";
                    break;
                case PlayerAction.Bomb:
                    message = "1";
                    break;
                case PlayerAction.Down:
                    message = "2";
                    break;
                case PlayerAction.Left:
                    message = "3";
                    break;
                case PlayerAction.Right:
                    message = "4";
                    break;
                case PlayerAction.Up:
                    message = "5";
                    break;
            }
            message = "action " + message;
            writeStream(message);
        }

        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        static string readStream()
        {
            Byte[] serverData = new Byte[2];
            int bytes = strm.Read(serverData, 0, serverData.Length);

            string serverMessage = Encoding.Default.GetString(serverData, 0, bytes);
            return serverMessage;
        }


        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        static void writeStream(string message)
        {
            Byte[] data = Encoding.ASCII.GetBytes(message);

            strm.Write(data, 0, data.Length);
        }
    }
}
