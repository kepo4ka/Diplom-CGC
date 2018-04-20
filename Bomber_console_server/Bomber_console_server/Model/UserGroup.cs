using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomber_console_server.Model
{
   public class UserGroup
    {     
       public int group_id;

        public List<User> users = new List<User>();
    }
}
