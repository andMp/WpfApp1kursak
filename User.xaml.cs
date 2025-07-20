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
using System.Windows.Threading;
using Newtonsoft.Json;

namespace WpfApp1kursak
{
    /// <summary>
    /// Interaction logic for User.xaml
    /// </summary>

    public partial class User : Page
    {
       
        string connectionString = "Server=WIN-DVNHOAUCHN7;Database=Opituvanna;Integrated Security=True;";
        private Osoba currentUser;
        private Dictionary<string, Opituvanna> slovnOp;
        private Opituv activeOpituv = null;
        private int currentQuestionIndex = 0;
        private DispatcherTimer timer;
        private TimeSpan timeLeft;

        public User(string id)
        {
            try
            {
                InitializeComponent();
                currentUser = GetUserFromDatabase(id);

                if (currentUser != null)
                {
                    this.id.Text = $"Особистий кабінет користувача з телефоном: {currentUser.Tel}";
                    telBox.Text = currentUser.Tel;
                    parBox.Text = currentUser.Parol;
                    posadaBox.Text = GetPosadaText(currentUser.Posada);
                    rivDostBox.Text = currentUser.RivDostupu.ToString();
                    slovnOp = GetOpituvFromDatabase();
                    OpitList.ItemsSource = slovnOp.Values.Where(x => x != null && x.Op != null).SelectMany(x => x.Op).ToList();
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

        public Opituvanna DeserializeOpituv(string json)
        {
            return JsonConvert.DeserializeObject<Opituvanna>(json);
        }

        private Osoba GetUserFromDatabase(string tel)
        {
            Osoba user = null;
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
                            Parol = reader.GetString(reader.GetOrdinal("Par")),
                            Posada = (byte)reader["Posada"],
                            RivDostupu = (byte)reader["RivDost"]
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
                1 => "Адміністратор",
                2 => "Користувач",
                _ => "Невідома посада"
            };
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentUser == null)
                    return;

                string newTel = telBox.Text;
                string newParol = parBox.Text;

                string updateQuery = "UPDATE Users SET Tel = @tel, Par = @parol WHERE Id = @id";

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

                        id.Text = $"Особистий кабінет користувача з телефоном: {currentUser.Tel}";
                    }
                    else
                    {
                        MessageBox.Show("Помилка при збереженні змін.");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Помилка " + ex.Message); }
        }

        private void goToOpituvClick(object sender, RoutedEventArgs e)
        {
            if (OpitList.SelectedItem is Opituv selectedOp)
            {
                activeOpituv = selectedOp;
                currentQuestionIndex = 0;

                temaOp.Text = activeOpituv.Tema;
                ShowCurrentQuestion();

                middleColumn.IsEnabled = false;

                timeLeft = TimeSpan.FromMinutes(
    activeOpituv.Pitanni
        .Where(p => int.TryParse(p.TrivPit, out _))
        .Sum(p => int.Parse(p.TrivPit)));

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeLeft.TotalSeconds > 0)
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
                zalChas.Text = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                timer.Stop();
                MessageBox.Show("Час опитування вичерпано!");
                endGol.IsEnabled = true;
            }
        }

        private void ShowCurrentQuestion()
        {
            if (activeOpituv == null || activeOpituv.Pitanni == null || !activeOpituv.Pitanni.Any())
                return;

            var pit = activeOpituv.Pitanni[currentQuestionIndex];

            nomPit.Text = $"Питання {currentQuestionIndex + 1}/{activeOpituv.Pitanni.Count}";
            this.pit.Text = pit.Pitan;

            string status = pit.Vidp switch
            {
                0 => "Ні",
                1 => "Так",
                2 => "Не вклались в час",
                _ => "-"
            };

            vidpNaPit.Text = $"Відповідь: {status}";
        }
        private void CheckIfAllAnswered()
        {
            if (activeOpituv.Pitanni.All(p => p.Vidp >= 0 && p.Vidp <= 2))
            {
                endGol.IsEnabled = true;
            }
        }
        private void AnswerQuestion(int answer)
        {
            if (activeOpituv == null) return;

            activeOpituv.Pitanni[currentQuestionIndex].Vidp = answer;
            ShowCurrentQuestion();
            CheckIfAllAnswered();
        }

        private void PrevQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestionIndex > 0)
            {
                currentQuestionIndex--;
                ShowCurrentQuestion();
            }
        }
        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestionIndex < activeOpituv.Pitanni.Count - 1)
            {
                currentQuestionIndex++;
                ShowCurrentQuestion();
            }
        }

        private void endGol_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timer?.Stop();
                foreach (var pyt in activeOpituv.Pitanni)
                {
                    if (pyt.Vidp != -1)
                    {
                        pyt.KstVidp += 1;
                    }
                }

                string serialized = JsonConvert.SerializeObject(activeOpituv);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string updateQuery = "UPDATE Users SET Opit = @opit WHERE Tel = @tel";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@opit", serialized);
                        cmd.Parameters.AddWithValue("@tel", currentUser.Tel);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Опитування завершене та збережене.");
                endGol.IsEnabled = false;
                middleColumn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при збереженні: " + ex.Message);
            }
        }

        private void yes_Click(object sender, RoutedEventArgs e) => AnswerQuestion(1);
        private void no_Click(object sender, RoutedEventArgs e) => AnswerQuestion(0);


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
