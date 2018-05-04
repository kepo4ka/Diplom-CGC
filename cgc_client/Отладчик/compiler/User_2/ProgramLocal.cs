using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

using ClassLibrary_CGC;
using User_class;
using Newtonsoft.Json;



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
        static NetworkStream stream;


        static void Main(string[] args)
        {
            try
            {
                Connect();

                if (server.Connected)
                {
                    Log("Connected to Server");
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

                //  Environment.Exit(0);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// ФУНКЦИЯ, КОТОРАЯ ИСПОЛЬЗУЕТСЯ ДЛЯ ОТЛАДКИ
        /// </summary>
        public static void DEBUGMYCODE()
        {
            myUser.ACTION = myUser.Play(gameBoard);
        }



        /// <summary>
        /// Записать сообщение в Лог
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void Log(string message)
        {
            if (myUser == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(myUser.Name))
            {
                return;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\compiler\\" +  myUser.Name + "\\log.txt", true))
                {
                    string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
                    time = "[" + time + "] ";
                    sw.WriteLine(time + ": " + message);
                    Console.WriteLine(time + ": " + message);
                }
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
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
                    int length = int.Parse(ReceiveMessage());

                    SendMessage("p");

                    string gamestring = "";

                    while (gamestring.Length < length)
                    {
                        gamestring += ReceiveMessage();
                    }

                    gamestring = DecompressString(gamestring);					
                    SendMessage("p");

                    string userstr = ReceiveMessage();
                    userstr = DecompressString(userstr);

                    gameBoard = JsonConvert.DeserializeObject<GameBoard>(gamestring);
                    myUser = JsonConvert.DeserializeObject<User>(userstr);


                    try
                    {
                        DEBUGMYCODE();
                    }
                    catch (Exception er)
                    {
                        Log("STRATEGY ERROR: " + er.Message);
                        myUser.ACTION = PlayerAction.Wait;
                    }

                    SendMessage((int)myUser.ACTION + "");

                    Log("My ACTION: " + myUser.ACTION);
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




        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        static void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Получить сообщение
        /// </summary>
        /// <returns>Полученное сообщение</returns>
        static string ReceiveMessage()
        {
            string message = "";

            byte[] data = new byte[256];
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
        /// Из числа в Команду
        /// </summary>
        /// <param name="message">Строка</param>
        /// <returns>Команда</returns>
        public static PlayerAction DecryptAction(string message)
        {
            PlayerAction pa = new PlayerAction();
            int actionInt = int.Parse(message);
            pa = (PlayerAction)actionInt;
            return pa;
        }

        /// <summary>
        /// Из Команды в Число
        /// </summary>
        /// <param name="action">Команда</param>
        /// <returns>Число</returns>
        public static string EncryptAction(PlayerAction action)
        {
            int actionInt = (int)action;
            string actionString = actionInt.ToString();
            return actionString;
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }


        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }




    }
}

