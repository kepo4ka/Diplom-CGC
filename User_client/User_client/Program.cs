using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using ClassLibrary_CGC;
using User_class;
using System.Threading;


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
            Console.WriteLine("test1");
            Connect();
            Console.WriteLine("test2");

            CommunicateWithServer();
           
        }

        static void Connect()
        {
            try
            {               
                //  gameBoard = new GameBoard();                

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
                }
            }
        }
    }
}
