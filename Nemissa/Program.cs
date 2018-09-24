namespace Nemissa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            switch (args[0])
            {
                case "-po":
                    EVE eveClass = new EVE();
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
                                    eveClass.Initialize(filefound);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR: No EVE files found");
                                Console.WriteLine(ex);
                                Console.ReadLine();
                            }
                            ////recursive search
                            break;
                    }

                    break;
                case "-eve":
                    PO poClass = new PO();
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".eve":
                            if (Path.GetExtension(args[2]).ToLower() == ".po")
                                poClass.POImport(args[1], args[2]);
                            else
                            {
                                Console.WriteLine("ERROR: No PO files found");
                                Console.ReadLine();
                            }
                            break;
                        default:
                            Console.WriteLine("ERROR: No EVE files found");
                            Console.ReadLine();
                            break;
                    }
                    break;
            }
        }
    }
}