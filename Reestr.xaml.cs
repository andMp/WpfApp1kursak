using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1kursak
{
    public partial class Reestr : Page
    {
        public Reestr()
        {
            InitializeComponent();
        }

        private void ZarButtonClick(object sender, RoutedEventArgs e)
        {
            if (telBox == null || parBox == null)
            {
                MessageBox.Show("Помилка: Поля введення не знайдені!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string tel = telBox.Text?.Trim();
            string password = parBox.Password?.Trim();

            if (string.IsNullOrEmpty(tel) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Видаляємо всі символи, крім цифр
            tel = System.Text.RegularExpressions.Regex.Replace(tel, @"\D", "");

            // Перевіряємо довжину номера
            if (tel.Length < 1 || tel.Length > 15)
            {
                MessageBox.Show("Некоректний номер телефону! Введіть від 1 до 15 цифр.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                RobotaZdb vnes = new RobotaZdb();
                MessageBox.Show("Підключення до бази даних відбулося успішно.");

                bool isRegistered = vnes.ReestrUser(tel, password);

                if (isRegistered)
                {
                    MessageBox.Show("Реєстрація успішна!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Помилка реєстрації. Можливо, користувач вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("SQL помилка: " + sqlEx.Message, "SQL Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при реєстрації: " + ex.Message, "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NaGolClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
