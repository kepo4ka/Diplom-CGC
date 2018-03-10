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
                    NetworkStream stream = server.GetStream();
               

                    Thread.Sleep(2000);

                  //  formatter.Serialize(stream, res);


                    Byte[] data = Encoding.Unicode.GetBytes("123");

                    stream.Write(data, 0, data.Length);

                //    stream.WriteTimeout = 1000; //  <------- 1 second timeout
                //    stream.ReadTimeout = 1000; //  <------- 1 second timeout
                    stream.Write(data, 0, data.Length);

                    //   gameBoard = (GameBoard) formatter.Deserialize(strm);

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
