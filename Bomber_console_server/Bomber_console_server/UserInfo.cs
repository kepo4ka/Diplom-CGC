using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary_CGC;
using System.Net;
using System.Net.Sockets;


namespace Bomber_console_server
{
    public class UserInfo
    {
        public Player player;
        public int globalTimeLimit;
        public Compiler compiler;
        public TcpClient client;


        public UserInfo(Player pl, TcpClient cl, Compiler cmp)
        {
            player = pl;           
            compiler = cmp;
            client = cl;
            globalTimeLimit = 0;
        }
    }
}
