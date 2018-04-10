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
        }


    }
}
