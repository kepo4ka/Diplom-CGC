using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_person;
using System.Text;
using System.Threading;

namespace tcpServer_serialize
{
    class Program
    {
        static void Main(string[] args)
        {

           
           


            Int32 port = 9595;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr, port);
            server.Start();


            Console.WriteLine("Server");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream strm = client.GetStream();

                string response = ReceiveMessage(strm);
                Console.WriteLine(response);
                SendMessage(bigString(), strm);
                strm.Close();
                client.Close();

            }

            Console.ReadKey();

            //strm.Close();
            //client.Close();
            server.Stop();
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
            message = builder.ToString();
            return message;
        }

        static string bigString()
        {
            
            string add = "odsfjpsodfjposidjpfoijs";

            for (int i = 0; i < 15; i++)
            {
                add += add;
            }
            
            Console.WriteLine(add.Length);

            return add;

        }


    }


}
