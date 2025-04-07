using System;
using System.Collections.Generic;
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
        public string TrivOp { get; set; }
        public string DataPoch { get; set; }
        public string DataZupin { get; set; }
        public string RivDost { get; set; }
        public List<Pytannia> Pitanni { get; set; }
    }
    public class Pytannia
    {
        public string Pitan { get; set; }
        public string TrivPit { get; set; }
        public string Vidp { get; set; }
        public string KstVidp { get; set; }
    }
}
