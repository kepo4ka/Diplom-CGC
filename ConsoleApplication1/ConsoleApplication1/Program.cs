using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary_CGC;
using User_class;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            GameBoard gb = new GameBoard();


            User_class.User us = new User();

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(us.Play(gb));

            }

            Console.ReadKey();
        }
    }
}
