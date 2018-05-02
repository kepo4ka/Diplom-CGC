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

            if (X<1)
            {
                action = PlayerAction.Down;
            }

            if (gb.XYinfo[X,Y+1].Player != null || gb.Cells[X,Y+1].Type==CellType.Destructible)
            {
                action = PlayerAction.Bomb;
            }
            if (gb.XYinfo[X,Y+1].Bomb != null || gb.XYinfo[X,Y].Bomb != null)
            {
                action = PlayerAction.Up;
            }
           

            return action;
        }


        

    }
}
