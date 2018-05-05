using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ClassLibrary_CGC;

namespace User_class
{
    [Serializable]
    public class User : Player
    {
		Random rn = new Random();
        public override PlayerAction Play(GameBoard gb)
        {
            PlayerAction action = PlayerAction.Wait;
            action = (PlayerAction)rn.Next(6);
			Thread.Sleep(1000);
            return action;
        }
    }
}