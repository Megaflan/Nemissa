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
            Dictionary<int, int> dic = new Dictionary<int, int>
            {
                //Spanish localization
                { 0x00F1, 0xEDEC }, //ñ
                { 0x00D1, 0xEDED }, //Ñ
                { 0x00E1, 0xEDE1 }, //á
                { 0x00ED, 0xEDE3 }, //í
                { 0x00FC, 0xEDEB }, //ü
                { 0x00FA, 0xEDE5 }, //ú
                { 0x00E9, 0xEDE7 }, //é
                { 0x00F3, 0xEDE9 }, //ó
                { 0x00C1, 0xEDE2 }, //Á
                { 0x00CD, 0xEDE4 }, //Í
                { 0x00DA, 0xEDE6 }, //Ú
                { 0x00C9, 0xEDE8 }, //É
                { 0x00D3, 0xEDEA }, //Ó
                { 0x00A1, 0xEDEE }, //¡
                { 0x00BF, 0xEDEF }, //¿
            };
            Dictionary<string, string> dicCC = new Dictionary<string, string>
            {
                //Control Code Sanitation
                { "{FF01}", "\n" },
                { "{C7B7}", "[" },
                { "{C7B8}", "]" },
                { "{FF19}", "<BC>" },
                { "{FF16}", "<WC>" },
                { "{FF03}{FF02}", "<NT>" },
                { "{FF81}", "<NAME>" },
                { "{FF82}", "<SUBNAME>" },
                { "{FF00}", "<END>" },
                //Reverse
                { "\n", "{FF01}" },
                { "[", "{C7B7}" },
                { "]", "{C7B8}" },
                { "<BC>", "{FF19}" },
                { "<WC>", "{FF16}" },
                { "<NT>", "{FF03}{FF02}" },
                { "<NAME>", "{FF81}" },
                { "<SUBNAME>", "{FF82}" },
                { "<END>", "{FF00}" },
            };
            char[] dictArray = toPO.ToCharArray();
            foreach (char c in dictArray)
            {
                if (dic.ContainsKey(c)) //If the dic has a char available in the string, replace it
                {
                    dic.TryGetValue(c, out int a);
                    var aByte = BitConverter.GetBytes((ushort)a);
                    result += $"{{{aByte[1]:X2}{aByte[0]:X2}" + "}";
                }
                else //If not, just construct the string like normal
                {
                    result += c;
                }
            }
            foreach (KeyValuePair<string, string> repl in dicCC)
            {
                result = result.Replace(repl.Key, repl.Value);
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
                        chars0 == "0D" || chars0 == "0E" || chars0 == "0F" || chars0 == "C2" || chars0 == "C3" || chars0 == "C4" || chars0 == "C5" || chars0 == "C6" || chars0 == "C7" ||
                        chars0 == "C8" || chars0 == "C9" || chars0 == "CA" || chars0 == "CB" || chars0 == "D0" || chars0 == "D1" || chars0 == "D2" || chars0 == "D3" || chars0 == "D4" ||
                        chars0 == "D5" || chars0 == "D6" || chars0 == "D7" || chars0 == "D8" || chars0 == "D9" || chars0 == "DA" || chars0 == "DB" || chars0 == "DC" || chars0 == "DD" ||
                        chars0 == "DE" || chars0 == "DF" || chars0 == "E0" || chars0 == "E1" || chars0 == "E2" || chars0 == "E3" || chars0 == "E4" || chars0 == "E5" || chars0 == "E6" ||
                        chars0 == "E7" || chars0 == "E8" || chars0 == "E9" || chars0 == "EA" || chars0 == "EB" || chars0 == "EC" || chars0 == "ED" || chars0 == "EE" || chars0 == "EF" ||
                        chars0 == "CC" || chars0 == "CD" || chars0 == "CE" || chars0 == "CF")
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
