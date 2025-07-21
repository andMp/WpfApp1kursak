using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            this.id.Text = id;
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

        private void OpitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                temaOp.Text = selectedOp.Tema;
                trivOp.Text = selectedOp.TrivOp;

                if (DateTime.TryParse(selectedOp.DataPoch, out DateTime dp))
                    datPochOp.SelectedDate = dp;

                if (DateTime.TryParse(selectedOp.DataZupin, out DateTime dz))
                    datZaverOp.SelectedDate = dz;

                rivDostOp.Text = selectedOp.RivDost;

                PitanVsogo = selectedOp.Pitanni?.Count ?? 0;
                PitanVRob = 0;

                OnovytyPerehliadPytannia(selectedOp);
                OnovytyStatystyku(selectedOp);
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
                selectedOp.DataPoch = datPochOp.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                selectedOp.DataZupin = datZaverOp.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                selectedOp.RivDost = rivDostOp.Text;

                UpdateOpituvInDatabase(selectedOp);
                MessageBox.Show("Зміни збережені!");
                OpitList.ItemsSource = null;
                OpitList.ItemsSource = slovnOp.Values
                    .Where(x => x != null && x.Op != null)
                    .SelectMany(x => x.Op)
                    .ToList();
            }
        }

        private void UpdateOpituvInDatabase(Opituv op)
        {
            if (slovnOp.TryGetValue(op.Telef, out Opituvanna opit))
            {
                int index = opit.Op.FindIndex(o => o.Tema == op.Tema);
                if (index != -1)
                {
                    opit.Op[index] = op;

                    slovnOp[op.Telef] = opit;

                    string serOpituv = SerializeOpituv(opit);
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

            Pytannia pit = new Pytannia
            {
                Pitan = pitannDlaOpit,
                TrivPit = poleChasNaVidpVkaz
            };

            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                if (selectedOp.Pitanni == null)
                    selectedOp.Pitanni = new ObservableCollection<Pytannia>();

                selectedOp.Pitanni.Add(pit);
                UpdateOpituvInDatabase(selectedOp);

                MessageBox.Show("Питання додано!");

                dodPitOp.Text = string.Empty;
                chasNaVidpVkazati.Text = string.Empty;

                PitanVsogo = selectedOp.Pitanni.Count;
                PitanVRob = PitanVsogo - 1;
                OnovytyPerehliadPytannia(selectedOp);
            }
            else
            {
                MessageBox.Show("Оберіть опитування перед додаванням питання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void OnovytyPerehliadPytannia(Opituv opituv)
        {
            if (opituv.Pitanni == null || opituv.Pitanni.Count == 0)
            {
                diysn.Text = "0";
                vsogo.Text = "0";
                poleVivedPitan.Text = "-";
                chasNaVidpVidobr.Text = "-";
                PerehPytUp.IsEnabled = false;
                PerehPytDown.IsEnabled = false;
                return;
            }

            if (PitanVRob < 0) PitanVRob = 0;
            if (PitanVRob >= opituv.Pitanni.Count) PitanVRob = opituv.Pitanni.Count - 1;

            diysn.Text = (PitanVRob + 1).ToString();
            vsogo.Text = opituv.Pitanni.Count.ToString();
            poleVivedPitan.Text = opituv.Pitanni[PitanVRob].Pitan;
            chasNaVidpVidobr.Text = opituv.Pitanni[PitanVRob].TrivPit;

            PerehPytUp.IsEnabled = PitanVRob > 0;
            PerehPytDown.IsEnabled = PitanVRob < opituv.Pitanni.Count - 1;
        }

        private void PerehPytUp_Click(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp && selectedOp.Pitanni?.Count > 0)
            {
                if (PitanVRob > 0)
                {
                    PitanVRob--;
                    OnovytyPerehliadPytannia(selectedOp);
                }
            }
        }
        private void PerehPytDown_Click(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp && selectedOp.Pitanni?.Count > 0)
            {
                if (PitanVRob < selectedOp.Pitanni.Count - 1)
                {
                    PitanVRob++;
                    OnovytyPerehliadPytannia(selectedOp);
                }
            }
        }

        private void stvorNovTemOpit(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Починаємо створювати опитування");
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
                OpitList.ItemsSource = null;
                OpitList.ItemsSource = slovnOp.Values
                    .Where(x => x != null && x.Op != null)
                    .SelectMany(x => x.Op)
                    .ToList();
                vvedNovTem.Text = string.Empty;
                MessageBox.Show("Опитування успішно створено!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сталася помилка в stvorNovTemOpit: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnovytyStatystyku(Opituv opituv)
        {
            if (opituv.Pitanni == null || opituv.Pitanni.Count == 0)
            {
                SetStatystyka("-", "-", "-");
                return;
            }

            int tak = 0;
            int ni = 0;
            int neVklylys = 0;

            foreach (var pyt in opituv.Pitanni)
            {
                switch (pyt.Vidp)
                {
                    case 1: tak += pyt.KstVidp; break;
                    case 0: ni += pyt.KstVidp; break;
                    case 2: neVklylys += pyt.KstVidp; break;
                }
            }
            SetStatystyka(tak.ToString(), ni.ToString(), neVklylys.ToString());
        }
        private void SetStatystyka(string tak, string ni, string nevklylys)
        {
            takCount.Text = tak;
            niCount.Text = ni;
            nevklylysCount.Text = nevklylys;
        }

        private void vidPitInActive_click(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                if (selectedOp.Pitanni == null || selectedOp.Pitanni.Count == 0)
                {
                    MessageBox.Show("В опитуванні немає питань для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBoxResult result = MessageBox.Show(
                    $"Ви дійсно хочете видалити поточне питання:\n\"{selectedOp.Pitanni[PitanVRob].Pitan}\"?",
                    "Підтвердження видалення питання",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedOp.Pitanni.RemoveAt(PitanVRob);

                        if (PitanVRob >= selectedOp.Pitanni.Count)
                            PitanVRob = selectedOp.Pitanni.Count - 1;

                        if (slovnOp.TryGetValue(selectedOp.Telef, out Opituvanna opituvanna))
                        {
                            int index = opituvanna.Op.FindIndex(o => o.Tema == selectedOp.Tema);
                            if (index != -1)
                            {
                                opituvanna.Op[index] = selectedOp;
                                slovnOp[selectedOp.Telef] = opituvanna;

                                string serOpituv = SerializeOpituv(opituvanna);
                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    conn.Open();
                                    string query = "UPDATE Users SET Opit = @Opit WHERE Tel = @Tel";
                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@Opit", serOpituv);
                                        cmd.Parameters.AddWithValue("@Tel", selectedOp.Telef);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        OnovytyPerehliadPytannia(selectedOp);
                        OnovytyStatystyku(selectedOp);

                        MessageBox.Show("Питання успішно видалено.", "Видалення", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка при видаленні питання: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Оберіть опитування та питання для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}



