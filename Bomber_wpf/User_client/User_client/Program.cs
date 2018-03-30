using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.Threading;
using System.Windows.Forms;

namespace User_client
{
    class Program
    {
        static string serverIp = "127.0.0.1";
        static TcpClient server;
        static User myUser;
        static GameBoard gameBoard;
        static bool connected;

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
                    IFormatter formatter = new BinaryFormatter(); 
                    NetworkStream strm = server.GetStream(); 

                    //Информация о игровом мире, полученная с сервера
                    gameBoard = (GameBoard) formatter.Deserialize(strm);
                    myUser = (User)formatter.Deserialize(strm);                   

                    myUser.ACTION = myUser.Play(gameBoard);

                    //Отправка на сервер информацию об игроке, в частности, планируемое действие
                     formatter.Serialize(strm, myUser); 
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    connected = false;
                    server.Close();
                    Application.Exit();
                }
            }
        }
    }
}
