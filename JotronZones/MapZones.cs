
using System;
using System.Text;
using System.IO;
using System.Security.Claims;

namespace JotronZones
{
    class MapZones
    {



        public static int Main(string[] args)
        {
            if (Path.GetExtension(args[0]) != ".map") throw new InvalidDataException("Please supply correct data format");

            string[] lines = File.ReadAllLines(args[0]);

            Console.WriteLine(lines.Length);




            return 0;
        }


    }

}





