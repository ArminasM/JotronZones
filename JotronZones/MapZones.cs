
using System.Text.RegularExpressions;

namespace JotronZones
{
    class MapZones
    {
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
            List<(Shape shape, ZoneTypes type)> zones = new();

            foreach (string line in lines)
            {
                string[] lineSplit = line.Split(' ');
                if (lineSplit.Length != 5) throw new ArgumentException($"Line {line} does not meet the format");

                Shape zone = NewShape(lineSplit);

                string type = lineSplit[0];

                switch (type)
                {
                    case nameof(ZoneTypes.warn):
                        zones.Insert(0, (zone, ZoneTypes.warn));
                        break;
                    case nameof(ZoneTypes.fire):
                        zones.Insert(0, (zone, ZoneTypes.fire));
                        break;
                    case nameof(ZoneTypes.safe):
                        zones.Insert(0, (zone, ZoneTypes.safe));
                        break;
                    default: throw new ArgumentException("Invalid type provided");
                }
            }

            //Map generation is finished, time to listen
            Console.WriteLine("Awaiting for input. Type \"exit\" to quit");
            while (true)
            {
                string? input = Console.ReadLine();
                if (input == "exit") break;
                Console.WriteLine(GetResponse((input ?? ""), zones));
            }

            return 0;
        }

        #region HelperFunctions
        private static Shape NewShape(string[] line)
        {
            string shape = line[1];
            string coords1 = line[3];
            var match = Regex.Match(coords1, @"\((-?\d+),(-?\d+)\)");
            int x1 = ParseCordinates(match).x;
            int y1 = ParseCordinates(match).y;

            if (typeof(Rectangle).Name.ToLower() == shape)
            {
                string coords2 = line[4];
                
                var match2 = Regex.Match(coords2, @"\((-?\d+),(-?\d+)\)");
                int x2 = ParseCordinates(match2).x;
                int y2 = ParseCordinates(match2).y;

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
        private static string GetResponse(string input, List<(Shape shape, ZoneTypes zone)> zones)
        {
            if (string.IsNullOrEmpty(input)) return "";
            input = Regex.Replace(input, @"\s+", "").Trim();

            var match = Regex.Match(input, @"\((-?\d+),(-?\d+)\)");
            string id = Regex.Match(input, @"^[a-zA-Z0-9]*").Value;

            if (!match.Success) return "Wrong input, try again";

            int x = ParseCordinates(match).x;
            int y = ParseCordinates(match).y;

            foreach (var area in zones)
            {
                switch (area.zone)
                {
                    case ZoneTypes.warn:
                        if(area.shape.IsInTheArea((x,y))) return $"Warning {id}";
                        break;
                    case ZoneTypes.fire:
                        if (area.shape.IsInTheArea((x, y))) return $"Shooting {id} at ({x},{y})";
                        break;
                    case ZoneTypes.safe:
                        if (area.shape.IsInTheArea((x, y))) return "";
                        break;
                    default: break;
                }
            }
            return "nothing found";
        }
        private static (int x, int y) ParseCordinates(Match match) => (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));

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
            public override bool IsInTheArea((int x, int y) coords) => Math.Pow(coords.x - this.coords.x, 2) +
                    Math.Pow(coords.y - this.coords.x, 2) <= Math.Pow(radius, 2);


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
            public override bool IsInTheArea((int x, int y) coords) => coords.x >= coords1.x && coords.x <= coords2.x && coords.y >= coords1.y && coords.y <= coords2.y;

        }
    }

}





