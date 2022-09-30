
using System;
using System.Text;
using System.IO;
using System.Security.Claims;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace JotronZones
{
    class MapZones
    {
        static private List<(Shape shape, ZoneTypes type)> zones = new();


        enum ZoneTypes {
            warn,
            safe,
            fire
        }


        public static int Main(string[] args)
        {
            if (Path.GetExtension(args[0]) != ".map") throw new InvalidDataException("Please supply correct data format");

            string[] lines = File.ReadAllLines(args[0]);
            lines = lines.Select(line => Regex.Replace(line, @"\s+", " ").Trim()).ToArray();

            foreach (string line in lines)
            {
                Console.WriteLine(line);
                string[] lineSplit = line.Split(' ');
                if (lineSplit.Length != 5) throw new ArgumentException($"Line {line} does not meet the format");

                Shape zone = NewShape(lineSplit);

                string type = lineSplit[0];

                switch (type)
                {
                    case nameof(ZoneTypes.warn):
                        zones.Add((zone, ZoneTypes.warn));
                        break;
                    case nameof(ZoneTypes.fire):
                        zones.Add((zone, ZoneTypes.fire));
                        break;
                    case nameof(ZoneTypes.safe):
                        zones.Add((zone, ZoneTypes.safe));
                        break;
                    default: throw new ArgumentException("Invalid type provided");
                }
            }

            return 0;
        }

        #region HelperFunctions
        private static Shape NewShape(string[] line)
        {
            string shape = line[1];
            string coords1 = line[3];
            var match = Regex.Match(coords1, @"\((-?\d+),(-?\d+)\)");
            int x1 = int.Parse(match.Groups[1].Value);
            int y1 = int.Parse(match.Groups[2].Value);
            if (typeof(Rectangle).Name.ToLower() == shape)
            {
                string coords2 = line[4];
                
                var match2 = Regex.Match(coords2, @"\((-?\d+),(-?\d+)\)");
                int x2 = int.Parse(match2.Groups[1].Value);
                int y2 = int.Parse(match2.Groups[2].Value);

                return new Rectangle((x1, y1), (x2, y2));
            }
            else if (typeof(Circle).Name.ToLower() == shape)
            {
                string radius = line[4];
                int r = int.Parse(radius);
                return new Circle((x1, y1), r);
            }
            else throw new ArgumentException();
        }
        #endregion


        abstract class Shape {
            public abstract bool IsInTheArea((int x,int y) coords);
        }
        private class Circle : Shape
        {
            private (int x, int y) coords;
            private int radius;

            public Circle((int x, int y) coords, int radius)
            {
                this.coords = coords;
                this.radius = radius;
                if (radius < 0) throw new ArgumentException("A circle with a negative radius is invalid");
            }
            public override bool IsInTheArea((int x, int y) coords)
            {
                if (Math.Pow(coords.x - this.coords.x, 2) +
                    Math.Pow(coords.y - this.coords.x, 2) <= Math.Pow(radius, 2))
                    return true;
                else
                    return false;
            }

        }
        private class Rectangle : Shape
        {
            private (int x, int y) coords1;
            private (int x, int y) coords2;

            public Rectangle((int x, int y) coords1, (int x, int y) coords2)
            {
                this.coords1 = coords1;
                this.coords2 = coords2;
                if (coords2.x < coords1.x || coords2.y < coords1.y) throw new ArgumentException("Invalid coordinates of a rectangle");
            }
            public override bool IsInTheArea((int x, int y) coords) => coords.x >= coords1.x && coords.x <= coords2.x && coords.y >= coords2.y && coords.y <= coords2.y;

        }
    }

}





