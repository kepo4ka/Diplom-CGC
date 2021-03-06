﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Dynamic;
using Bomber_console_server.Model;


namespace Bomber_console_server
{
   public class MySQL
    {
       public MySqlConnection myConnection;
        string Connect = "Database=test;Data Source=127.0.0.1;User Id=root;Password=''; CharSet=utf8";

        public MySQL()
        {
            myConnection = new MySqlConnection(Connect);
            myConnection.Open();    
        }

        public void GetUsers()
        {
            string sql = "Select name, is_bot FROM users";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                     
                        string name = reader.GetString(0);
                        bool is_bot = reader.GetBoolean(1);
                        Console.WriteLine(name + " is bot - " + is_bot.ToString());
                    }

                }
            }
        }

        public void GetWaitSourcesId(out int id, out int user_id)
        {          
            id = -1;
            user_id = -1;
            string sql = "Select id,user_id FROM sources WHERE status='wait' LIMIT 1";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    id = reader.GetInt16(0);
                    user_id = reader.GetInt16(1);                  
                }
            }           
          
        }

        public int GetWaitCount()
        {           
            string sql = "Select COUNT(id) FROM sources WHERE status='wait'";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;
            int count = 0;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                
                if (reader.HasRows)
                {
                    reader.Read();
                    count = reader.GetInt16(0);                  
                }
            }
            return count;
        }


        public bool SetWorkStatus(int id, string status)
        {
            //   string sql = "UPDATE sources SET error='', exe_path='', status=@status WHERE user_id=@user_id";
            string sql = $"UPDATE sources SET status='{status}' WHERE id=@id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@status", SqlDbType.VarChar).Value = status;
            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows==1)
            {
                return true;
            }
            return false;
        }


        public bool SetCompiledStatus(int id)
        {
            string sql = $"UPDATE sources SET error='', status='ok' WHERE id=@id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }


        public bool SetErrorStatus(int id, string error)
        {           
            string sql = $"UPDATE sources SET error='{error}', status='error' WHERE id=@id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }



        public UserGroup GetUsersInGroup(int group_id)
        {
            UserGroup ug = new UserGroup();

            string sql = $"Select user_id FROM user_group WHERE group_id={group_id}";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;           

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {                      
                        User us = new User();
                        us.
                    }
                }
            }
            ug.users = users;
            return ug;
           
        }

        public List<SandboxGame> GetWaitGameSandBox()
        {
            List<SandboxGame> gameinfo = new List<SandboxGame>();

            string sql = $"Select * FROM sandbox_game_session WHERE status='wait'";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SandboxGame sb = new SandboxGame();
                        sb.id = reader.GetInt16(0);
                        sb.datetime = reader.GetInt16(1);
                        UserGroup ug = new UserGroup();
                        ug.users

                    }
                }
            }
            return gameinfo;
        }



        public User GetUserSourceInfo(int user_id)
        {
            User us = new User();

            string sql = $"SELECT users.id, users.name, sources.id, upload_time FROM users,sources WHERE sources.user_id=users.id AND users.id={user_id} AND used=1 AND sources.status='ok'";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        us.id = reader.GetInt16(0);
                        us.name = reader.GetString(1);
                        us.compiledAndUsedSourceId = reader.GetInt16(3);
                        us.last_upload_time = reader.GetInt16(4);
                    }
                }
            }
            return us;
        }





    }
}
