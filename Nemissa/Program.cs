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
            EVE eveClass = new EVE();
            PO PO = new PO();
            switch (args[0])
            {
                case "-po":
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".eve":
                            eveClass.Initialize(args[1]);
                            break;
                        default:
                            try
                            {
                                foreach (var filefound in Directory.GetFiles(args[1], "*.eve", SearchOption.AllDirectories))
                                {
                                    eveClass.Initialize(args[1]);
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