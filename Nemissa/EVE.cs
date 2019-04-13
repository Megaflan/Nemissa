namespace Nemissa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EVE
    {
        public class Header
        {
            public ushort const1;
            public ushort const2;
            public ushort pointerOffset;
            public ushort textOffset;
        }

        private Header header = new Header();
        private List<ushort> pointer;
        private int entryCount;

        System.Text.Encoding SJIS = System.Text.Encoding.GetEncoding(932);
        private FileStream fstream;

        public void Initialize(string file)
        {
            using (var br = new BinaryReader(fstream = new FileStream(file, FileMode.Open), SJIS, true))
            {
                    header.const1 = br.ReadUInt16();
                    header.const2 = br.ReadUInt16();
                    header.pointerOffset = br.ReadUInt16();
                    header.textOffset = br.ReadUInt16();
                    entryCount = (header.textOffset - header.pointerOffset) / sizeof(short);
                    ParsePointer();
                    Read(file);
            }
        }

        private void ParsePointer()
        {
            pointer = new List<ushort>();
            using (var br = new BinaryReader(fstream, SJIS, true))
            {
                br.BaseStream.Position = header.pointerOffset;
                for (int j = 0; j < entryCount; j++)
                {
                    var textP = br.ReadUInt16();
                    if (textP == 0 && j == 0) pointer.Add(textP);
                    if (textP != 0 && j != 0) pointer.Add(textP);
                }
            }
            entryCount = pointer.Count;           
        }

        private string ParseTextEntry(byte[] toParse)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < toParse.Length; i++)
            {
                var byte0 = toParse[i];
                if (byte0 == 0xFF || byte0 == 0x00 || byte0 == 0x01 || byte0 == 0x02 || byte0 == 0x03 || byte0 == 0x04 || byte0 == 0x05 || byte0 == 0x06 || byte0 == 0x07 ||
                    byte0 == 0x08 || byte0 == 0x09 || byte0 == 0x10 || byte0 == 0x11 || byte0 == 0x12 || byte0 == 0x13 || byte0 == 0x0A || byte0 == 0x0B || byte0 == 0x0C ||
                    byte0 == 0x0D || byte0 == 0x0E || byte0 == 0x0F || byte0 == 0xC2 || byte0 == 0xC3 || byte0 == 0xC4 || byte0 == 0xC5 || byte0 == 0xC6 || byte0 == 0xC7 ||
                    byte0 == 0xC8 || byte0 == 0xC9 || byte0 == 0xCA || byte0 == 0xCB || byte0 == 0xD0 || byte0 == 0xD1 || byte0 == 0xD2 || byte0 == 0xD3 || byte0 == 0xD4 ||
                    byte0 == 0xD5 || byte0 == 0xD6 || byte0 == 0xD7 || byte0 == 0xD8 || byte0 == 0xD9 || byte0 == 0xDA || byte0 == 0xDB || byte0 == 0xDC || byte0 == 0xDD ||
                    byte0 == 0xDE || byte0 == 0xDF || byte0 == 0xE0 || byte0 == 0xE1 || byte0 == 0xE2 || byte0 == 0xE3 || byte0 == 0xE4 || byte0 == 0xE5 || byte0 == 0xE6 ||
                    byte0 == 0xE7 || byte0 == 0xE8 || byte0 == 0xE9 || byte0 == 0xEA || byte0 == 0xEB || byte0 == 0xEC || byte0 == 0xED || byte0 == 0xEE || byte0 == 0xEF ||
                    byte0 == 0xCB || byte0 == 0xCC || byte0 == 0xCD || byte0 == 0xCE || byte0 == 0xCF)
                {
                    var byte1 = toParse[i + 1];
                    i++;
                    sb.Append($"{{{byte0:X2}{byte1:X2}");
                    sb.Append("}");
                }
                else
                {
                    sb.Append((char)toParse[i]);
                }
            }
            return sb.ToString();
        }

        private string ReadEntry(int n, int size)
        {
            using (var br = new BinaryReader(fstream, SJIS, true))
            {
                br.BaseStream.Position = pointer[n] + header.textOffset;
                var toParse = br.ReadBytes(size);
                return ParseTextEntry(toParse);
            }
        }

        private void Read(string file)
        {
            PO PO = new PO();
            for (int i = 0; i < entryCount; i++)
            {
                var size = (i + 1 == entryCount) ? fstream.Length - (pointer[i] + header.textOffset) : pointer[i + 1] - pointer[i];
                var entry = ReadEntry(i, (int)size);
                PO.POExport(entry, i);
            }
            PO.POWrite(file);
        }
    }
}
