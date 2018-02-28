using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using OneExeProgram.Domain;
using OneExeProgram.Properties;
using ClassLibrary_CGC;

namespace OneExeProgram
{
    public class Program
    {
        /// <summary>
        /// Точка входа
        /// </summary>
        public static void Main( string[] args )
        {
            // Прицепиться к событию определения сборки
            AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;


            // Проверить работоспособность
            Console.WriteLine( "AutoMapper works: {0}", new AutoMapperTester( ).IsCorrect );

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine();
            }
            try
            {
                Console.WriteLine("classlibrary work???: {0}", new GameBoard().H);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.ReadKey( );
        }

        /// <summary>
        /// Событие возникающее при не возможности найти сборку
        /// </summary>
        private static Assembly AppDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            Console.WriteLine("--------------------");
            Console.WriteLine(args.Name);
            Console.WriteLine("--------------------");

            if ( args.Name.Contains( "AutoMapper" ) )
            {
                Console.WriteLine( "Resolving assembly: {0}", args.Name );

                // Загрузка запакованной сборки из ресурсов, ее распаковка и подстановка
                using( var resource = new MemoryStream( Resources.AutoMapper_dll ) )
                using( var deflated = new DeflateStream( resource, CompressionMode.Decompress ) )
                using( var reader = new BinaryReader( deflated ) )
                {
                    var one_megabyte = 1024 * 1024;
                    var buffer = reader.ReadBytes( one_megabyte );
                    return Assembly.Load( buffer );
                }
            }

            else if (args.Name.Contains("ClassLibrary_CGC"))
            {
                Console.WriteLine("Resolving assembly: {0}", args.Name);

                // Загрузка запакованной сборки из ресурсов, ее распаковка и подстановка
                using (var resource = new MemoryStream(Resources.ClassLibrary_CGC_dll))
                using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflated))
                {
                    var one_megabyte = 1024 * 1024;
                    var buffer = reader.ReadBytes(one_megabyte);
                    return Assembly.Load(buffer);
                }
            }

            return null;
        }
    }
}
