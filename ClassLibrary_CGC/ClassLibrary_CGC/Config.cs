using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.CSharp;
using System.Dynamic;

namespace ClassLibrary_CGC
{
    /// <summary>
    /// Конфигурация
    /// </summary>   
    public class Config
    {       
        private static int gameTicksMax;
        private static int wait_time;
        private static int client_program_memory_quote;
        private static int bonuses_count;
        private static int bang_start_radius;
        private static int lava_livetime;
        private static int bomb_live_time;
        private static int player_bombs_count_start;
        private static int player_kill_points;
        private static int player_survive_points;
        private static int player_cell_destroy_points;
        private static int player_win_points;
        private static int player_bonus_find_points;
        private static Color cell_destructible_color;
        private static Color cell_indestructible_color;
        private static Color lava_color;
        private static Color bomb_color;
        private static Color bonus_fast;
        private static Color bonus_big;

  

        public static int GameTicksMax { get => gameTicksMax; set => gameTicksMax = value; }
        public static int Wait_time { get => wait_time; set => wait_time = value; }
        public static int Client_program_memory_quote { get => client_program_memory_quote; set => client_program_memory_quote = value; }
        public static int Bonuses_count { get => bonuses_count; set => bonuses_count = value; }
        public static int Bang_start_radius { get => bang_start_radius; set => bang_start_radius = value; }
        public static int Lava_livetime { get => lava_livetime; set => lava_livetime = value; }
        public static int Bomb_live_time { get => bomb_live_time; set => bomb_live_time = value; }
        public static int Player_bombs_count_start { get => player_bombs_count_start; set => player_bombs_count_start = value; }
        public static int Player_kill_points { get => player_kill_points; set => player_kill_points = value; }
        public static int Player_survive_points { get => player_survive_points; set => player_survive_points = value; }
        public static int Player_cell_destroy_points { get => player_cell_destroy_points; set => player_cell_destroy_points = value; }
        public static int Player_win_points { get => player_win_points; set => player_win_points = value; }
        public static int Player_bonus_find_points { get => player_bonus_find_points; set => player_bonus_find_points = value; }
        public static Color Cell_destructible_color { get => cell_destructible_color; set => cell_destructible_color = value; }
        public static Color Cell_indestructible_color { get => cell_indestructible_color; set => cell_indestructible_color = value; }
        public static Color Lava_color { get => lava_color; set => lava_color = value; }
        public static Color Bomb_color { get => bomb_color; set => bomb_color = value; }
        public static Color Bonus_fast { get => bonus_fast; set => bonus_fast = value; }
        public static Color Bonus_big { get => bonus_big; set => bonus_big = value; }

        /// <summary>
        /// Настроить config из json объекта
        /// </summary>
        /// <param name="obj">Json объект</param>
        public static void SetConfig(dynamic obj)
        {
            GameTicksMax =  obj.GameTicksMax;
            Wait_time = obj.Wait_time;
            Client_program_memory_quote = obj.Client_program_memory_quote;
            Bonuses_count = obj.Bonuses_count;
            Bang_start_radius = obj.Bang_start_radius;
            Lava_livetime = obj.Lava_livetime;
            Bomb_live_time = obj.Bomb_live_time;
            Player_bombs_count_start = obj.Bomb_live_time;
            Player_kill_points = obj.Player_kill_points;
            Player_survive_points = obj.Player_survive_points;
            Player_cell_destroy_points = obj.Player_cell_destroy_points;
            Player_win_points = obj.Player_cell_destroy_points;
            Player_bonus_find_points = obj.Player_bonus_find_points;
            Cell_destructible_color =obj.Cell_destructible_color;
            Cell_indestructible_color = obj.Cell_indestructible_color;
            Lava_color = obj.Lava_color;
            Bomb_color = obj.Bomb_color;
            Bonus_fast = obj.Bonus_fast;
            Bonus_big = obj.Bonus_big;
        }
    }
}
