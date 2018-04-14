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
      //  static string serverIp = "10.0.2.2";
      //  static string serverIp = "127.0.0.1";
       // static TcpClient server;
        static User myUser;
        static GameBoard gameBoard;
        static bool connected; 
   
        static int TimeLimit = 1000;
        static long sleeptime;
        static object obj = new object();
        static StreamWriter sw;
        static int GameTimer;

        //static string gameboardjsonpath = "/cgc/gameboard.json";
        //static string actionjsonpath = "/cgc/action.txt";
        //static string logpath = "/cgc/log.txt";
      //  static string userjsonpath = "/cgc/user.json";

        static string gameboardjsonpath = "gameboard.json";
        static string actionjsonpath = "action.txt";
        static string logpath = "log.txt";
        static string userjsonpath = "user.json";



        static void Main(string[] args)
        {
            try
            {
                sw = new StreamWriter(logpath, true);
                sw.AutoFlush = true;

                Connect();    
                           
                CommunicateWithServer();
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Process.Start("notepad");
                Log("ERROR IN MAIN: " + e.Message);
                Console.ReadKey();
            }
        }

        static void Log(string message)
        {  
            string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
            time = "[" + time + "] ";
            sw.WriteLine(time + ": " + message);
            Console.WriteLine(time + ": " + message);
        }


        /// <summary>
        /// Подключиться к серверу
       /// </summary>
        static void Connect()
        {
            if (!File.Exists(gameboardjsonpath))
            {
                Thread.Sleep(500);
                Connect();
            }
            else
            {
                connected = true; 
                Log("Connected with server");
            }


            //try
            //{
            //    //server = new TcpClient(serverIp, 9595);
            //    //connected = true;
            //}
            //catch
            //{
            //    Thread.Sleep(500);
            //    Log("Не удалось подключиться к серверу, повторная попытка");
            //    Connect();
            //}
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
                    Log("getinfo end");

                    Thread thr = CreateMyThread();                    
                    thr.Start();
                    Thread.Sleep(TimeLimit);

                    if (thr.ThreadState == System.Threading.ThreadState.Running)
                    {
                        Log("low sentinfo start");
                        SentInfo(TimeLimit);
                        Log("low sentinfo end");
                    }
                    else
                    {
                        Log("fast sentinfo start");
                        SentInfo(sleeptime);
                        Log("fast sentinfo start");
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
                    sw.Close();
                    //  server.Close();
                    //Thread.Sleep(200000);
                    //Environment.Exit(0);
                    Console.ReadKey();

                }
            }
        }


        static Thread CreateMyThread()
        {
            Thread thr = new Thread(() =>
            {
                Stopwatch a = new Stopwatch();
                a.Start();
                myUser.ACTION = myUser.Play(gameBoard);
                myUser.ACTION = PlayerAction.Down;
                a.Stop();

                sleeptime = a.ElapsedMilliseconds;
                if (sleeptime > TimeLimit)
                {
                    sleeptime = TimeLimit;
                }
                Log("Strategy work Time: " + sleeptime);
            });

            return thr;
        }


        
        /// <summary>
        /// Получить данные от сервера
        /// </summary>
        static void GetInfo()
        {
            //IFormatter formatter = new BinaryFormatter();
            ////   gameBoard = (GameBoard)formatter.Deserialize(strm);
            ////  myUser = (User)formatter.Deserialize(strm);
            //int rn = (int)formatter.Deserialize(strm);

            //Log("desiarelize: " + rn);
           
            if (!File.Exists(gameboardjsonpath) || !File.Exists(userjsonpath))
            {
                Log(" if (!File.Exists(gameboardjsonpath) || !File.Exists(userjsonpath))");
                Thread.Sleep(100);
                GetInfo();
            }           

            try
            {
                using (StreamReader sr = new StreamReader(gameboardjsonpath))
                {
                   int tempGameTimer = int.Parse(sr.ReadLine());

                    

                    if (tempGameTimer == GameTimer)
                    {
                        throw new IOException();
                    }
                    else
                    {
                        GameTimer = tempGameTimer;
                    }
                    string gameboardjson = sr.ReadToEnd();
                    gameBoard = JsonConvert.DeserializeObject<GameBoard>(gameboardjson);                  
                }

                using (StreamReader srr = new StreamReader(userjsonpath))
                {
                    int tempGameTimer = int.Parse(srr.ReadLine());
                    Log(tempGameTimer+ " tempGameTimer");
                    if (tempGameTimer == GameTimer)
                    {
                        throw new IOException();
                    }
                    else
                    {
                        GameTimer = tempGameTimer;
                    }
                    string userjson = srr.ReadToEnd();
                    myUser = JsonConvert.DeserializeObject<User>(userjson);
                    
                }
            }

            catch (IOException)
            {
                Thread.Sleep(100);
                GetInfo();
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
            try
            {
                Log("sentinfo start");
                using (StreamWriter sw = new StreamWriter(actionjsonpath))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(GameTimer);
                    sw.WriteLine(limit + "");
                    sw.WriteLine(ActionToString());
                }
            }
            catch (IOException)
            {
                Log("sentinfo ioexception");
                Thread.Sleep(100);
                SentInfo(limit);
            }            
        }

        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        //static string readStream()
        //{
        //    Byte[] serverData = new Byte[50000000];
        //    int bytes = strm.Read(serverData, 0, serverData.Length);            

        //    string serverMessage = Encoding.Unicode.GetString(serverData, 0, bytes);
        //    return serverMessage;
        //}


        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        //static void writeStream(string message)
        //{
        //    Byte[] data = Encoding.Unicode.GetBytes(message);

        //    strm.Write(data, 0, data.Length);
        //}
    }
}
