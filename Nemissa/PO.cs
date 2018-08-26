using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Yarhl.IO;
using Yarhl.Media.Text;
using Yarhl.FileFormat;
using System.Threading.Tasks;

namespace Nemissa
{
    class PO
    {
        Po poYarhl = new Po
        {
            Header = new PoHeader("Shin Megami Tensei: Devil Summonner: Soul Hackers", "tradusquare@gmail.com", "es")
            {
                LanguageTeam = "TraduSquare",
            }
        };

        public void POEx(string toPO, int i)
        {
            poYarhl.Add(new PoEntry(toPO) { Context = i.ToString() });
        }

        public void PoWrite(string file)
        {
            poYarhl.ConvertTo<BinaryFormat>().Stream.WriteTo(file + ".po");
        }
        
    }
}
