using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ClassLibrary_person;



namespace tcpClient_Serialize
{
    class Program
    {
        static void Main(string[] args)
        {
          
            string serverIp = "127.0.0.1";
            int port = 9595;

            Console.WriteLine("Client");

            TcpClient client = new TcpClient(serverIp, port); // have my connection established with a Tcp Server       

            NetworkStream strm = client.GetStream(); // the stream 

            SendMessage("I client", strm);

            string response = ReceiveMessage(strm);

           // Console.WriteLine(response);

            Console.ReadKey();
            strm.Close();
            client.Close();
        }


        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        static void SendMessage(string message, NetworkStream stream)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Получить сообщение
        /// </summary>
        /// <returns>Полученное сообщение</returns>
        static string ReceiveMessage(NetworkStream stream)
        {
            string message = "";

            byte[] data = new byte[256];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;

            do
            {
                bytes = stream.Read(data, 0, data.Length);
               
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);


            while ((bytes = stream.Read(data, 0, data.Length)) != 0)
            {
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }

            message = builder.ToString();

            Console.WriteLine(message.Length);
            return message;
        }



    }
}
