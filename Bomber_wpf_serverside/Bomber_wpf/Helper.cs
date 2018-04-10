using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

using ClassLibrary_CGC;
using System.Diagnostics;

namespace Bomber_wpf
{
    class Helper
    {

        /// <summary>
        /// Распознать Команду из строки
        /// </summary>
        /// <param name="message">Данная строка</param>
        /// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
        /// <param name="i">Индекс элемента в Списке, который получает команду</param>
        public static void DiscoverAction(string message, ref List<UserInfo> usersInfo, int i)
        {
            string[] messagePie = message.Split(' ');

            if (message.Length < 1 || message == "" || messagePie[0] != "action")
            {
                throw new Exception("Сообщение от стратегии игрока " + usersInfo[i].player.Name + " неверное");
            }

            switch (messagePie[1])
            {
                case "1":
                    usersInfo[i].player.ACTION = PlayerAction.Bomb;
                    break;
                case "2":
                    usersInfo[i].player.ACTION = PlayerAction.Down;
                    break;
                case "3":
                    usersInfo[i].player.ACTION = PlayerAction.Left;
                    break;
                case "4":
                    usersInfo[i].player.ACTION = PlayerAction.Right;
                    break;
                case "5":
                    usersInfo[i].player.ACTION = PlayerAction.Up;
                    break;
                default:
                    usersInfo[i].player.ACTION = PlayerAction.Wait;
                    break;
            }
        }

        /// <summary>
        /// Случайный md5 хеш
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CalculateMD5Hash(string input)
        {

            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }



        /// <summary>
        /// Выделить из Пути файла имя этого Файла
        /// </summary>
        /// <param name="ppath">Полный путь до файла</param>
        /// <returns>Имя файла</returns>
        public static string SpliteEndPath(string ppath, bool k = false)
        {
            Stack<char> tsymbols = new Stack<char>();
            string nfileName = "";

            int tindex = ppath.Length - 1;

            for (; tindex >= 0; tindex--)
            {
                if (ppath[tindex] == '.')
                {
                    break;
                }
            }

            tindex--;

            for (; tindex >= 0; tindex--)
            {

                if (ppath[tindex] == '\\')
                {
                    break;
                }
                tsymbols.Push(ppath[tindex]);
            }

            while (tsymbols.Count > 0)
            {
                nfileName += tsymbols.Pop();
            }

            if (k)
            {
                nfileName = ppath.Substring(0, ppath.IndexOf(nfileName + ".cs"));
            }
            return nfileName;
        }


        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        public static string readStream(NetworkStream strm)
        {
            Byte[] serverData = new Byte[16];
            int bytes = strm.Read(serverData, 0, serverData.Length);

            string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
            return serverMessage;
        }

        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        public static void writeStream(NetworkStream strm, string message)
        {
            Byte[] data = Encoding.ASCII.GetBytes(message);

            strm.Write(data, 0, data.Length);
        }

           /// <summary>
        /// Добавить информацию в лог
        /// </summary>
        /// <param name="message"></param>
        public static void LogUpdate(string message, ref System.Windows.Forms.TextBox log_box)
        {
            string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
            time = "[" + time + "] ";

            log_box.Text += time + message + Environment.NewLine;
        }


        /// <summary>
        /// Запустить процесс со специфичными настройками
        /// </summary>
        /// <param name="stroke">Команда и её параметры</param>
        /// <returns>Результат выполнения процесса</returns>
        public static string startProccess(string stroke)
        {
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo(
                    "cmd", "/c " + stroke);
            // Следующая команды означает, что нужно перенаправить стандартынй вывод
            // на Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // не создавать окно CMD
            procStartInfo.CreateNoWindow = true;

            Process proc = new Process();
            // Получение текста в виде кодировки 866 win
            procStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            //запуск CMD
            proc.StartInfo = procStartInfo;
            proc.Start();
            //чтение результата
            string result = proc.StandardOutput.ReadToEnd();
            return result;
        }

    }
}
