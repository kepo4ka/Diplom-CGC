using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomber_console_server.Model
{
   public class Game
    {
     public   int id;
       public int datetime;
        public UserGroup usergroup = new UserGroup();
       public string status;
        public dbUser creator = new dbUser();
    }
}
