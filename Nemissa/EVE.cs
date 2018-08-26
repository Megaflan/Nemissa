using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemissa
{
    class EVE
    {
        class Header
        {
            public ushort const1;
            public ushort const2;
            public ushort pointerOffset;
            public ushort textOffset;
        }

        private Header header = new Header();
        private int entryCount;
        private List<short> pointer;
        

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
           }
           ParsePointer();
           Read(file);
        }

        private void ParsePointer()
        {
           pointer = new List<short>();
           using (var br = new BinaryReader(fstream, SJIS, true))
           {
               br.BaseStream.Position = header.pointerOffset;
               for (int i = 0; i < entryCount; i++)
               {
                    var textP = br.ReadInt16();
                    if (textP == 0 && i == 0) pointer.Add(textP);
                    if (textP != 0 && i != 0) pointer.Add(textP);
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
                if (byte0 == 0xFF || byte0 == 0x0B || byte0 == 0x0C || byte0 == 0x0E || byte0 == 0x11 || byte0 == 0xC5 || byte0 == 0xC7 || byte0 == 0xCA || byte0 == 0xD0)
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

        public void Read(string file)
        {
            PO PO = new PO();
            for (int i = 0; i < entryCount; i++)
            {
                var size = (i + 1 == entryCount) ? fstream.Length - (pointer[i] + header.textOffset) : pointer[i + 1] - pointer[i];
                var entry = ReadEntry(i, (int)size);
                PO.POEx(entry + "\n", i);
            }
            //PO.PoWrite(file);
        }
    }
}
