using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.Threading;
using System.Windows.Forms;
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

                    ResetPlayerId(ref gameBoard);
                    
                    myUser = (User)formatter.Deserialize(strm);

                    string message = "";
                    byte[] data = Encoding.ASCII.GetBytes("s");

                    strm.Write(data, 0, data.Length);

                    //   myUser.ACTION = myUser.Play(gameBoard);                        

                    switch(myUser.Play(gameBoard))
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

                    data = Encoding.ASCII.GetBytes(message);

                    strm.Write(data, 0, data.Length);

                    //Отправка на сервер информацию об игроке, в частности, планируемое действие
                    // formatter.Serialize(strm, myUser); 
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


        /// <summary>
        /// Очистить ID игроков
        /// </summary>
        /// <param name="gb"></param>
        static void ResetPlayerId(ref GameBoard gb)
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                gb.Players[i].ID = "";
            }
        }
    }
}
