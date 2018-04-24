using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;

namespace AutoCompiler
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
            string sql = $"UPDATE sources SET error='', status='ok', used=1 WHERE id=@id";

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




    }
}
