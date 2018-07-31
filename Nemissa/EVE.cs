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

            private Stream fstream;

            private Header header = new Header();
            private int entryCount;
            private List<short> pointer;

            System.Text.Encoding SJIS = System.Text.Encoding.GetEncoding(932);

            public EVE(Stream file)
            {
                fstream = file;
                Initialize();
                ParsePointer();
                Read();
            }

            private void Initialize()
            {
                using (var br = new BinaryReader(fstream, SJIS, true))
                {
                    header.const1 = br.ReadUInt16();
                    header.const2 = br.ReadUInt16();
                    header.pointerOffset = br.ReadUInt16();
                    header.textOffset = br.ReadUInt16();
                    entryCount = (header.textOffset - header.pointerOffset) / sizeof(short);
                }
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
                        if (textP != 0 && i != 0) pointer.Add(br.ReadInt16());
                    }
                }
                entryCount = pointer.Count;
            }

            private string ParseTextEntry(byte[] toParse)
            {
                // TODO
                // byte[0] == 0xFF, byte[0] == 0x0B, byte[0] == 0x0C and byte[0] == 0xC7
                // Control codes written in pairs, ex. {FF0B}
                // toParse will be all the bytes in the entry (Line 77)
                // Search through the byte array created by toParse and use switch / case to deal with them
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

            public string Read()
            {
                string result = "";
                for (int i = 0; i < entryCount; i++)
                {
                    var size = (i + 1 == entryCount) ? fstream.Length - (pointer[i] + header.textOffset) : pointer[i + 1] - pointer[i];
                    var entry = ReadEntry(i, (int)size);
                    result += entry + "\n";
                }
                return result;
            }
    }
}
