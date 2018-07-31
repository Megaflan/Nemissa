﻿using System;
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
            switch (args[0])
            {
                case "-po":
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".eve":
                            using (BinaryReader br = new BinaryReader(File.OpenRead(args[1])))
                            {
                                File.WriteAllText(args[1] + ".xml", PO.Placeholder);
                            }
                            break;
                        default:
                            try
                            {
                                foreach (var filefound in Directory.GetFiles(args[1], "*.eve", SearchOption.AllDirectories))
                                {
                                    File.WriteAllText(filefound + ".xml", PO.Placeholder);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR: No EVE files found");
                                Console.WriteLine(ex);
                                Console.ReadLine();
                            }
                            //recursive search
                            break;
                    }
                    break;
                case "-eve":
                    break;
            }
        }

    }
}