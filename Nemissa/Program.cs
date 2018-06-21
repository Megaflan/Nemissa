using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nemissa
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> offsets = new List<int>();
            List<int> checkedBytes = new List<int>();
            List<string> strings = new List<string>();
            using (BinaryReader br = new BinaryReader(File.Open(args[0], FileMode.Open)))
            {
                //Header identification
                br.BaseStream.Position = 0x4;
                ushort tablePos = br.ReadUInt16();
                ushort textPos = br.ReadUInt16();

                //Pointer interpretation
                br.BaseStream.Position = tablePos;
                int offlen = textPos - tablePos;
                for (int i = 0; i < offlen / 2; i++)
                {
                    ushort off = br.ReadUInt16();
                    if (off != 0)
                    {
                        offsets.Add(off);
                    }
                }
                //End of pointer table
                br.BaseStream.Position = textPos + offsets.Last();
                byte checkByte = 0;
                bool endFound = false;
                while (endFound != true)
                {
                    while (checkByte != 0xFF)
                    {
                        checkByte = br.ReadByte();
                        checkedBytes.Add(checkByte);
                    }
                    checkByte = br.ReadByte();
                    if (checkByte == 0x01)
                    {
                        endFound = true;
                    }
                }
                var lastPointerEnds = offsets.Last() + checkedBytes.Count() + 0x2;

                //Text interpretation
                var sjis = Encoding.GetEncoding("sjis");
                var len = 0;
                for (int i = 0; i < offsets.Count; i++)
                {
                    br.BaseStream.Position = textPos + offsets[i];
                    if (i != offsets.Count - 1)
                    {
                        len = offsets[i + 1] - offsets[i];
                    }
                    else
                    {
                        len = lastPointerEnds;
                    }
                    byte[] str = br.ReadBytes(len);
                    var convString = sjis.GetString(str);
                    strings.Add(convString);
                    Console.WriteLine(convString);
                }
                Console.WriteLine("--------------------------------");
                Console.WriteLine(offsets.Count + " entries found");
                Console.ReadLine();
            }
        }
    }
}