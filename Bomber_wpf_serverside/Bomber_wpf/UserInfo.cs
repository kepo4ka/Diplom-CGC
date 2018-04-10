using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary_CGC;
using System.Net;
using System.Net.Sockets;


namespace Bomber_wpf
{
    public class UserInfo
    {
        public Player player;
        public TcpClient client;
        public int globalTimeLimit;

        public UserInfo(Player pl, TcpClient cl)
        {
            player = pl;
            client = cl;
            globalTimeLimit = 0;
        }
    }
}
