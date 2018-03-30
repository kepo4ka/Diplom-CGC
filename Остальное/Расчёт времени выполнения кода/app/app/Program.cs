using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("test1");
           
            Console.WriteLine("test2");
            Process a = new Process();
            a.StartInfo.FileName = "app1.exe";
            a.Start();
          
        }
    }
}
