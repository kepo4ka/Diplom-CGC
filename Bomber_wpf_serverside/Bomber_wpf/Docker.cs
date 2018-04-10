using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomber_wpf
{
     class Docker
    {
        /// <summary>
        /// Запустить docker контейнер
        /// </summary>       
        /// <returns>ID запущенного контейнера</returns>
      public static string dockerRun()
        {
            string containerId = "";
            string runResult = Helper.startProccess("docker-machine shh default docker run --rm -d kepo4ka/ubuntu_mono");
            bool isError = false;

            for (int i = 0; i < runResult.Length; i++)
            {
                if (runResult[i] == ' ')
                {
                    isError = true;
                    break;
                }
            }

            if (!isError)
            {
                containerId = runResult;
            }
            return containerId;
        }


        public static void dockerStopContainer(string ID)
        {
            Helper.startProccess($"docker-machine shh default docker stop {ID}");
        }


    }
}
