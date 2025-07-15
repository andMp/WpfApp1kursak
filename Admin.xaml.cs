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
        string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
        private List<Osoba> users;

        private Dictionary<string, Opituvanna> slovnOp;
        private int PitanVRob;
        private int PitanVsogo;
        private string idAdmina;

        public Admin(string id)
        {
            InitializeComponent();
            LoadUsers();
            LoadOpituv();
            idAdmina = id;
        }

        private void LoadUsers()
        {
            users = GetUsersFromDatabase();
            UserList.ItemsSource = users;
        }
        private void LoadOpituv()
        {
            slovnOp = GetOpituvFromDatabase();
            OpitList.ItemsSource = slovnOp.Values.Where(x => x != null && x.Op != null).SelectMany(x => x.Op).ToList();
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

        private void OpitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (OpitList.SelectedItem is Opituv selectedOp)
                {
                    MessageBox.Show("Опитування вибрано: " + (selectedOp?.Tema ?? "null"));

                    temaOp.Text = selectedOp.Tema;
                    trivOp.Text = selectedOp.TrivOp;
                    if (DateTime.TryParse(selectedOp.DataPoch, out DateTime dp))
                        datPochOp.SelectedDate = dp;
                    if (DateTime.TryParse(selectedOp.DataZupin, out DateTime dz))
                        datZaverOp.SelectedDate = dz;
                    rivDostOp.Text = selectedOp.RivDost;

                    if (selectedOp.Pitanni == null)
                    {
                        MessageBox.Show("Pitanni є NULL. Створюємо новий список.");
                        selectedOp.Pitanni = new List<Pytannia>();
                    }

                    vsogo.Text = selectedOp.Pitanni.Count.ToString();
                    PitanVsogo = selectedOp.Pitanni.Count;

                    if (selectedOp.Pitanni.Count > 0)
                    {
                        PitanVRob = 0;
                        diysn.Text = "1";
                        poleVivedPitan.Text = selectedOp.Pitanni[PitanVRob].Pitan;
                        chasNaVidpVidobr.Text = selectedOp.Pitanni[PitanVRob].TrivPit;
                    }
                    else
                    {
                        diysn.Text = "0";
                        PitanVRob = 0;
                        poleVivedPitan.Text = "-";
                        chasNaVidpVidobr.Text = "-";
                    }
                }
                else
                {
                    MessageBox.Show("OpitList.SelectedItem не є Opituv");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Помилка при виборі опитування: " + ex.Message);
            }
        }


        private Dictionary<string, Opituvanna> GetOpituvFromDatabase()
        {
            Dictionary<string, Opituvanna> us2 = new Dictionary<string, Opituvanna>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Tel, Opit FROM Users WHERE Posada = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            us2.Add(reader.GetString(0), reader.IsDBNull(1) ? null : DeserializeOpituv(reader.GetString(1)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка в GetOpituvFromDatabase: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return us2;
        }

        private void ZberZmOpitButClick(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                selectedOp.Tema = temaOp.Text;
                selectedOp.TrivOp = trivOp.Text;
                selectedOp.DataPoch = datPochOp.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                selectedOp.DataZupin = datZaverOp.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
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

        private void stvorNovTemOpit(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Починаємо створювати опитув.1");
                string newTemaOpit = vvedNovTem.Text?.Trim();
                if (string.IsNullOrEmpty(newTemaOpit))
                {
                    MessageBox.Show("Будь ласка, введіть тему опитування!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (slovnOp[idAdmina] != null && slovnOp[idAdmina].Op.Any(x => x.Tema == newTemaOpit))
                {
                    MessageBox.Show("Опитування на дану тему вже існує!\tПідберіть іншу тему", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Opituv novOp = new Opituv();
                novOp.Tema = newTemaOpit;
                novOp.Telef = idAdmina;
                if (slovnOp.TryGetValue(novOp.Telef, out Opituvanna opituvanna))
                {
                    if (opituvanna == null)
                    {
                        opituvanna = new Opituvanna();
                        slovnOp[novOp.Telef] = opituvanna;
                    }
                    if (opituvanna.Op == null)
                    {
                        opituvanna.Op = new List<Opituv>();
                    }
                    opituvanna.Op.Add(novOp);
                }
                else
                {
                    slovnOp[novOp.Telef] = new Opituvanna
                    {
                        Op = new List<Opituv> { novOp }
                    };
                }
                UpdateOpituvInDatabase(novOp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сталася помилка в stvorNovTemOpit: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



