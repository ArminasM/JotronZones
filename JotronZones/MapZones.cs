
using System;
using System.Text;
using System.IO;
using System.Security.Claims;
using System.Linq;
using System.Text.RegularExpressions;

namespace JotronZones
{
    class MapZones
    {
        public static int Main(string[] args)
        {
            if (Path.GetExtension(args[0]) != ".map") throw new InvalidDataException("Please supply correct data format");

            string[] lines = File.ReadAllLines(args[0]);

            lines = lines.Select(line => Regex.Replace(line, @"\s+", " ")).ToArray();

            return 0;
        }


    }

}





