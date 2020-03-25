using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public static Map map;
    public static List<Pos> tail = new List<Pos>();
    public static List<List<Pos>> possibleEnemyTails = new List<List<Pos>>();

    static void Main(string[] args)
    {
        var inputStr = "";
        string[] inputs;
        inputStr = Console.ReadLine();
        inputs = inputStr.Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);
        map = new Map(width, height);

        for (int i = 0; i < height; i++)
        {
            string line = Console.ReadLine();
            map.PopulateMap(i, line);
        }
        Err(map.ToString());
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        var startPos = map.RandomStart();
        tail.Add(startPos);
        Console.WriteLine(startPos.ToString());

        // populate possible enemy tails start
        foreach (Pos validPos in map.GetAllValidPos())
        {
            possibleEnemyTails.Add(new List<Pos> { validPos });
        }

        // game loop
        while (true)
        {

            inputStr = Console.ReadLine();
            Err(inputStr);
            inputs = inputStr.Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int myLife = int.Parse(inputs[2]);
            int oppLife = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = Console.ReadLine();
            Err(sonarResult);
            string opponentOrders = Console.ReadLine(); // MOVE N |TORPEDO 3 5
            Err($"opponentOrders are: {opponentOrders}");
            CardinalPos oppoenetCardinalMove;
            int surfaceSector;
            ParseOpponentOrders(opponentOrders, out oppoenetCardinalMove, out surfaceSector);
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            // Populate enemy tails
            PopulateEnemyTails(oppoenetCardinalMove, surfaceSector);
            Err($"There are {possibleEnemyTails.Count} possible enemy tails left");
            if (possibleEnemyTails.Count == 1)
            {
                Err($"I KNOW THE ENEMY IS AT {possibleEnemyTails.First().Last()}.");
            }


            // Make my move
            var validMoves = map.ValidMoves(new Pos(x, y));
            Err($"TAIL: {tail.Select(t => t.ToString()).Aggregate((a, b) => a + "," + b)}");
            Err(validMoves.Select(m => m.ToString()).Aggregate((a, b) => a + "," + b));
            validMoves = validMoves.Where(m => !tail.Contains(m)).ToList();
            if (!validMoves.Any())
            {
                Console.WriteLine("SURFACE"); // CAN I ALSO MOVE? 
                tail = new List<Pos>();
                tail.Add(new Pos(x, y));
            }
            else
            {
                Err(validMoves.Select(m => m.ToString()).Aggregate((a, b) => a + "," + b));

                Random rand = new Random();
                var move = validMoves[rand.Next(validMoves.Count)];
                tail.Add(move);
                var cardinal = move.ToCardinal(new Pos(x, y));
                Console.WriteLine($"MOVE {cardinal} TORPEDO");
            }
        }
    }

    private static void PopulateEnemyTails(CardinalPos oppoenetCardinalMove, int surfaceSector)
    {
        if (surfaceSector > 0)
        {
            possibleEnemyTails.RemoveAll(t => !map.IsInSector(t.Last(), surfaceSector));
        }

        if (oppoenetCardinalMove == CardinalPos.NA) return;

        foreach (var tail in possibleEnemyTails)
        {
            var lastMove = tail.Last();
            Pos newMove = lastMove;
            switch (oppoenetCardinalMove)
            {
                case CardinalPos.N:
                    newMove.Y--; 
                    break;
                case CardinalPos.E:
                    newMove.X++;
                    break;
                case CardinalPos.S:
                    newMove.Y++;
                    break;
                case CardinalPos.W:
                    newMove.X--;
                    break;
            }

            if(map.IsWaterTile(newMove))
            {
                tail.Add(newMove);
            }
            else
            {
                tail.RemoveAll(x => true);
            }
        }

        possibleEnemyTails.RemoveAll(x => x.Count == 0);
    }

    private static void ParseOpponentOrders(string opponentOrders, out CardinalPos oppoenetCardinalMove, out int surfaceSector)
    {
        const string MOVE = "MOVE ";
        const string SURFACE= "SURFACE ";

        oppoenetCardinalMove = CardinalPos.NA;
        surfaceSector = 0;

        if (opponentOrders.Contains(MOVE))
        {
            var strOppoenetCardinalMove = opponentOrders[opponentOrders.IndexOf(MOVE) + MOVE.Length];
            Err($"strOppoenetCardinalMove: {strOppoenetCardinalMove}");
            switch (strOppoenetCardinalMove)
            {
                case 'N':
                    oppoenetCardinalMove = CardinalPos.N;
                    break;
                case 'E':
                    oppoenetCardinalMove = CardinalPos.E;
                    break;
                case 'S':
                    oppoenetCardinalMove = CardinalPos.S;
                    break;
                case 'W':
                    oppoenetCardinalMove = CardinalPos.W;
                    break;
                default:
                    throw new Exception($"oppoenetCardinalMove {strOppoenetCardinalMove} not a valid cardinal position");
            }
        }

        if (opponentOrders.Contains(SURFACE))
        {
            Err($"opponentOrders.IndexOf(SURFACE) + SURFACE.Length: {opponentOrders.IndexOf(SURFACE) + SURFACE.Length}");
            Err($"opponentOrders in ParseOpponentOrders: {opponentOrders}");
            surfaceSector = int.Parse(opponentOrders[opponentOrders.IndexOf(SURFACE) + SURFACE.Length].ToString());
            Err($"surfaceSector {surfaceSector}");
        }
    }

    private static void Err(string msg)
    {
        Console.Error.WriteLine(msg);
    }
}

