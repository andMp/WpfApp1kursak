using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme;

namespace WpfApp1kursak
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //MainFrame.Navigate(new Dim()); // Відкриваємо головну сторінку при запуску
        }
        private void ReestrButClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigate(new Reestr());
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
                        AdminPage adminPage = new AdminPage();
                        this.NavigationService.Navigate(adminPage); // Перехід на сторінку адміністратора
                    }
                    else
                    {
                        UserPage userPage = new UserPage();
                        this.NavigationService.Navigate(userPage); // Перехід на сторінку користувача
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
    }
}