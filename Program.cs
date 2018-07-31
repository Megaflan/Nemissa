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
                    checkedBytes.Add(checkByte);
                    if (checkByte == 0)
                    {
                        endFound = true;
                    }
                }
                int lastPointerEnds = offsets.Last() + checkedBytes.Count();

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
                        len = lastPointerEnds - offsets[i];
                    }
                    byte[] str = br.ReadBytes(len);
                    var convString = sjis.GetString(str);
                    strings.Add(convString);
                    Console.WriteLine("\n[Entry " + i + "]");
                    Console.WriteLine(convString);

                } 

                //Search multiple files
                //Console.WriteLine("--------------------------------");
                //Console.WriteLine("Length of file: " + br.BaseStream.Length);
                //Console.WriteLine("Bytes readen: " + br.BaseStream.Position);
                if (br.BaseStream.Length > br.BaseStream.Position)
                {
                    //Console.WriteLine("//New entries found//");
                    //Console.WriteLine("--------------------------------");
                    br.BaseStream.Position = br.BaseStream.Position + 0x1;
                    while (br.BaseStream.Length >= br.BaseStream.Position)
                    {
                        //Preparation
                        checkedBytes.Clear();
                        offsets.Clear();

                        //Header identification
                        int nextText = (int)br.BaseStream.Position;
                        br.BaseStream.Position = nextText + 0x4;
                        if (br.BaseStream.Position >= br.BaseStream.Length)
                        {
                            break;
                        }
                        tablePos = br.ReadUInt16();
                        textPos = br.ReadUInt16();

                        //Pointer interpretation
                        br.BaseStream.Position = nextText + tablePos;
                        offlen = textPos - tablePos;
                        for (int i = 0; i < offlen / 2; i++)
                        {
                            ushort off = br.ReadUInt16();
                            if (i != offlen)
                            {
                                if (off != 0)
                                {
                                    offsets.Add(off);
                                }
                            }
                            else
                            {
                                offsets.Add(off);
                            }
                        }

                        //End of pointer table
                        br.BaseStream.Position = (nextText + textPos) + offsets.Last();
                        checkByte = 0;
                        endFound = false;
                        int endFoundPos = 0;
                        while (endFound != true)
                        {
                            while (checkByte != 0xFF)
                            {
                                checkByte = br.ReadByte();
                                checkedBytes.Add(checkByte);
                            }
                            checkByte = br.ReadByte();
                            checkedBytes.Add(checkByte);
                            if (checkByte == 0)
                            {
                                endFound = true;
                                endFoundPos = (int)br.BaseStream.Position;
                            }
                        }
                        lastPointerEnds = nextText + textPos + offsets.Last() + checkedBytes.Count();

                        //Text interpretation
                        sjis = Encoding.GetEncoding("sjis");
                        len = 0;
                        for (int i = 0; i < offsets.Count; i++)
                        {
                            br.BaseStream.Position = nextText + textPos + offsets[i];
                            if (i != offsets.Count - 1)
                            {
                                len = offsets[i + 1] - offsets[i];
                            }
                            else
                            {
                                len = endFoundPos - (nextText + textPos + offsets[i]);
                            }
                            byte[] str = br.ReadBytes(len);
                            var convString = sjis.GetString(str);
                            strings.Add(convString);
                            //Console.WriteLine("\n[Entry " + i + "]");
                            //Console.WriteLine(convString);
                        }
                    }
                }
                //Console.WriteLine("--------------------------------");
                Console.ReadLine();
            }
        }

        static string ControlCodeInterpreter(byte[] str)
        {
            var sb = new StringBuilder();

            
            return sb.ToString();
        }

    }
}