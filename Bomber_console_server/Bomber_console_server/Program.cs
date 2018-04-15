using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bomber_console_server
{
    class Program
    {
        static void Main(string[] args)
        {

            Session session = new Session(GetUserSourcePathsFromFile());
            session.InitGame();
            Console.WriteLine("ССЕсСИЯ игры завершена");
            Console.ReadKey();
        }

        static string[] GetUserSourcePathsFromFile()
        {
            string[] usersPath = new string[4];
            //using (StreamReader sr = new StreamReader("users_source.txt"))
            //{
            //    for (int i = 0; i < usersPath.Length; i++)
            //    {
            //        usersPath[i] = sr.ReadLine();
            //    }
            //}

            int i = 0;
            DirectoryInfo di = new DirectoryInfo(Path.GetFullPath($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..")}\\User_sources"));
            foreach (FileInfo fi in di.GetFiles())
            {
                usersPath[i] = fi.FullName;
                i++;
            }

            return usersPath;
        }
    }
}
