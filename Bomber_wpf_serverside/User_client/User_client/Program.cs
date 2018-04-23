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
      //  static string serverIp = "127.0.0.1";
        static int serverPort;
        static TcpClient server;
        static StreamWriter sw;
        static NetworkStream strm;
        static bool connected;
        static object obj = new object();

        static User myUser;
        static GameBoard gameBoard;           
        static int TimeLimit = 1000;
        static long sleeptime;
        static bool sended;

        static string logpath = "/cgc/log.txt";
        static string gameboardjsonpath = "/cgc/gameboard.json";
        static string userjsonpath = "/cgc/user.json";

       // static string logpath = "log.txt";
       // static string gameboardjsonpath = "gameboard.json";
       // static string userjsonpath = "user.json";



        static void Main(string[] args)
        {
            try
            {
                if (!int.TryParse(args[0], out serverPort))
                {
                    throw new Exception("Command parametrs (args) ERROR");
                }
                sw = new StreamWriter(logpath, true);
                sw.AutoFlush = true;

                Connect();
                connected = true;
                Log("start: " + serverIp + ":" + serverPort);
                CommunicateWithServer();
            }
            catch (Exception e)
            {
                server.Close();
                Log(e.Message);                            
                sw.Close();
                Environment.Exit(0);
            }
        }




        static void Log(string message)
        {
            string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
            time = "[" + time + "] ";
            sw.WriteLine(time + ": " + message);
           // Console.WriteLine(time + ": " + message);
        }


        /// <summary>
        /// Подключиться к серверу
       /// </summary>
        static void Connect()
        {
            //if (!File.Exists(gameboardjsonpath))
            //{
            //    Thread.Sleep(500);
            //    Connect();
            //}
            //else
            //{
            //    connected = true; 
            //    Log("Connected with server");
            //}


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
                    GetInfo();

                    sended = false;
                    Thread thr = CreateMyThread();
                    thr.Start();
                    int waittime = 0;

                    while (waittime < TimeLimit)
                    {
                        waittime += 5;
                        Thread.Sleep(5);

                        if (thr.ThreadState == System.Threading.ThreadState.Stopped)
                        {
                            SentInfo(sleeptime);
                            Log("Strategy work Time: " + sleeptime);
                            break;                      
                        }                      
                    }

                    if (thr.ThreadState == System.Threading.ThreadState.Running)
                    {
                        SentInfo(TimeLimit);
                        Log("Strategy work Time: " + TimeLimit);
                    }

                    //strm = server.GetStream();
                    //string temp = readStream();
                    //Log("lENGHT: " + temp.Length);                   
                    //gameBoard = JsonConvert.DeserializeObject<GameBoard>(temp);
                    //Log("Bang_radius: " + gameBoard.XYinfo[0,5].Player.Bang_radius);
                    //writeStream("client");

                    //string message = JsonConvert.SerializeObject(player);
                    //writeStream(message);


                    //strm = server.GetStream();

                    //string temp = readStream();
                    //if (temp != "1")
                    //{
                    //    Log("readstream: " + temp);
                    //    continue;
                    //}           

                    //    GetInfo();


                    //message = "";
                    //sleeptimeSended = false;

                    //Thread thr = CreateMyThread();
                    //writeStream("1");
                    //thr.Start();
                    //Thread.Sleep(TimeLimit);

                    //if (thr.ThreadState == System.Threading.ThreadState.Running)
                    //{
                    //    thr.Abort();
                    //    if (sleeptimeSended==false)
                    //    {
                    //        lock (obj)
                    //        {
                    //            sleeptimeSended = true;
                    //        }
                    //        writeStream(TimeLimit.ToString());
                    //    }
                    //    continue;
                    //}               

                    //SentInfo();
                    ////writeStream("e");             
                }
                catch (Exception e)
                {
                    Log("ERROR: " + e.GetType() + " : " + e.Message);
                    connected = false;      
                    throw new Exception("exit");                   
                }
            }
        }



        static Thread CreateMyThread()
        {
            Thread thr = new Thread(() =>
            {
                Stopwatch a = new Stopwatch();
                a.Start();
                try
                {
                    myUser.ACTION = myUser.Play(gameBoard);
                }
                catch (Exception e)
                {
                Log("user_error: " + e.Message);
                }       
                a.Stop();

                sleeptime = a.ElapsedMilliseconds;
                if (sleeptime > TimeLimit)
                {
                    sleeptime = TimeLimit;
                }     
            });
            return thr;
        }


        
        /// <summary>
        /// Получить данные от сервера
        /// </summary>
        static void GetInfo()
        {
            strm = server.GetStream();           
            string message = readStream();           

            if (message !="s")
            {              
                throw new Exception("Потеря связи с сервером");
            }

            //IFormatter formatter = new BinaryFormatter();
            ////   gameBoard = (GameBoard)formatter.Deserialize(strm);
            ////  myUser = (User)formatter.Deserialize(strm);
            //int rn = (int)formatter.Deserialize(strm);

            //Log("desiarelize: " + rn);           

            try
            {
                using (StreamReader sr = new StreamReader(gameboardjsonpath))
                {
                    string gameboardjson = sr.ReadToEnd();
                    gameBoard = JsonConvert.DeserializeObject<GameBoard>(gameboardjson);                  
                }

                using (StreamReader sr = new StreamReader(userjsonpath))
                {
                    string userjson = sr.ReadToEnd();
                    myUser = JsonConvert.DeserializeObject<User>(userjson);                    
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }  


        static string ActionToString()
        {            
            switch (myUser.ACTION)
            {                
                case PlayerAction.Bomb:
                    return "1";                    
                case PlayerAction.Down:
                   return "2";
                   
                case PlayerAction.Left:
                    return "3";
                    
                case PlayerAction.Right:
                    return "4";
                   
                case PlayerAction.Up:
                    return "5";                  
                default:
                   return "0";                    
            }           
        }




        /// <summary>
        /// Отправить данные на сервер
        /// </summary>
        static void SentInfo(long limit)
        {
            writeStream(limit + "");
            if (readStream() == "p")
            {
                writeStream(ActionToString());
            }
            else
            {
                throw new Exception("Неверное промежуточное значение");
            }
        }

        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        static string readStream()
        {
            string serverMessage = "";
            Byte[] serverData = new Byte[16];
            if (strm.CanRead)
            {
                int bytes = strm.Read(serverData, 0, serverData.Length);
                serverMessage = Encoding.Unicode.GetString(serverData, 0, bytes);
            }
            return serverMessage;
        }


        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name = "message" > Отправляемая строка</param>
        static void writeStream(string message)
        {
            Byte[] data = Encoding.Unicode.GetBytes(message);
            strm.Write(data, 0, data.Length);
        }
    }
}
