using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;


namespace User_client
{
    class Program
    {
        static string serverIp = "10.0.2.2";
        static int serverPort;
        static TcpClient server;
      
        static NetworkStream stream;
        static bool connected;
        static object obj = new object();

        static User myUser;
        static GameBoard gameBoard;
        static int TimeLimit = 1000;
        static long sleeptime;
        static bool sended;

        static string logpath = "/cgc/log.txt";
        //static string gameboardjsonpath = "/cgc/gameboard.json";
        //static string userjsonpath = "/cgc/user.json";


        static void Main(string[] args)
        {
            try
            {
                if (!int.TryParse(args[0], out serverPort))
                {
                    throw new Exception("Command parametrs (args) ERROR");
                }

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

                Environment.Exit(0);
            }
            //Console.ReadKey();
        }

        public static void Log(string message)
        {
            using (StreamWriter sw = new StreamWriter("/cgc/log.txt", true))
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
                    string gamestring = "";
                    string gamestrLength = ReceiveMessage();

                    while (gamestring.Length < int.Parse(gamestrLength))
                    {
                        gamestring += ReceiveMessage();
                        Log("recieved gamestring: " + gamestring.Length);
                    }

                    SendMessage("p");
                    Log("send p");
                    string userstr = ReceiveMessage();
                   Log("recieved userstr: " + userstr.Length);

                    gameBoard = JsonConvert.DeserializeObject<GameBoard>(gamestring);
                    myUser = JsonConvert.DeserializeObject<User>(userstr);

                    SendMessage(myUser.Name);
                    Log("send name : " + myUser.Name);
                   string temp = ReceiveMessage();
                    Log("recieve temp: " + temp);

                    myUser.ACTION = myUser.Play(gameBoard);

                    SendMessage((int)myUser.ACTION + "");
                    Log("send action: " + (int)myUser.ACTION + "");
                }
                catch (Exception e)
                {
                    Log("ERROR: " + e.Message + " " + e.StackTrace);
                    connected = false;
                    if (server != null)
                        server.Close();
                    //  Console.ReadKey();
                    Environment.Exit(0);
                }
            }
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
