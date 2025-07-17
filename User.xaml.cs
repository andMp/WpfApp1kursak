using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for User.xaml
    /// </summary>

    public partial class User : Page
    {
        private Osoba currentUser;

        public User(string id)
        {
            try
            {

                InitializeComponent();
                currentUser = GetUserFromDatabase(id);

                //rivDostBox.Text = GetRivDostupText(_korystuvach.RivDostupu);

                if (currentUser != null)
                {
                    this.id.Text = $"Особистий кабінет користувача з телефоном: {currentUser.Tel}";
                    telBox.Text = currentUser.Tel;
                    parBox.Text = currentUser.Parol;
                    posadaBox.Text = GetPosadaText(currentUser.Posada);
                    rivDostBox.Text = currentUser.RivDostupu.ToString();
                }
                else
                {
                    MessageBox.Show("Користувача не знайдено!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }
        private Osoba GetUserFromDatabase(string tel)
        {
            Osoba user = null;
            string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";

            string query = "SELECT * FROM Users WHERE Tel = @tel";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tel", tel);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new Osoba
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Tel = reader.GetString(reader.GetOrdinal("Tel")),
                            Parol = reader.GetString(reader.GetOrdinal("Parol")),
                            Posada = (byte)reader["Posada"],
                            RivDostupu = (byte)reader["RivDostupu"]
                        };
                    }
                }
            }

            return user;
        }

        private string GetPosadaText(byte posada)
        {
            return posada switch
            {
                0 => "Користувач",
                1 => "Адміністратор",
                _ => "Невідома посада"
            };
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
                return;

            string newTel = telBox.Text;
            string newParol = parBox.Text;

            string connectionString = @"Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DB_NAME;Integrated Security=True";
            string updateQuery = "UPDATE Osoba SET Tel = @tel, Parol = @parol WHERE Id = @id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@tel", newTel);
                command.Parameters.AddWithValue("@parol", newParol);
                command.Parameters.AddWithValue("@id", currentUser.Id);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Зміни збережено.");
                    currentUser.Tel = newTel;
                    currentUser.Parol = newParol;
                }
                else
                {
                    MessageBox.Show("Помилка при збереженні змін.");
                }
            }
        }


        //private string GetRivDostupText(byte dost)
        //{
        //    return dost switch
        //    {
        //        0 => "Низький",
        //        1 => "Середній",
        //        2 => "Високий",
        //        _ => "Невідомий"
        //    };
        //}

        private void OpitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                //temaOp.Text = selectedOp.Tema;
                //trivOp.Text = selectedOp.TrivOp;

                //if (DateTime.TryParse(selectedOp.DataPoch, out DateTime dp))
                //    datPochOp.SelectedDate = dp;

                //if (DateTime.TryParse(selectedOp.DataZupin, out DateTime dz))
                //    datZaverOp.SelectedDate = dz;

                //rivDostOp.Text = selectedOp.RivDost;

                //PitanVsogo = selectedOp.Pitanni?.Count ?? 0;
                //PitanVRob = 0;

                //OnovytyPerehliadPytannia(selectedOp);
            }
        }

        private void UserList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = FindVisualParent<ScrollViewer>(sender as DependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true; // Щоб не було подвійної прокрутки
            }
        }
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
