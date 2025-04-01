using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1kursak
{
    class RobotaZdb
    {
        private readonly string connectionString;

        public RobotaZdb()
        {
            try
            {
                var connSetting = System.Configuration.ConfigurationManager.ConnectionStrings["toDB"];
                if (connSetting == null)
                {
                    throw new Exception("Рядок підключення 'toDB' не знайдено! Перевірте App.config.");
                }
                connectionString = connSetting.ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при зчитуванні підключення: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public bool ReestrUser(string phone, string password)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Помилка: Немає рядка підключення до бази даних!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Перевіряємо, чи існує користувач із таким номером
                    MessageBox.Show("Перевіряємо дублікати користувачів");

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Tel = @Phone";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar, 15).Value = phone;
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Користувач з таким номером телефону вже існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    MessageBox.Show("Додаємо нового користувача");

                    // Додаємо нового користувача
                    string query = "INSERT INTO Users (Tel, Par) VALUES (@Phone, @Password)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar, 15).Value = phone;
                        cmd.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar, 100).Value = password;

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка бази даних: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public bool CheckLogin(string phone, string password, out int Posada)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Par, Posada FROM Users WHERE Tel = @Phone";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = phone;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader.GetString(0); // Отримуємо пароль
                            Posada = reader.GetByte(1); // Отримуємо статус
                            return storedPassword == password; // Порівнюємо паролі
                        }
                        Posada = 0;
                    }
                }
            }
            return false; // Якщо користувача не знайдено
        }

    }
}
