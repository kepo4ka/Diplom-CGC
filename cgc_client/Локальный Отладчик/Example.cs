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
            PlayerAction action = PlayerAction.Wait;
            if (gb.Tick == 1)
            {
                action = PlayerAction.Bomb;
            }
            if (gb.Tick == 2)
            {
                action = PlayerAction.Wait;
            }
            if (gb.Tick == 3)
            {
                action = PlayerAction.Up;
            }
            if (gb.Tick == 4)
            {
                action = PlayerAction.Right;
            }

            return action;
        }
    }
}