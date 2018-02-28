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
            if (rn.Next(1, 10) % 2 == 0)
            {
                action = PlayerAction.Up;
            }
            else
            {
                action = PlayerAction.Down;
            }
            /*
                 Код пользователя
            */
            for (int i = 0; i < 1000000; i++)
            {
                for (int j = 0; j < 100000; j++)
                {

                }
            }

            return action;
        }
    }
}
