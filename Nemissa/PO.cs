namespace Nemissa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Yarhl.Media.Text;

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
