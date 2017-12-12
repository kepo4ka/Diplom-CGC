using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_person;


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
            TcpClient client = server.AcceptTcpClient();
            NetworkStream strm = client.GetStream();
            IFormatter formatter = new BinaryFormatter();

            Person p = (Person)formatter.Deserialize(strm);

            Console.WriteLine("Hi, I'm " + p.FirstName);

            strm.Close();
            client.Close();
            server.Stop();
        }
    }


}
