using Newtonsoft.Json;
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
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        //private RobotaZdb db = new RobotaZdb();
        //private List<string> users = new List<string>(); // Список телефонів

        //public Admin(string p)
        //{
        //    InitializeComponent();
        //    id.Text = p;
        //    LoadUsers();
        //}
        //private void LoadUsers()
        //{
        //    users = db.GetUserList(); // Отримуємо телефони користувачів
        //    UserList.Items.Clear();
        //    foreach (var user in users)
        //    {
        //        UserList.Items.Add(user); // Додаємо у ListBox
        //    }
        //}

        string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
        private List<Osoba> users;

        //private List<Opituv> Op2;
        private Dictionary<string, Opituvanna> slovnOp;
        //private Dictionary<string,Opituv> Op2;
        private int PitanVRob;
        private int PitanVsogo;

        public Admin()
        {
            InitializeComponent();
            LoadUsers();
            LoadOpituv();
        }

        private void LoadUsers()
        {
            users = GetUsersFromDatabase();
            UserList.ItemsSource = users;
        }
        private void LoadOpituv()
        {
            //Op2 = GetOpituvFromDatabase();
            //Op = GetOpituvFromDatabase();
            //OpitList.ItemsSource = Op2;
            slovnOp = GetOpituvFromDatabase();
            OpitList.ItemsSource = slovnOp.Values.SelectMany(x => x.Op).ToList();

        }

        private List<Osoba> GetUsersFromDatabase()
        {
            List<Osoba> us = new List<Osoba>();
            //string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Tel, Par, Posada, RivDost FROM Users";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        us.Add(new Osoba
                        {
                            Id = reader.GetInt32(0),
                            Tel = reader.GetString(1),
                            Parol = reader.GetString(2),
                            Posada = reader.GetByte(3),
                            RivDostupu = reader.GetByte(4)
                        });
                    }
                }
            }
            return us;
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem is Osoba selectedUser)
            {
                telAdm.Text = selectedUser.Tel;
                parAdm.Text = selectedUser.Parol;
                posAdm.Text = selectedUser.Posada.ToString();
                rivAdm.Text = selectedUser.RivDostupu.ToString();
                //if (selectedUser.Posada == 1)
                //{
                //    GetOpituvFromDatabase(selectedUser.Tel);
                //}
            }
        }

        private void OpitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpitList.SelectedItem is KeyValuePair<string, Opituv> selectedOp)
            {
                temaOp.Text = selectedOp.Value.Tema;
                trivOp.Text = selectedOp.Value.TrivOp;
                datPochOp.Text = selectedOp.Value.DataPoch;
                datZaverOp.Text = selectedOp.Value.DataZupin;
                rivDostOp.Text = selectedOp.Value.RivDost;

                diysn.Text = selectedOp.Value.Pitanni.Count == 0 ? "0" : "1";
                PitanVRob = 1;
                vsogo.Text = selectedOp.Value.Pitanni.Count == 0 ? "0" : selectedOp.Value.Pitanni.Count.ToString();
                PitanVsogo = selectedOp.Value.Pitanni.Count;

                poleVivedPitan.Text = selectedOp.Value.Pitanni.Count > 0 ? selectedOp.Value.Pitanni[0].Pitan : "-";
                chasNaVidpVidobr.Text = selectedOp.Value.Pitanni.Count > 0 ? selectedOp.Value.Pitanni[PitanVRob].TrivPit : "-";
            }
        }

        private void ZberZmKor(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is Osoba selectedUser)
            {
                selectedUser.Tel = telAdm.Text;
                selectedUser.Parol = parAdm.Text;

                if (byte.TryParse(posAdm.Text, out byte posada) && byte.TryParse(rivAdm.Text, out byte rivDostupu))
                {
                    selectedUser.Posada = posada;
                    selectedUser.RivDostupu = rivDostupu;

                    UpdateUserInDatabase(selectedUser);
                    MessageBox.Show("Зміни збережені!");
                }
                else
                {
                    MessageBox.Show("Помилка! Поля 'Посада' і 'Рівень доступу' повинні бути числами від 0 до 5.",
                                    "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UpdateUserInDatabase(Osoba user)
        {
            //string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET Tel = @Imya, Par = @Parol, Posada = @Posada, RivDost = @RivDostupu WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Imya", user.Tel);
                    cmd.Parameters.AddWithValue("@Parol", user.Parol);
                    cmd.Parameters.AddWithValue("@Posada", user.Posada);
                    cmd.Parameters.AddWithValue("@RivDostupu", user.RivDostupu);
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.ExecuteNonQuery();
                }
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

        // Допоміжний метод для пошуку батьківського ScrollViewer
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


        private void VidOblZapButClick(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is Osoba selectedUser)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Ви дійсно хочете видалити користувача {selectedUser.Tel}?",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Users WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", selectedUser.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    users.Remove(selectedUser);
                    UserList.ItemsSource = null;
                    UserList.ItemsSource = users;
                    MessageBox.Show("Користувач успішно видалений.", "Видалення", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Оберіть користувача для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public string SerializeOpituv(Opituvanna opituv)
        {
            return JsonConvert.SerializeObject(opituv);
        }

        public Opituvanna DeserializeOpituv(string json)
        {
            return JsonConvert.DeserializeObject<Opituvanna>(json);
        }

        //##########################################################################################################################
        //##########################################################################################################################
        //##########################################################################################################################

        //private List<Opituv> GetOpituvFromDatabase()
        private Dictionary<string, Opituvanna> GetOpituvFromDatabase()
        {
            //Dictionary<string, string> slovn1 = new Dictionary<string, string>();
            //List<Opituv> slovn = new List<Opituv>();

            //List<Opituvanna> us = new List<Opituvanna>();
            //Opituvanna us = new Opituvanna();
            //List<Opituv> us2 = new List<Opituv>();
            Dictionary<string, Opituvanna> us2 = new Dictionary<string, Opituvanna>();

            //string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Tel, Opit FROM Users WHERE Tel = 2";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //cmd.Parameters.AddWithValue("@Tel", Tel);
                    while (reader.Read())
                    {
                        us2.Add(reader.GetString(0), reader.IsDBNull(1) ? null : DeserializeOpituv(reader.GetString(1)));
                    }
                }
            }
            return us2;
        }

        private void ZberZmOpitButClick(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                selectedOp.Tema = temaOp.Text;
                selectedOp.TrivOp = trivOp.Text;
                selectedOp.DataPoch = datPochOp.Text;
                selectedOp.DataZupin = datZaverOp.Text;
                selectedOp.RivDost = rivDostOp.Text;

                UpdateOpituvInDatabase(selectedOp);
                MessageBox.Show("Зміни збережені!");
            }
        }

        private void UpdateOpituvInDatabase(Opituv op)
        {
            //Opituvanna opit=new Opituvanna();
            ////newOp = slovnOp[op.Telef].Op.Where(o => o.Tema == op.Tema);
            ////slovnOp[op.Telef].Op[slovnOp[op.Telef].Op.FindIndex(o => o.Tema == op.Tema)]=op;

            //opit.Op = slovnOp[op.Telef].Op;
            //int index = opit.Op.FindIndex(o => o.Tema == op.Tema);
            //if (index != -1)
            //{
            //    opit.Op[index] = op;
            //}

            //SerializeOpituv(opit);
            if (slovnOp.TryGetValue(op.Telef, out Opituvanna opit))
            {
                int index = opit.Op.FindIndex(o => o.Tema == op.Tema);
                if (index != -1)
                {
                    opit.Op[index] = op;

                    // Зберігаємо назад у словник — хоча посилання те саме, це необов'язково, але можна явно
                    slovnOp[op.Telef] = opit;

                    // Серіалізуємо вже оновлений словник або об'єкт
                    string serOpituv = SerializeOpituv(opit);
                    //string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "UPDATE Users SET Opit = @Opit WHERE Tel = @Tel";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Opit", serOpituv);
                            cmd.Parameters.AddWithValue("@Tel", op.Telef);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Зміни збережені!");
                }
            }
        }

        private void DodPitButClick(object sender, RoutedEventArgs e)
        {
            string pitannDlaOpit = dodPitOp.Text?.Trim();
            string poleChasNaVidpVkaz = chasNaVidpVkazati.Text?.Trim();

            if (string.IsNullOrEmpty(pitannDlaOpit) || string.IsNullOrEmpty(poleChasNaVidpVkaz))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Pytannia pit = new Pytannia();
            pit.Pitan = pitannDlaOpit;
            pit.TrivPit = poleChasNaVidpVkaz;

            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                selectedOp.Pitanni.Add(pit);
                UpdateOpituvInDatabase(selectedOp);
                MessageBox.Show("Зміни збережені!");
            }
        }
    }
}



