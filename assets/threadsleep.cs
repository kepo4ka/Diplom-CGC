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
public override PlayerAction Play(GameBoard gb)
{
/*
Код пользователя
*/
Random rn = new Random();
PlayerAction action;
Thread.Sleep(rn.Next(100,1000));

action = (PlayerAction)rn.Next(6);




return action;
}
}
}