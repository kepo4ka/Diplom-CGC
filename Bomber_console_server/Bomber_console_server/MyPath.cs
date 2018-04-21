using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomber_console_server
{
    public class MyPath
    {
        public static string gameStaterVisualizerFileName = "Visualizer.dat";
        public static string gameStaterVisualizerJSONFileName = "Visualizer.json";
        public static string userComandsFileName = "UserCommands.json";
        public static string gameStaterVisualizeFileNamerDATtoGZ = "VisualizerDAT.gz";
        public static string gameStaterVisualizeFileNamerJSONtoGZ = "VisualizerJSON.gz";
        public static string gameResultsFileName = "gameResults.json";

       public static string binDir = "C:\\xampp\\htdocs\\sandbox";
       public static string source_file_name = "strategy.cs";
       public static string exe_file_name = "Program.exe";

        public static string gameboardjsonpath = "gameboard.json";
        public static string userjsonpath = "user.json";

        public static string userClass_dllName = "User_class.dll";
        public static string ClassLibrary_CGC = "ClassLibrary_CGC.dll";
        public static string NewtonJsonLibraryName = "Newtonsoft.Json.dll";

        public static string dockerImage = "kepo4ka/ubuntu_mono";
    }
}
