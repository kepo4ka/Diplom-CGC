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

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream strm = client.GetStream();
                //IFormatter formatter = new BinaryFormatter();

                //Person p = (Person)formatter.Deserialize(strm);

                //Console.WriteLine("Hi, I'm " + p.FirstName);

                byte[] data = new byte[256];
                strm.Read(data, 0, data.Length);

                string message = Encoding.UTF8.GetString(data);

                data = Encoding.UTF8.GetBytes("привет");
                strm.Write(data, 0, data.Length);

                Console.WriteLine(message);
            }
            Console.ReadKey();

            //strm.Close();
            //client.Close();
            server.Stop();
        }
    }


}
