using System;

namespace Bomber_console_server
{
     class Docker
    {
        static string output = "";
        static string errorput = "";        

        /// <summary>
        /// Запустить Docker контейнер
        /// </summary>
        /// <param name="image">Образ, который будет использоваться</param>
        /// <param name="hostPath">Путь на хост машине в специальном формате:  /c/Users/Galaxy/Desktop/dockertest</param>
        /// <param name="contPath">Путь внутри контейнера</param>
        /// <returns>ID запущенного контейнера</returns>
        public static string Run(string image, string hostPath, string name, string contPath="/cgc")
        {
            string containerId = "";
            output = "";
            errorput = "";
            hostPath = VolumeFormat(hostPath);

            Helper.startProccess($"docker-machine ssh default docker run -it -d --network=host --name={name} -m 64m --memory-swap 64m --rm --volume {hostPath}:{contPath} {image}", out output, out errorput);           

            if (errorput != "")
            {
                throw new Exception($"Run ERROR: {errorput}");
            }
            else if (output == "")
            {
                throw new Exception($"Run ERROR: не получен ID контейнера");
            }

            containerId = output;
            return containerId;
        }

        /// <summary>
        /// Приведение Пути до спец формата для docker run --volume
        /// </summary>
        /// <param name="origin">Начальный путь</param>
        /// <returns>Путь в спец формате</returns>
        public static string VolumeFormat(string origin)
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
        public static void StopContainer(string ID)
        {
            output = "";
            errorput = "";
            Helper.startProccess($"docker-machine ssh default docker kill {ID}", out output, out errorput);

            if (errorput !="")
            {
                throw new Exception($"StopContainer ERROR: {errorput}");
            }
        }
          

        /// <summary>
        /// Запустить команду внутри контейнера
        /// </summary>
        /// <param name="ID">ID запущенного контейнера</param>
        /// <param name="command">Команда вместе с параметрами</param>
        public static void Exec(string ID, string command)
        {
            output = "";
            errorput = "";
            Helper.startProccess($"docker-machine ssh default docker exec -it -d {ID} {command}", out output, out errorput);

            if (errorput != "")
            {
                throw new Exception($"StopContainer ERROR: {errorput}");
            }
        }
    }
}