internal enum CardinalPos
{
    NA, N, E, S, W
}

class Map
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] GameMap { get; set; }

    public List<Sector> Sectors = new List<Sector>
    {
        new Sector
        {
            MinPos = new Pos(0, 0),
            MaxPos = new Pos(4, 4)
        },
        new Sector
        {
            MinPos = new Pos(5, 0),
            MaxPos = new Pos(9, 4)
        },
        new Sector
        {
            MinPos = new Pos(10, 0),
            MaxPos = new Pos(14, 4)
        },
        new Sector
        {
            MinPos = new Pos(0, 5),
            MaxPos = new Pos(4, 9)
        },
        new Sector
        {
            MinPos = new Pos(5, 5),
            MaxPos = new Pos(9, 9)
        },
        new Sector
        {
            MinPos = new Pos(10, 5),
            MaxPos = new Pos(14, 9)
        },
        new Sector
        {
            MinPos = new Pos(0, 10),
            MaxPos = new Pos(4, 14)
        },
        new Sector
        {
            MinPos = new Pos(5, 10),
            MaxPos = new Pos(9, 14)
        },
        new Sector
        {
            MinPos = new Pos(10, 10),
            MaxPos = new Pos(14, 14)
        }
    };

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        GameMap = new Tile[height, width];
    }

    public void PopulateMap(int lineNo, string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            var tile = (line[i] == '.') ? Tile.Water : Tile.Island;
            GameMap[lineNo, i] = tile;
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                result.Append((GameMap[y, x] == Tile.Water) ? '.' : 'x');
            }
            result.AppendLine();
        }
        return result.ToString();
    }

    internal Pos RandomStart()
    {
        var waterTiles = GetAllValidPos().ToList();

        Random rand = new Random();
        return waterTiles[rand.Next(waterTiles.Count)];
    }

    internal List<Pos> ValidMoves(Pos pos)
    {
        var result = new List<Pos>();

        var north = new Pos(pos.X, pos.Y - 1);
        if (IsWaterTile(north)) result.Add(north);

        var east = new Pos(pos.X+1, pos.Y);
        if (IsWaterTile(east)) result.Add(east);

        var south = new Pos(pos.X, pos.Y + 1);
        if (IsWaterTile(south)) result.Add(south);

        var west = new Pos(pos.X - 1, pos.Y);
        if (IsWaterTile(west)) result.Add(west);

        return result;
    }

    internal IEnumerable<Pos> GetAllValidPos()
    {
        var waterTiles = new List<Pos>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (GameMap[y, x] == Tile.Water)
                {
                    waterTiles.Add(new Pos(x, y));
                }
            }
        }
        return waterTiles;
    }

    internal bool IsPosOnBoard(Pos pos)
    {
        return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
    }

    internal bool IsWaterTile(Pos pos)
    {
        return IsPosOnBoard(pos) && GameMap[pos.Y, pos.X] == Tile.Water;
    }

    internal bool IsInSector(Pos pos, int sectorNumber)
    {
        Sector sector = GetSector(sectorNumber);

        if (pos.X >= sector.MinPos.X && pos.X <= sector.MaxPos.X &&
            pos.Y >= sector.MinPos.Y && pos.Y <= sector.MaxPos.Y)
        {
            return true;
        }

        return false;
    }

    private Sector GetSector(int sectorNumber)
    {
        return Sectors[sectorNumber - 1];
    }
}

class Pos : IEquatable<Pos>
{
    public Pos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString()
    {
        return $"{X} {Y}";
    }

    public bool Equals(Pos other)
    {
        return X == other.X && Y == other.Y;
    }

    internal string ToCardinal(Pos currnetPos)
    {
        if (currnetPos.Y - 1 == Y && currnetPos.X == X) return "N";
        if (currnetPos.X + 1 == X && currnetPos.Y == Y) return "E";
        if (currnetPos.Y + 1 == Y && currnetPos.X == X) return "S";
        if (currnetPos.X - 1 == X && currnetPos.Y == Y) return "W";

        throw new Exception($"ToCardinal count not be calcualted for {this} with currentPos {currnetPos}");
    }
}

class Sector
{
    public Pos MinPos { get; set; }
    public Pos MaxPos { get; set; }
}

enum Tile
{
    Water,
    Island
}