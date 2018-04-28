using System;
using System.Linq;
using System.Collections.Generic;

using ClassLibrary_CGC;

namespace User_class
{
    [Serializable]
    public class User : Player
    {
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
            PlayerAction action;


            action = (PlayerAction)rn.Next(0, 6);

            return action;
        }
    }
}
