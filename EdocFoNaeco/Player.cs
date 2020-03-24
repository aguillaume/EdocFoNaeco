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
            string opponentOrders = Console.ReadLine();
            Err(opponentOrders);
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

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

    private static void Err(string msg)
    {
        Console.Error.WriteLine(msg);
    }
}

class Map
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] GameMap { get; set; }

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

        Random rand = new Random();
        return waterTiles[rand.Next(waterTiles.Count)];
    }

    internal List<Pos> ValidMoves(Pos pos)
    {
        var result = new List<Pos>();

        var north = new Pos(pos.X, pos.Y - 1);
        if (north.Y >= 0 && GameMap[north.Y, north.X] == Tile.Water) result.Add(north);

        var east = new Pos(pos.X+1, pos.Y);
        if (east.X < Width && GameMap[east.Y, east.X] == Tile.Water) result.Add(east);

        var south = new Pos(pos.X, pos.Y + 1);
        if (south.Y < Height && GameMap[south.Y, south.X] == Tile.Water) result.Add(south);

        var west = new Pos(pos.X - 1, pos.Y);
        if (west.X >= 0 && GameMap[west.Y, west.X] == Tile.Water) result.Add(west);

        return result;
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

enum Tile
{
    Water,
    Island
}