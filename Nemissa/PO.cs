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


        /*private void Dict(string toPO)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>
            {
                ["{FF01}"] = "\n",

                //Spanish localization
                ["\u30F3"] = "ñ", //ン
                ["\u30D9"] = "ä", //ベ
                ["\u30E9"] = "á", //ラ　
                ["\u30DA"] = "ï", //ペ
                ["\u30EA"] = "í", //リ
                ["\u30DB"] = "ü", //ホ
                ["\u30EB"] = "ú", //ル
                ["\u30DC"] = "ë", //ボ
                ["\u30EC"] = "é", //レ
                ["\u30DD"] = "ö", //ポ
                ["\u30ED"] = "ó", //ロ
                ["\u30A1"] = "Ä", //ァ
                ["\u30A2"] = "Á", //ア
                ["\u30A3"] = "Ï", //ィ
                ["\u30A4"] = "Í", //イ
                ["\u30A5"] = "Ü", //ゥ
                ["\u30A6"] = "Ú", //ウ
                ["\u30A7"] = "Ë", //ェ
                ["\u30A8"] = "É", //エ
                ["\u30A9"] = "Ö", //ォ
                ["\u30AA"] = "Ó", //オ
                ["\u30D1"] = "¡", //パ
                ["\u30D7"] = "¿", //プ
            };
        }*/

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
                    if (chars0 == "FF" || chars0 == "0B" || chars0 == "0C" || chars0 == "0E" || chars0 == "11" || chars0 == "C5" || chars0 == "C7" || chars0 == "CA" || chars0 == "D0")
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
            poYarhl.Add(new PoEntry(toPO) { Context = i.ToString() });
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
                    pointer = CCI(poInstance.Entries[i].Translated).Length + pointer;
                    bw.Write((ushort)pointer);
                }

                bw.BaseStream.Position = header.textOffset;
                foreach (var entry in poInstance.Entries)
                {
                    bw.Write(CCI(entry.Translated));
                }
                fs.SetLength(bw.BaseStream.Position);
            }
                   
        }
    }
}
