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

        public class Header
        {
            public ushort const1;
            public ushort const2;
            public ushort pointerOffset;
            public ushort textOffset;
            public int entryCount;
        }

        private Header header = new Header();
        
        private string Dict(string toPO)
        {
            string result = "";
            Dictionary<char, char> dic = new Dictionary<char, char>
            {
                //Spanish localization
                { '\u00F1', '\u30F3' }, //ン from ñ
                { '\u00E4', '\u30D9' }, //ベ from ä
                { '\u00E1', '\u30E9' }, //ラ from á
                { '\u00EF', '\u30DA' }, //ペ from ï
                { '\u00ED', '\u30EA' }, //リ from í
                { '\u00FC', '\u30DB' }, //ホ from ü
                { '\u00FA', '\u30EB' }, //ル from ú
                { '\u00EB', '\u30DC' }, //ボ from ë
                { '\u00E9', '\u30EC' }, //レ from é
                { '\u00F6', '\u30DD' }, //ポ from ö
                { '\u00F3', '\u30ED' }, //ロ from ó
                { '\u00C4', '\u30A1' }, //ァ from Ä
                { '\u00C1', '\u30A2' }, //ア from Á
                { '\u00CF', '\u30A3' }, //ィ from Ï
                { '\u00CD', '\u30A4' }, //イ from Í
                { '\u00DC', '\u30A5' }, //ゥ from Ü
                { '\u00DA', '\u30A6' }, //ウ from Ú
                { '\u00CB', '\u30A7' }, //ェ from Ë
                { '\u00C9', '\u30A8' }, //エ from É
                { '\u00D6', '\u30A9' }, //ォ from Ö
                { '\u00D3', '\u30AA' }, //オ from Ó
                { '\u00A1', '\u30D1' }, //パ from ¡
                { '\u00BF', '\u30D7' }, //プ from ¿
            };
            if (toPO.Contains("{FF01}"))
            {
                toPO = toPO.Replace("{FF01}", "\n");
            }
            if (toPO.Contains("\n"))
            {
                toPO = toPO.Replace("\n", "{FF01}");
            }
            char[] dictArray = toPO.ToCharArray();
            foreach (char c in dictArray)
            {
                if (dic.ContainsKey(c)) //If the dic has a char available in the string, replace it
                {
                    result += dic[c];
                }
                else //If not, just construct the string like normal
                {
                    result += c;
                }
            }
            return result;
        }

        private byte[] CCI(string toByte) //ControlCodeInterpreter
        {
            var byteBuffer = new List<byte>();
            byte[] strArray = Encoding.UTF8.GetBytes(toByte);
            byte[] strNewArray;
            for (int i = 0; i < strArray.Length; i++)
            {
                byte c = strArray[i];
                if (c == 0x7B || c == 0x7D)
                {
                    byte[] chrArray0 = { strArray[i + 1], strArray[i + 2] };
                    byte[] chrArray1 = { strArray[i + 3], strArray[i + 4] };
                    string chars0 = Encoding.UTF8.GetString(chrArray0);
                    string chars1 = Encoding.UTF8.GetString(chrArray1);
                    if (chars0 == "FF" || chars0 == "00" || chars0 == "01" || chars0 == "02" || chars0 == "03" || chars0 == "04" || chars0 == "05" || chars0 == "06" || chars0 == "07" ||
                        chars0 == "08" || chars0 == "09" || chars0 == "10" || chars0 == "11" || chars0 == "12" || chars0 == "13" || chars0 == "0A" || chars0 == "0B" || chars0 == "0C" || 
                        chars0 == "0D" || chars0 == "0E" || chars0 == "0F" || chars0 == "C5" || chars0 == "C7" || chars0 == "CA" || chars0 == "D0")
                    {
                        var byte0 = Convert.ToByte("0x" + chars0, 16);
                        var byte1 = Convert.ToByte("0x" + chars1, 16);
                        byteBuffer.Add(byte0);
                        byteBuffer.Add(byte1);
                        i = i + 5;
                    }
                    else
                        i++;
                }
                else
                {
                    byteBuffer.Add(c);
                }
            }
            return strNewArray = byteBuffer.ToArray();
        }

        public void POExport(string toPO, int i)
        {
            poYarhl.Add(new PoEntry(Dict(toPO)) { Context = i.ToString() });
        }

        public void POWrite(string file)
        {
            poYarhl.ConvertTo<BinaryFormat>().Stream.WriteTo(file + ".po");
        }

        public void POImport(string orgFile, string poFile)
        {
            using (var br = new BinaryReader(File.Open(orgFile, FileMode.Open)))
            {
                header.const1 = br.ReadUInt16();
                header.const2 = br.ReadUInt16();
                header.pointerOffset = br.ReadUInt16();
                header.textOffset = br.ReadUInt16();
                header.entryCount = (header.textOffset - header.pointerOffset) / sizeof(short);
                var pointer = new List<ushort>();
                br.BaseStream.Position = header.pointerOffset;
                for (int j = 0; j < header.entryCount; j++)
                {
                    var textP = br.ReadUInt16();
                    if (textP == 0 && j == 0) pointer.Add(textP);
                    if (textP != 0 && j != 0) pointer.Add(textP);
                }
                header.entryCount = pointer.Count;
            }
            

            File.Copy(orgFile, Path.GetFileNameWithoutExtension(orgFile) + ".bkp", true); // Copy feedback file for backup before modifying orgFile with poFile data
            var poInstance = new BinaryFormat(new DataStream(poFile, FileOpenMode.Read)).ConvertTo<Po>();
            var fs = new FileStream(orgFile, FileMode.Open);
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.BaseStream.Position = header.pointerOffset + 2;
                int pointer = 0;

                for (int i = 0; i < header.entryCount - 1; i++)
                {
                    pointer = CCI(Dict(poInstance.Entries[i].Text)).Length + pointer;
                    bw.Write((ushort)pointer);
                }

                bw.BaseStream.Position = header.textOffset;
                foreach (var entry in poInstance.Entries)
                {
                    bw.Write(CCI(Dict(entry.Text)));
                }
                fs.SetLength(bw.BaseStream.Position);
            }
                   
        }
    }
}
