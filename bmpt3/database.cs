using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmpt3
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public int FailedAttempts { get; set; }
    }

    public static class Database
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static UserInfo GetUserByLogin(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT user_id, login, role, is_blocked, failed_attempts
                    FROM dbo.users
                    WHERE login = @login";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@login", login);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new UserInfo
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Login = reader["login"].ToString(),
                            Role = reader["role"].ToString(),
                            IsBlocked = Convert.ToBoolean(reader["is_blocked"]),
                            FailedAttempts = Convert.ToInt32(reader["failed_attempts"])
                        };
                    }
                }
            }
        }

        public static bool CheckPassword(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM dbo.users
                    WHERE login = @login AND password = @password";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static void AddFailedAttempt(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE dbo.users
                    SET failed_attempts = failed_attempts + 1,
                        is_blocked = CASE
                            WHEN failed_attempts + 1 >= 3 THEN 1
                            ELSE is_blocked
                        END
                    WHERE user_id = @userId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ResetFailedAttempts(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE dbo.users
                    SET failed_attempts = 0
                    WHERE user_id = @userId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
