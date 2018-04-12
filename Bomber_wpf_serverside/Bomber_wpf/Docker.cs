using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bomber_wpf
{
     class Docker
    {

        static string output = "";
        static string errorput = "";

        /// <summary>
        /// Запустить docker контейнер
        /// </summary>
        /// <param name="image">Образ, который будет использоваться</param>
        /// <param name="hostPath">Путь на хост машине в специальном формате:  /c/Users/Galaxy/Desktop/dockertest</param>
        /// <param name="contPath">Путь внутри контейнера</param>
        /// <returns>ID запущенного контейнера</returns>
        public static string dockerRun(string image, string hostPath, string contPath="/cgc")
        {
            string containerId = "";
            output = "";
            errorput = "";

            hostPath = dockerVolumeFormat(hostPath);

            Helper.startProccess($"docker-machine ssh default docker run -it -d --network=host --name=test --rm --volume {hostPath}:{contPath} {image}", out output, out errorput);           

            if (errorput != "")
            {
                throw new Exception($"dockerRun ERROR: {errorput}");
            }
            else if (output == "")
            {
                throw new Exception($"dockerRun ERROR: не получен ID контейнера");
            }
            MessageBox.Show(output);

            containerId = output;            
            return containerId;
        }

        /// <summary>
        /// Приведение Пути до спец формата для docker run --volume
        /// </summary>
        /// <param name="origin">Начальный путь</param>
        /// <returns>Путь в спец формате</returns>
        public static string dockerVolumeFormat(string origin)
        {
            string result = "";
            origin = origin.Replace(":\\\\", "/");
            origin = origin.Replace(":\\", "/");
            origin = origin.Replace("\\\\", "/");
            origin = origin.Replace("\\", "/");

            result += "/";
            result += origin[0].ToString().ToLower();
            result += origin.Substring(1);          
            return result;
        }


        /// <summary>
        /// Остановить контейнер
        /// </summary>
        /// <param name="ID">ID запущенного контейнера</param>
        public static void dockerStopContainer(string ID)
        {
            output = "";
            errorput = "";
            Helper.startProccess($"docker-machine ssh default docker stop {ID}", out output, out errorput);

            if (errorput !="")
            {
                throw new Exception($"dockerStopContainer ERROR: {errorput}");
            }
        }
          

        /// <summary>
        /// Запустить команду внутри контейнера
        /// </summary>
        /// <param name="ID">ID запущенного контейнера</param>
        /// <param name="command">Команда вместе с параметрами</param>
        public static void dockerExec(string ID, string command)
        {
            output = "";
            errorput = "";
            Helper.startProccess($"docker-machine ssh default docker exec -it  {ID} {command}", out output, out errorput);

            if (errorput != "")
            {
                throw new Exception($"dockerStopContainer ERROR: {errorput}");
            }
        }


    }
}
