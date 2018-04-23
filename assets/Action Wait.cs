using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary_CGC;

namespace User_class
{
    [Serializable]
    public class User : Player
    {
        public override PlayerAction Play(GameBoard gb)
        {
            Random rn = new Random();
            PlayerAction action;
            action = PlayerAction.Wait;
            /*
                 Код пользователя
            */

            return action;
        }
    }
}
