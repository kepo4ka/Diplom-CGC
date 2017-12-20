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
            gameBoard = new GameBoard();
            myUser = new User();
            myUser.Name = "user1";
            myUser.ID = 1111;
            myUser.Color = Color.Blue;

            server = new TcpClient(serverIp, 9595);
            connected = true;
           

        }

        static void Connect(User user)
        {
            while (connected)
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter(); // the formatter that will serialize my object on my stream 
                    NetworkStream strm = server.GetStream(); // the stream 

                    gameBoard = (GameBoard) formatter.Deserialize(strm);

                    user.ACTION = user.Play();
                    formatter.Serialize(strm, user); // the serialization process 
                }
                catch (Exception e)
                {
                    Console.Write("ERROR: " + e.Message);
                    connected = false;
                }
            }
        }
    }
}
