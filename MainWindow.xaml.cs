using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;  // Необхідно додати

namespace WpfApp1kursak
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReestrButClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame != null)
            {
                //SecondGrid.Visibility = Visibility.Visible;// Показати
                SecondGrid.Visibility = Visibility.Collapsed;// Сховати
                MainFrame.Content = null;
                MainFrame.Navigate(new Reestr());
            }
        }

        private void VhidClick(object sender, RoutedEventArgs e)
        {
            string phoneText = tel.Text?.Trim();
            string password = par.Password?.Trim();

            if (string.IsNullOrEmpty(phoneText) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            phoneText = System.Text.RegularExpressions.Regex.Replace(phoneText, @"\D", "");
            try
            {
                RobotaZdb db = new RobotaZdb();
                int Posada;
                bool loginSuccess = db.CheckLogin(phoneText, password, out Posada);

                if (loginSuccess)
                {
                    MessageBox.Show("Вхід успішний!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (Posada == 1)
                    {
                        //Admin adminPage = new Admin();
                        //MainFrame.Navigate(adminPage);
                        MessageBox.Show("Заходимо на адміна!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        SecondGrid.Visibility = Visibility.Collapsed;// Сховати
                        MainFrame.Content = null;
                        MainFrame.Navigate(new Admin(phoneText));
                    }
                    else
                    {
                        User userPage = new User();
                        MainFrame.Navigate(userPage);
                    }
                }
                else
                {
                    MessageBox.Show("Невірний номер телефону або пароль!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL помилка: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
