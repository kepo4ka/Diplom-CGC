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
        static string binDir = "C:\\xampp\\htdocs\\sources";
        static string source_file_name = "strategy.cs";
        static string exe_file_name = "Program.exe";
        static MySQL mysql;


        static void Main(string[] args)
        {
            try
            {
                Monitoring();
            }
            catch (Exception e)
            {
               Helper.LOG(e.Message);
                mysql.myConnection.Close();
                Monitoring();
            }            
        }


        static void Monitoring()
        {
            mysql = new MySQL();
            Helper.LOG("Старт поиска...");

            while (true)
            {      
                int count = mysql.GetWaitCount();             
                
                while (count > 0)
                {
                    Helper.LOG("Количество ждущих :" + count);                    
                    int source_id = -1;
                    int user_id = -1;
                    mysql.GetWaitSourcesId(out source_id, out user_id);

                    if (source_id>0  && mysql.SetWorkStatus(source_id, "work"))
                    {
                        string fullpath = GetSourceFullName(source_id);

                        try
                        {
                            CompileProccess(fullpath);

                            Helper.LOG($"Compile Success: user_id - {user_id}, source_id - {source_id}");
                            mysql.SetCompiledStatus(source_id);
                        }
                        catch (Exception er)
                        {
                            Helper.LOG($"Compile ERROR: user_id - {user_id}, source_id -{source_id} : {er.Message}");                            
                            mysql.SetErrorStatus(source_id, er.Message);
                        }
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


        static string GetSourceFullName(int user_id)
        {            
            string fullPath = $"{binDir}/{user_id}/{source_file_name}";
            fullPath = fullPath.Replace('/', '\\');
            if (!File.Exists(fullPath))
            {
                return "";
            }
            return fullPath;
        }


     

    }
}
