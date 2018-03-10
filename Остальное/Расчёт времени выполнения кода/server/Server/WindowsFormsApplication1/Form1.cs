using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string serverIp = "127.0.0.1";
        TcpListener server;
        TcpClient cl;
        static int tick = 0;
        static int value = 0;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            value = 0;
            IPAddress ip = IPAddress.Parse(serverIp);
            server = new TcpListener(ip, 9595);
            server.Start();
            cl = server.AcceptTcpClient();

           readMessage();
            //timer.Interval = 1000;
            //timer.Tick += Timer_Tick;
            //timer.Start();
         //   textBox1.Text = value + "!";

        }


        private async void Timer_Tick(object sender, EventArgs e)
        {
                      
        }



        public void readMessage()
        {
            //      BinaryFormatter fr = new BinaryFormatter();
            string message = "";
            NetworkStream stream = cl.GetStream();
            //   stream.ReadTimeout = 1000;

            string[] res_array = new string[4];


            for (int i = 0; i < res_array.Length; i++)
            {
                res_array[i] = "";
                Thread thr = new Thread(() =>
                {
                    res_array[i] = ReadStream(stream);
                });

                thr.Start();
                Thread.Sleep(1000);

                if (res_array[i] == "" && thr.IsAlive)
                {
                    message += (i + 1) + " - долго " + thr.ManagedThreadId + "\n";
                }
                else
                {
                    message += (i + 1) + " - быстро " + thr.ManagedThreadId + "\n";
                }
            }
            MessageBox.Show(message);
        }



        public string ReadStream(NetworkStream str)
        {

            byte[] data = new byte[512];
            
            Int32 bytes = str.Read(data, 0, data.Length);           

            string result = Encoding.Unicode.GetString(data, 0, data.Length);

            return result;
        }
    }
}
