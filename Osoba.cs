using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WpfApp1kursak
{
    class Osoba
    {
        public int Id { get; set; }
        public string Tel { get; set; }
        public string Parol { get; set; }
        public byte Posada { get; set; }
        public byte RivDostupu { get; set; }

        public override string ToString()
        {
            return Tel; // Щоб у ListBox відображалося Tel користувача
        }
    }
}
