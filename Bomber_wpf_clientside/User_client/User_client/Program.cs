using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.Threading;
using System.Text;

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
                    message = "";
                    data = new byte[4];
                    strm = server.GetStream();

                    GetInfo();

                    // Получить Команду, которую хочет выполнить Игровая Стратегия
                    myUser.ACTION = myUser.Play(gameBoard);
                    
                    SentInfo();                   
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

            data = Encoding.Unicode.GetBytes(message);            
            strm.Write(data, 0, data.Length);
        }
    }
}
