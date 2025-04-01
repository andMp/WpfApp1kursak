using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1kursak
{
    class User
    {
        public int id { get; set; }
        public long tel { get; set; }
        public string par {  get; set; }
        public User() { }
        public User(long te, string pa)
        {
            this.tel = te;
            this.par = pa;
        }
    }
}
