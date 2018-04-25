using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

using ClassLibrary_CGC;
using User_class;
using Newtonsoft.Json;
using System.Runtime.InteropServices;


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
        static byte[] data;
        static string message;
        static NetworkStream stream;

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();

        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        private static bool showConsole = true;

        static void Main(string[] args)
        {
            if (showConsole)
            {
                AllocConsole();
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }

            try
            {
                Connect();

                if (server.Connected)
                {
                    Console.WriteLine("start");
                    CommunicateWithServer();
    }
}
            catch (Exception e)
            {
                server.Close();
                Log(e.Message);
//  Console.ReadKey();
Environment.Exit(0);
            }
            Console.ReadKey();
        }

        public static void Log(string message)
{
    using (StreamWriter sw = new StreamWriter("log.txt", true))
    {
        string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
        time = "[" + time + "] ";
        sw.WriteLine(time + ": " + message);
        Console.WriteLine(time + ": " + message);
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

            string gamestring = ReceiveMessage();
            SendMessage("p");
            string userstr = ReceiveMessage();

            gameBoard = JsonConvert.DeserializeObject<GameBoard>(gamestring);
            myUser = JsonConvert.DeserializeObject<User>(userstr);

            SendMessage(gameBoard.Players[0].Name);
            ReceiveMessage();
            myUser.ACTION = myUser.Play(gameBoard);

            SendMessage((int)myUser.ACTION + "");
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: " + e.Message + " " + e.StackTrace);
            connected = false;
            if (server != null)
                server.Close();
            //  Console.ReadKey();
            Environment.Exit(0);
        }
    }
}



static void SendMessage(string message)
{
    byte[] data = Encoding.Unicode.GetBytes(message);
    stream.Write(data, 0, data.Length);
}


static string ReceiveMessage()
{
    string message = "";

    byte[] data = new byte[256]; // буфер для получаемых данных
    StringBuilder builder = new StringBuilder();
    int bytes = 0;
    do
    {
        bytes = stream.Read(data, 0, data.Length);
        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
    }
    while (stream.DataAvailable);
    message = builder.ToString();
    Console.WriteLine("recieved length: " + message.Length);
    return message;
}



///// <summary>
///// Получить данные от сервера
///// </summary>
//static void GetInfo()
//{
//    string start = readStream(server.GetStream());
//    NetworkStream strm = server.GetStream();
//    if (start == "s")
//    {
//        string lengthStr = readStream(server.GetStream());
//        int length = int.Parse(lengthStr);
//        Console.WriteLine("LENGTH " + length);
//        Byte[] data = new byte[length];
//        string json = "";
//        try
//        {
//            string json = readStream(server.GetStream(), length);
//        }
//        catch
//        {
//            Log("readStream error");
//        }


//        for (int i = 0; i < length; i+=500)
//        {
//            int k;
//            while ((k = strm.Read(data, 0, data.Length)) != 0)
//            {

//                json += Encoding.ASCII.GetString(data,0,k);
//            }
//        }


//        gameBoard = JsonConvert.DeserializeObject<GameBoard>(json);
//        Console.WriteLine("Players.Count " + gameBoard.Players.Count);

//        writeStream(server.GetStream(), "p");

//        lengthStr = readStream(server.GetStream());
//        length = int.Parse(lengthStr);
//        json = readStream(server.GetStream(), length);
//        myUser = JsonConvert.DeserializeObject<User>(json);
//    }
//}


/// <summary>
/// Отправить данные на сервер
/// </summary>
static void SentInfo()
{
    string action = EncryptAction(myUser.ACTION);
    writeStream(server.GetStream(), action);
}




/// <summary>
/// Cчитать строку из сетевого потока
/// </summary>
/// <returns></returns>
public static string readStream(NetworkStream strm)
{
    Byte[] serverData = new Byte[16];
    int bytes = strm.Read(serverData, 0, serverData.Length);
    string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
    return serverMessage;
}

/// <summary>
/// Cчитать строку из сетевого потока
/// </summary>
/// <returns></returns>
public static string readStream(NetworkStream strm, int bytesLength)
{
    Byte[] serverData = new Byte[bytesLength];
    int bytes = strm.Read(serverData, 0, serverData.Length);
    string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
    return serverMessage;
}

/// <summary>
/// Отправить строку в сетевой поток
/// </summary>
/// <param name="message">Отправляемая строка</param>
public static void writeStream(NetworkStream strm, string message)
{
    Byte[] data = Encoding.Unicode.GetBytes(message);

    strm.Write(data, 0, data.Length);
}

/// <summary>
/// Отправить строку в сетевой поток
/// </summary>
/// <param name="message">Отправляемая строка</param>
public static void writeStream(NetworkStream strm, Byte[] data)
{
    strm.Write(data, 0, data.Length);
}



/// <summary>
/// Распознать Команду из строки
/// </summary>
/// <param name="message">Данная строка</param>
/// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
/// <param name="i">Индекс элемента в Списке, который получает команду</param>
public static PlayerAction DecryptAction(string message)
{
    PlayerAction pa = new PlayerAction();
    int actionInt = int.Parse(message);
    pa = (PlayerAction)actionInt;
    return pa;
}

/// <summary>
/// Распознать Команду из строки
/// </summary>
/// <param name="message">Данная строка</param>
/// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
/// <param name="i">Индекс элемента в Списке, который получает команду</param>
public static string EncryptAction(PlayerAction action)
{
    int actionInt = (int)action;
    string actionString = actionInt.ToString();
    return actionString;
}
    }
}

