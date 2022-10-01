using System.Text.RegularExpressions;

namespace JotronZones
{
    public class MapZones
    {
        enum ZoneTypes {
            warn,
            safe,
            fire
        }
        private static List<(Shape shape, ZoneTypes type)> zones = new();
        public static int Main(string[] args)
        {
            if (args.Length == 0) throw new ArgumentException("Please provide a file");
            if (Path.GetExtension(args[0]) != ".map") throw new InvalidDataException("Data file extension is not supported");

            string[] lines = File.ReadAllLines(args[0]);
            zones = PopulateMap(lines);

            //Map generation is finished, time to listen
            Console.WriteLine("Awaiting for input. Type \"exit\" to quit");
            while (true)
            {
                string? input = Console.ReadLine();
                if (input == "exit") break;
                Console.Write(GetResponse((input ?? "")));
            }

            return 0;
        }
        private static List<(Shape shape, ZoneTypes type)> PopulateMap(string[] lines)
        {
            List<(Shape shape, ZoneTypes type)> zones = new();
            lines = lines.Select(line => Regex.Replace(line, @"\s+", " ").Trim()).ToArray();
            foreach (string line in lines)
            {
                string[] lineSplit = line.Split(' ');
                if (lineSplit.Length != 5) throw new InvalidDataException($"Map zone {line} does not meet the input format");

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
                    default: throw new ArgumentException($"Invalid zone type provided {type}");
                }
            }
            return zones;
        }
        private static Shape NewShape(string[] line)
        {
            string shape = line[1];
            string coords1 = line[3];
            var match = Regex.Match(coords1, @"\((-?\d+),(-?\d+)\)");
            if (!match.Success) throw new InvalidDataException($"Coordinates given have the wrong format");
            int x1 = ParseCordinates(match).x;
            int y1 = ParseCordinates(match).y;

            if (typeof(Rectangle).Name.ToLower() == shape)
            {
                string coords2 = line[4];
                
                var match2 = Regex.Match(coords2, @"\((-?\d+),(-?\d+)\)");
                if (!match2.Success) throw new InvalidDataException("Coordinates given have the wrong format");

                int x2 = ParseCordinates(match2).x;
                int y2 = ParseCordinates(match2).y;

                return new Rectangle((x1, y1), (x2, y2));
            }
            else if (typeof(Circle).Name.ToLower() == shape)
            {
                if(int.TryParse(line[4], out int radius)) return new Circle((x1, y1), radius);
                throw new InvalidDataException($"Inlavid radius of a circle was given \"{line[4]}\"");
            }
            else throw new ArgumentException($"Such shape does not exist: {shape}");
        }
        private static string GetResponse(string input)
        {
            bool flag;
            string output = "";
            if (string.IsNullOrEmpty(input)) return "";
            string planesList = Regex.Replace(input, @"\s+", " ");
            string[] planes = planesList.Split(' ');
            

            foreach (string plane in planes)
            {
                flag = false;
                var match = Regex.Match(plane, @"\((-?\d+),(-?\d+)\)");
                string id;
                if (plane.IndexOf("(") != -1)
                {
                    id = plane[0..plane.IndexOf("(", 0)];
                }
                else return "One of the planes format was incorrect\n";
                if (string.IsNullOrEmpty(id)) return "We cannot act without a plane ID\n";
                if (!match.Success) return "Coordinates given are the wrong format\n";
                int x = ParseCordinates(match).x;
                int y = ParseCordinates(match).y;

                foreach (var area in zones)
                {
                    switch (area.type)
                    {
                        case ZoneTypes.warn:
                            if (area.shape.IsInTheArea((x, y))) {
                                output += $"Warning {id}\n";
                                flag = true;
                            } 
                            break;
                        case ZoneTypes.fire:
                            if (area.shape.IsInTheArea((x, y))) {
                                output += $"Shooting {id} at ({x},{y})\n";
                                flag = true;
                            }
                            break;
                        case ZoneTypes.safe:
                            if (area.shape.IsInTheArea((x, y))) {
                                output += "\n";
                                flag = true;
                            }
                            break;
                        default: break;
                    }
                    if (flag) break;
                }
            }
            return output;
        }

        #region HelperFunctions
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
                if (radius < 0) throw new ArgumentException("A circle zone with a negative radius is invalid");
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
                if (coords2.x < coords1.x || coords2.y < coords1.y) throw new ArgumentException($"Invalid coordinates of a rectangle zone");
            }
            public override bool IsInTheArea((int x, int y) coords) => coords.x >= coords1.x && coords.x <= coords2.x && coords.y >= coords1.y && coords.y <= coords2.y;

        }
    }
}