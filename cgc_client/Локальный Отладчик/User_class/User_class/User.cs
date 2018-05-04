using System;
using System.Linq;
using System.Collections.Generic;

using ClassLibrary_CGC;

namespace User_class
{
    [Serializable]
    public class User : Player
    {
        public GameBoard gameboard;

        public int Tick;

        /// <summary>
        /// Задать Команду на следующий Тик
        /// </summary>
        /// <param name="gb">Игровое Поле</param>
        /// <returns>Команда для Юнита</returns>
        public override PlayerAction Play(GameBoard gb)
        {
            /*
                Код пользователя
            */
            Random rn = new Random();
            gameboard = gb;

            PlayerAction action = PlayerAction.Wait;

            int difX = (X - gb.W / 2);
            int difY = (Y - gb.H / 2);

            if (difY > 0)
                action = PlayerAction.Up;
            else if (difY < 0)
            {
                action = PlayerAction.Down;
            }

            else if (difY > 0)
            {
                action = PlayerAction.Right;
            }
            else if (difY < 0)
            {
                action = PlayerAction.Left;
            }
            else action = PlayerAction.Bomb;




            return action;
        }




    }
}
