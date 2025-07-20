using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp1kursak
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Виникла помилка:\n" + e.Exception.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // щоб програма не закривалась
        }
    }

}
