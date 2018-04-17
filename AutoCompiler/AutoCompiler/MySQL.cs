using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace AutoCompiler
{
   public class MySQL
    {
       public MySqlConnection myConnection;
        string Connect = "Database=test;Data Source=127.0.0.1;User Id=root;Password=''";

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

        public void GetSource(out string source_path, out int user_id)
        {
            source_path = "";
            user_id = 0;
            string sql = "Select id, source_path FROM sources WHERE status='wait' LIMIT 1";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    source_path = reader.GetString(1);
                    user_id = reader.GetInt16(0);
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


        public bool SetWorkStatus(int user_id, string status)
        {
            //   string sql = "UPDATE sources SET error='', exe_path='', status=@status WHERE user_id=@user_id";
            string sql = $"UPDATE sources SET status='{status}' WHERE user_id=@user_id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@status", SqlDbType.VarChar).Value = status;
            cmd.Parameters.AddWithValue("@user_id", SqlDbType.Int).Value = user_id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows==1)
            {
                return true;
            }
            return false;
        }


        public bool SetCompiledStatus(int user_id, string exe_Path)
        {
            string sql = $"UPDATE sources SET error='', exe_path='{exe_Path}', status='compiled' WHERE user_id=@user_id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@user_id", SqlDbType.Int).Value = user_id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }


        public bool SetErrorStatus(int user_id, string error)
        {
            string sql = "UPDATE sources SET error=@error, exe_path='', status='error' WHERE user_id=@user_id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = myConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@error", SqlDbType.Text).Value = error;
            cmd.Parameters.AddWithValue("@user_id", SqlDbType.Int).Value = user_id;

            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
            {
                return true;
            }
            return false;
        }




    }
}
