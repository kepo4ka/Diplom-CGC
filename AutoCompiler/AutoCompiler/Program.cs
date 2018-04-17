using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;

namespace AutoCompiler
{
    class Program
    {
        static string binDir = "C:\\xampp\\htdocs\\cgc\\sources\\";
        static MySQL mysql;


        static void Main(string[] args)
        {
            try
            {
                Monitoring();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                mysql.myConnection.Close();
                Monitoring();
            }            
        }


        static void Monitoring()
        {
            mysql = new MySQL();

            while (true)
            {      
                int count = mysql.GetWaitCount();

                while (count > 0)
                {

                    string source_path = "";
                    int user_id = 0;
                    mysql.GetSource(out source_path, out user_id);
                    source_path = $"{source_path}\\{Compiler.userClientexe_Name}";

                    if (mysql.SetWorkStatus(user_id, "work"))
                    {
                        string fullpath = GetSourceFullName(source_path);
                        try
                        {
                            CompileProccess(fullpath);
                        }
                        catch (Exception er)
                        {
                            mysql.SetErrorStatus(user_id, er.Message);
                        }
                        fullpath = fullpath.Replace(binDir, "").Replace("\\", "/").Substring(1);

                        mysql.SetCompiledStatus(user_id, fullpath);
                    }
                    count--;
                }

                Thread.Sleep(5000);
            }
        }


        static void CompileProccess(string fullpath)
        {
            Compiler compiler = new Compiler(fullpath);
            compiler.Compile();
            compiler.BackFile(Helper.GetFileDirectory(fullpath));
            Compiler.EndProccess();
        }


        static string GetSourceFullName(string source_path)
        {            
            string fullPath = $"{binDir}\\{source_path}";
            fullPath = fullPath.Replace('/', '\\');
            if (!File.Exists(fullPath))
            {
                return "";
            }
            return fullPath;
        }




    }
}
