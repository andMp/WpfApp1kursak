using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1kursak
{
    public class Opituvanna
    {
        ////public Dictionary<string, Opituvannya> Op { get; set; }
        public List<Opituv> Op { get; set; }
    }
    public class Opituv
    {
        public string Telef {get;set; }
        public string Tema { get; set; }
        //public string TrivOp { get; set; }
        public string DataPoch { get; set; }
        public string DataZupin { get; set; }
        public string RivDost { get; set; }
        //public List<Pytannia> Pitanni { get; set; } = new List<Pytannia>();
        public ObservableCollection<Pytannia> Pitanni { get; set; } = new ObservableCollection<Pytannia>();

        public string TrivOp
        {
            get
            {
                int totalMinutes = Pitanni
                    .Where(p => int.TryParse(p.TrivPit, out _))
                    .Sum(p => int.Parse(p.TrivPit));

                TimeSpan ts = TimeSpan.FromMinutes(totalMinutes);

                return $"{ts.Hours * 60 + ts.Minutes} хв {ts.Seconds} сек";
            }
        }
        public int KstPitann => Pitanni?.Count ?? 0;
    }
    public class Pytannia
    {
        public string Pitan { get; set; }
        public string TrivPit { get; set; }
        public int Vidp { get; set; } = -1;//0 - "ні", 1 - "так", 2 - "не вклались в час"
        public string VidpText
        {
            get
            {
                return Vidp switch
                {
                    -1 => "—",
                    0 => "Ні",
                    1 => "Так",
                    2 => "Не вклались у час",
                    _ => "?"
                };
            }
        }
        public int KstVidp { get; set; } = 0;
    }
}
