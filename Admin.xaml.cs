using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1kursak
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        private RobotaZdb db = new RobotaZdb();
        private List<string> users = new List<string>(); // Список телефонів
        public Admin(string p)
        {
            InitializeComponent();
            id.Text = p;
            LoadUsers();
        }
        private void LoadUsers()
        {
            users = db.GetUserList(); // Отримуємо телефони користувачів
            UserList.Items.Clear();
            foreach (var user in users)
            {
                UserList.Items.Add(user); // Додаємо у ListBox
            }
        }

        // Подія при виборі користувача у списку
        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem != null)
            {
                string phone = UserList.SelectedItem.ToString();
                SelectedUserPhone.Text = phone;

                string info = db.GetUserInfo(phone);
                SelectedUserInfo.Text = info;
            }
        }
    }

}
