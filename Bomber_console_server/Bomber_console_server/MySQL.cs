using System;
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
        static string binDir = MyPath.binDir;
        static string exe_file_name = MyPath.exe_file_name;
        

        public MySQL()
        {          
            myConnection = new MySqlConnection(Connect);
            myConnection.Open();    
        }


        public UserGroup GetUsersInGroup(int group_id)
        {
            UserGroup ug = new UserGroup();

            string sql = $"Select group_id, user_id FROM users_group WHERE group_id={group_id}";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;           

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {                       
                        ug.group_id = reader.GetInt16(0);   
                        dbUser us = new dbUser();
                        us.id = reader.GetInt16(1);                       
                        ug.users.Add(us);
                    }
                }
            }

            for (int i = 0; i < ug.users.Count; i++)
            {
                ug.users[i] = GetUserSourceInfo(ug.users[i].id);
            }            
                        
            return ug;          
        }

        public List<Game> GetWaitGame(string type)
        {
            List<Game> games_list = new List<Game>();

            string sql = $"SELECT * FROM {type} WHERE status='wait'";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Game sb = new Game();
                        sb.id = reader.GetInt16(0);
                        sb.datetime = reader.GetInt32(1);
                        UserGroup ug = new UserGroup();
                        ug.group_id = reader.GetInt16(2);                        
                        sb.usergroup = ug;
                        sb.status = reader.GetString(3);
                        if (type == "sandbox")
                        {
                            dbUser us = new dbUser();
                            us.id = reader.GetInt16(4);
                            sb.creator = us;
                        }
                        games_list.Add(sb);
                    }
                }
            }

            for (int i = 0; i < games_list.Count; i++)
            {
                games_list[i].usergroup = GetUsersInGroup(games_list[i].usergroup.group_id);
                games_list[i].creator = GetUserSourceInfo(games_list[i].creator.id);
            }

            return games_list;
        }





        public dbUser GetUserSourceInfo(int user_id)
        {
            dbUser us = new dbUser();

            string sql = $"SELECT users.id, users.name, sources.id, sources.upload_time FROM users,sources WHERE sources.user_id=users.id AND users.id={user_id} AND used=1 AND sources.status='ok'";

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
                        us.compiledAndUsedSourceId = reader.GetInt16(2);
                        us.last_upload_time = reader.GetInt32(3);                       
                    }
                }
            }
            return us;
        }


        public List<dbUser> GetOnlyUsers(int group_id)
        {
            List<dbUser> users = new List<dbUser>();

            string sql = $"SELECT users.id FROM users, users_group WHERE users_group.group_id={group_id} AND users_group.user_id = users.id AND users.is_bot=1";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        dbUser us = new dbUser();
                        us.id = reader.GetInt16(0);
                        users.Add(us);              
                    }
                }
            }
            return users;
        }

        public bool CheckUserIsBot(int user_id)
        {
            List<dbUser> users = new List<dbUser>();

            string sql = $"SELECT users.id FROM users WHERE users.id = @id AND users.is_bot=1";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = user_id;


            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    return true;
                }
            }
            return false;
        }



        public bool SetGameWorkStatus(int id, string type)
        {
            //   string sql = "UPDATE sources SET error='', exe_path='', status=@status WHERE user_id=@user_id";
            string sql = $"UPDATE {type} SET status='work' WHERE id=@id";

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



        public bool SetGameCompiledStatus(int id, string type, string gameresult)
        {
            string sql = $"UPDATE {type} SET result='{gameresult}', errors='', status='ok' WHERE id=@id";

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


        public bool SetGameErrorStatus(int id, string type, string error)
        {
            string sql = $"UPDATE {type} SET errors='{error}', status='error' WHERE id=@id";

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


        public bool AddUserRatingPoints(int user_id, int points)
        {
            string sql = $"UPDATE users SET points= points + {points} WHERE id=@id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = user_id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }

        public bool AddUserFinalPoints(int user_id, int points)
        {
            string sql = $"UPDATE final_points SET points= points + {points} WHERE user_id=@id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", SqlDbType.Int).Value = user_id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }


    }
}
