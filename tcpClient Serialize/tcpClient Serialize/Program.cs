using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_person;


namespace tcpClient_Serialize
{
    class Program
    {
        static void Main(string[] args)
        {
            Person p = new Person(); // create my serializable object 
            string serverIp = "127.0.0.1";

            TcpClient client = new TcpClient(serverIp, 9595); // have my connection established with a Tcp Server 

            IFormatter formatter = new BinaryFormatter(); // the formatter that will serialize my object on my stream 

            NetworkStream strm = client.GetStream(); // the stream 
            formatter.Serialize(strm, p); // the serialization process 

            strm.Close();
            client.Close();
        }
    }
}
