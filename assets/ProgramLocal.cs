using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

using ClassLibrary_CGC;
using User_class;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;


namespace User_client
{
    class Program
    {
        static string serverIp = "127.0.0.1";
        static int serverPort = 9595;
        static TcpClient server;
        static User myUser;
        static GameBoard gameBoard;
        static bool connected;
        static byte[] data;
        static string message;
        static NetworkStream stream;
        static int TimeLimit = 10000;
        static object obj;


        static void Main(string[] args)
        {
            try
            {
                Connect();

                if (server.Connected)
                {
                    Log("start");
                    CommunicateWithServer();
                }
            }
            catch (Exception e)
            {
                if (server != null)
                {
                    server.Close();
                }
                Log(e.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.ReadKey();
        }

        public static void Log(string message)
        {
            using (StreamWriter sw = new StreamWriter("log.txt", true))
            {
                string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
                time = "[" + time + "] ";
                sw.WriteLine(time + ": " + message);
                Console.WriteLine(time + ": " + message);
            }
        }



        /// <summary>
        /// Подключиться к серверу
        /// </summary>
        static void Connect()
        {
            try
            {
                server = new TcpClient(serverIp, serverPort);
                connected = true;
            }
            catch
            {
                Thread.Sleep(500);
                Log("Не удалось подключиться к серверу, повторная попытка");
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
                    stream = server.GetStream();

                    string gamestring = ReceiveMessage();
                    Log("gamestring length " + gamestring.Length);
                    SendMessage("p");
                    string userstr = ReceiveMessage();
                    Log("userstr length " + userstr.Length);

                    gameBoard = JsonConvert.DeserializeObject<GameBoard>(gamestring);
                    myUser = JsonConvert.DeserializeObject<User>(userstr);

                    SendMessage(gameBoard.Players[0].Name);
                    ReceiveMessage();

                    Thread thr = CreateMyThread();
                    Stopwatch a = new Stopwatch();
                    a.Start();

                    thr.Start();

                    int waittime = 0;
                   
                    while (waittime < TimeLimit)
                    {
                        waittime += 50;
                        Thread.Sleep(50);
                        Log(waittime + " wait time");

                        if (thr.ThreadState == System.Threading.ThreadState.Stopped)
                        {
                            Log("Timeout Stopped " + a.ElapsedMilliseconds);

                            SendMessage(a.ElapsedMilliseconds + "");
                          
                            break;
                        }
                    }

                    Log(waittime + " wait Running; thr.ThreadState = " + thr.ThreadState.ToString());
                    if (thr.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        Log("Timeout Running " + TimeLimit);
                        SendMessage(TimeLimit + "");
                        myUser.ACTION = PlayerAction.Bomb;
                      
                    }
                    a.Stop();
                  

                    ReceiveMessage();
                    SendMessage((int)myUser.ACTION + "");
                    Log("send action: " + (int)myUser.ACTION + "");
                }
                catch (Exception e)
                {
                    Log("ERROR: " + e.Message + " " + e.StackTrace);
                    connected = false;
                    if (server != null)
                        server.Close();
                      Console.ReadKey();
                 //   Environment.Exit(0);
                }
            }
        }



        static Thread CreateMyThread()
        {
            Thread thr = new Thread(() =>
            {              
                try
                {
                    //lock (obj)
                    //{
                      //  myUser.ACTION = PlayerAction.Up;
                        myUser.ACTION = myUser.Play(gameBoard);
                        Console.WriteLine(" inthread");
                    //}
                }
                catch (Exception e)
                {
                    Log("user_error: " + e.Message);
                }   
            });
            return thr;
        }


        public static void streamWrite(Stream strm, string message)
        {
            Log("send length " + message.Length);
            using (StreamWriter sw = new StreamWriter(strm))
            {
                while (!sw.BaseStream.CanRead)
                {
                    Log("wait read");
                }
                sw.Write(message);
                sw.Flush();

            }
        }

        public static string streamRead(Stream strm)
        {
            string message = "";
            using (StreamReader sr = new StreamReader(strm))
            {
                while (!sr.BaseStream.CanRead)
                {
                    Log("wait read");
                }
                message = sr.ReadToEnd();
            }
            Log("read length " + message.Length);
            return message;
        }



        static void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }


        static string ReceiveMessage()
        {
            string message = "";

            byte[] data = new byte[256]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
            message = builder.ToString();          
            return message;
        }


        /// <summary>
        /// Отправить данные на сервер
        /// </summary>
        static void SentInfo()
        {
            string action = EncryptAction(myUser.ACTION);
            writeStream(server.GetStream(), action);
        }




        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        public static string readStream(NetworkStream strm)
        {
            Byte[] serverData = new Byte[16];
            int bytes = strm.Read(serverData, 0, serverData.Length);
            string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
            return serverMessage;
        }

        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        public static string readStream(NetworkStream strm, int bytesLength)
        {
            Byte[] serverData = new Byte[bytesLength];
            int bytes = strm.Read(serverData, 0, serverData.Length);
            string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
            return serverMessage;
        }

        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        public static void writeStream(NetworkStream strm, string message)
        {
            Byte[] data = Encoding.Unicode.GetBytes(message);

            strm.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        public static void writeStream(NetworkStream strm, Byte[] data)
        {
            strm.Write(data, 0, data.Length);
        }



        /// <summary>
        /// Распознать Команду из строки
        /// </summary>
        /// <param name="message">Данная строка</param>
        /// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
        /// <param name="i">Индекс элемента в Списке, который получает команду</param>
        public static PlayerAction DecryptAction(string message)
        {
            PlayerAction pa = new PlayerAction();
            int actionInt = int.Parse(message);
            pa = (PlayerAction)actionInt;
            return pa;
        }

        /// <summary>
        /// Распознать Команду из строки
        /// </summary>
        /// <param name="message">Данная строка</param>
        /// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
        /// <param name="i">Индекс элемента в Списке, который получает команду</param>
        public static string EncryptAction(PlayerAction action)
        {
            int actionInt = (int)action;
            string actionString = actionInt.ToString();
            return actionString;
        }
    }
}

