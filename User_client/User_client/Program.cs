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
            try
            {
                gameBoard = new GameBoard();                

                server = new TcpClient(serverIp, 9595);
                connected = true;
                Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Connect()
        {
            while (connected)
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter(); 
                    NetworkStream strm = server.GetStream(); 

                    //Информация о игровом мире, полученная с сервера
                    gameBoard = (GameBoard) formatter.Deserialize(strm);
                    Console.WriteLine(gameBoard.Players[0].Name);
                    myUser = (User)formatter.Deserialize(strm);                   

                    myUser.ACTION = myUser.Play(gameBoard);
                    Console.WriteLine(myUser.Name);

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
