using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Host_IP_search
{
    class Program
    {

        static List<string> validURL = new List<string>();


        static void Main(string[] args)
        {

            // Add a user agent header in case the 
            // requested URI contains a query.

            //  client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string startIp = "";
            string endIp = "";

            if (args.Length == 2)
            {
                 startIp = args[0];
                 endIp = args[1];
            }
            else
            {
                 startIp = "0.0.0.0";
                 endIp = "255.255.255.255";
            }

            Console.WriteLine(startIp + " - " + endIp);
            IPAddress startIP = IPAddress.Parse(startIp);
            IPAddress endIP = IPAddress.Parse(endIp);

            List<IPAddress> IP_list = GetIPList(startIP, endIP);

            Console.WriteLine("ip сформированы");
           
            Random rn = new Random();
            string temp_ip = "";           
           
            for (int i = 0; i < IP_list.Count; i++)
            {
                temp_ip = IP_list[i].ToString();

                int[] ipform = new int[4];

                for (int j = 0; j < ipform.Length; j++)
                {
                    ipform[j] = int.Parse(temp_ip.Split('.')[j]);
                }
                

                
             //   ThreadPool.QueueUserWorkItem((object state) => {
                    try
                    {
                        string tempurl = ipform[0] + "." + ipform[1] + "." + ipform[2] + "." + ipform[3];             

                        if (GetContent("http://" + tempurl).Length > 2)
                        {
                            // StreamWriter sw = new StreamWriter(temp.Replace('.', '_'), false);
                            // StreamWriter sw = new StreamWriter(rn.Next(0,1000)+"", false);

                            StreamWriter sw = new StreamWriter("ip.txt", true);
                            sw.WriteLine(tempurl);
                            sw.Close();
                        }
                    }
                    catch
                    {

                    }
             //   });


             
                //Thread th = new Thread(() => {

                //    s = GetContent("http://" + temp_ip);
                //    lock (x)
                //    {
                //        validURL.Add(temp_ip);
                //    }
                //});              
                 
            }

            for (int i = 0; i < validURL.Count; i++)
            {
                Console.WriteLine(validURL[i]);
            }
            Console.WriteLine("Завершено");
            Console.ReadKey();
       
        }

        private static List<IPAddress> GetIPList(IPAddress ipFrom, IPAddress ipTo)
        {
            List<IPAddress> ipList = new List<IPAddress>();
            String[] arrayFrom = ipFrom.ToString().Split(new Char[] { '.' });
            String[] arrayTo = ipTo.ToString().Split(new Char[] { '.' });

            long firstIP = (long)(Math.Pow(256, 3) * Convert.ToInt32(arrayFrom[0]) + Math.Pow(256, 2) * Convert.ToInt32(arrayFrom[1]) + Math.Pow(256, 1) * Convert.ToInt32(arrayFrom[2]) + Convert.ToInt32(arrayFrom[3]));
            long lastIP = (long)(Math.Pow(256, 3) * Convert.ToInt32(arrayTo[0]) + Math.Pow(256, 2) * Convert.ToInt32(arrayTo[1]) + Math.Pow(256, 1) * Convert.ToInt32(arrayTo[2]) + (Convert.ToInt32(arrayTo[3]) + 1));

            for (long i = firstIP; i < lastIP; i++)
                ipList.Add(IPAddress.Parse(string.Join(".", BitConverter.GetBytes(i).Take(4).Reverse())));
            return ipList;
        }




        static string GetContent(string url)
        {
            string res = "";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Timeout = 5;
            HttpWebResponse resp;

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch
            {
               // Console.WriteLine(url + " timeout");
                throw new Exception { };
              
            }
            using (StreamReader stream = new StreamReader(
                 resp.GetResponseStream(), Encoding.UTF8))
            {
               res = stream.ReadLine();
            }
            return res;
        }



    }
}
