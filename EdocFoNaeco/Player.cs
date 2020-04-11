﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public static Map map;
    public static List<Pos> tail = new List<Pos>();
    public static List<List<Pos>> possibleEnemyTails = new List<List<Pos>>();
    public static List<Pos> PossibleEnemiesHit = null;
    public static int? PreviousTurnOppLife = null;
    public static int? CurrentOppLife = null;
    public static Stopwatch Timer = new Stopwatch();
    public static Stopwatch Timer2 = new Stopwatch();


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
            Timer2.Restart();
            Timer.Restart();
            inputStr = Console.ReadLine();
            //Err(inputStr);
            inputs = inputStr.Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            var myPos = new Pos(x, y);
            int myLife = int.Parse(inputs[2]);
            CurrentOppLife = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            //Err($"torpedoCooldown : {torpedoCooldown}");
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = Console.ReadLine();
            //Err(sonarResult);
            string opponentOrders = Console.ReadLine(); // MOVE N |TORPEDO 3 5
            //Err($"opponentOrders are: {opponentOrders}");
            CardinalPos oppoenetCardinalMove;
            int surfaceSector;
            Pos enemyTorpedoPos;
            bool enemySilencedMove;

            TimerLogAndReset("ReadAllInput");

            ParseOpponentOrders(opponentOrders, out oppoenetCardinalMove, out surfaceSector, out enemyTorpedoPos, out enemySilencedMove);
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            
            // Populate enemy tails
            PopulateEnemyTails(oppoenetCardinalMove, surfaceSector, enemyTorpedoPos, enemySilencedMove);
            TimerLogAndReset("PopulateEnemyTails");

            Err((possibleEnemyTails.Count == 1) ? 
                $"I KNOW THE ENEMY IS AT {possibleEnemyTails.First().Last()}." :
                $"There are {possibleEnemyTails.Count} possible enemy tails left");

            // Fire Torpedo
            Pos FireTorpedoAt = null;
            if (torpedoCooldown == 0)
            {
                var torpedoRange = GetTorpedoRange(new Pos(x, y));
                var torpedosDamageRange = torpedoRange.Select(t => new { torp = t, dmg = GetTorpedoDamageRange(t) }).ToList();

                // Remove any torpedoes that would damage me
                torpedosDamageRange.RemoveAll(t => t.dmg.Contains(myPos));

                var possibleEnemyLocations = possibleEnemyTails.Select(t => t.Last());

                var possibleTargets = torpedosDamageRange.Select(tdr => new { torpedoDamageRange = tdr, enemiesHit = possibleEnemyLocations.Where(e => tdr.dmg.Contains(e)) }).ToList();
                possibleTargets.RemoveAll(pt => !pt.enemiesHit.Any());

                //foreach (var trg in possibleTargets)
                //{
                //    Err($"Torpedo at {trg.torpedoDamageRange.torp} can hit enemies {trg.enemiesHit.Select(a => a.ToString()).Aggregate((a, b) => a + " | " + b)}");
                //}

                if (possibleTargets.Any())
                {
                    ////If there is only one enemy to hit try to hit him for 2 damage
                    //if (possibleTargets.All(pt => possibleTargets.First().enemiesHit.First() == pt.enemiesHit.First()))
                    //{
                    //    var hitMostTargets = possibleTargets.Where(pt => pt.torpedoDamageRange.torp == pt.enemiesHit.First()).Single();
                    //    //Err($"With Torpedo at: {hitMostTargets.torpedoDamageRange.torp}, I can hit enemy {hitMostTargets.enemiesHit.First()} for 2 DMG");
                    //    FireTorpedoAt = hitMostTargets.torpedoDamageRange.torp;
                    //    PossibleEnemiesHit = hitMostTargets.enemiesHit.ToList();
                    //} 
                    //else
                    //{
                    //    var hitMostTargets = possibleTargets.Aggregate((a, b) => a.enemiesHit.Count() > b.enemiesHit.Count() ? a : b);
                    //    //Err($"With Torpedo at: {hitMostTargets.torpedoDamageRange.torp}, I can hit {hitMostTargets.enemiesHit.Count()} enemies");
                    //    FireTorpedoAt = hitMostTargets.torpedoDamageRange.torp;
                    //    PossibleEnemiesHit = hitMostTargets.enemiesHit.ToList();
                    //}

                    var hitMostTargets = possibleTargets.Aggregate((a, b) => a.enemiesHit.Count() > b.enemiesHit.Count() ? a : b);
                    //Err($"With Torpedo at: {hitMostTargets.torpedoDamageRange.torp}, I can hit {hitMostTargets.enemiesHit.Count()} enemies");
                    FireTorpedoAt = hitMostTargets.torpedoDamageRange.torp;
                    PossibleEnemiesHit = hitMostTargets.enemiesHit.ToList();
                }
            }

            TimerLogAndReset("FireTorpedoLogic");

            // Make my move
            var validMoves = map.ValidMoves(new Pos(x, y));
            //Err($"TAIL: {tail.Select(t => t.ToString()).Aggregate((a, b) => a + "| " + b)}");
            //Err(validMoves.Select(m => m.ToString()).Aggregate((a, b) => a + "| " + b));
            validMoves = validMoves.Where(m => !tail.Contains(m)).ToList();
            var fireTorpedoMessage = string.Empty;
            if (FireTorpedoAt != null)
            {
                fireTorpedoMessage = $"TORPEDO {FireTorpedoAt} | ";
            }

            if (!validMoves.Any())
            {
                Console.WriteLine($"{fireTorpedoMessage}SURFACE"); // CAN I ALSO MOVE? 
                tail = new List<Pos>();
                tail.Add(new Pos(x, y));
            }
            else
            {
                //Err(validMoves.Select(m => m.ToString()).Aggregate((a, b) => a + "| " + b));

                Random rand = new Random();
                var move = validMoves[rand.Next(validMoves.Count)];
                tail.Add(move);
                var cardinal = move.ToCardinal(new Pos(x, y));
                Console.WriteLine($"{fireTorpedoMessage}MOVE {cardinal} TORPEDO");
            }

            PreviousTurnOppLife = CurrentOppLife;
            TimerLogAndReset("MakeMoveLogic");
            Err($"Turn exec time {Timer2.ElapsedMilliseconds}ms.");
        }
    }

    private static void PopulateEnemyTails(CardinalPos oppoenetCardinalMove, int surfaceSector, Pos torpedoPos, bool enemySilencedMove)
    {
        if (enemySilencedMove)
        {
            var newTails = new List<List<Pos>>();
            foreach (var tail in possibleEnemyTails)
            {
                var lastPos = tail.Last();
                var silenceRance = GetSilenceRange(lastPos, tail);
                foreach (var pos in silenceRance)
                {
                    var newTail = tail.Select(x => x).ToList(); //copy tail
                    newTail.Add(pos);
                    newTails.Add(newTail);
                }
            }

            possibleEnemyTails = newTails;
        }

        if (surfaceSector > 0)
        {
            possibleEnemyTails.RemoveAll(t => !map.IsInSector(t.Last(), surfaceSector));
            //Err($"After Surface there are: {possibleEnemyTails.Count()} left");
        }

        if (torpedoPos != null)
        {
            List<Pos> torpedoRange = GetTorpedoRange(torpedoPos);
            //Err($"torpedoRange: {torpedoRange.Select(t => t.ToString()).Aggregate((a, b) => a + "| " + b)}");
            //remove all tails that have the last position outside the torpedo range 
            possibleEnemyTails.RemoveAll(t => !torpedoRange.Contains(t.Last()));
            //Err($"After torpedoPos there are: {possibleEnemyTails.Count()} left");

        }

        // I fired a torpedo, I should have hit one of these. If I did remove all other options. If I didn't remove these options.
        if (PossibleEnemiesHit != null)
        {
            if (CurrentOppLife != null && PreviousTurnOppLife != null && PreviousTurnOppLife != CurrentOppLife)
            {
                possibleEnemyTails.RemoveAll(t => !PossibleEnemiesHit.Contains(t.Last()));
            }
            else
            {
                possibleEnemyTails.RemoveAll(t => PossibleEnemiesHit.Contains(t.Last()));
            }
            PossibleEnemiesHit = null;
            //Err($"After PossibleEnemiesHit there are: {possibleEnemyTails.Count()} left");

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

            if (map.IsWaterTile(newMove))
            {
                tail.Add(newMove);
            }
            else
            {
                tail.RemoveAll(x => true);
            }
        }

        possibleEnemyTails.RemoveAll(x => x.Count == 0);
        //Err($"After EnemyMOve there are: {possibleEnemyTails.Count()} left");

    }

    private static List<Pos> GetSilenceRange(Pos lastPos, List<Pos> tail)
    {
        var silenceRange = new List<Pos>();

        //north
        for (int i = 1; i <= 4; i++)
        {
            var pos = new Pos(lastPos.X, lastPos.Y+i);
            if (map.IsPosOnBoard(pos) && map.IsWaterTile(pos) && !tail.Contains(pos))
            {
                silenceRange.Add(pos);
                break;
            }
        }

        //East
        for (int i = 1; i <= 4; i++)
        {
            var pos = new Pos(lastPos.X+i, lastPos.Y);
            if (map.IsPosOnBoard(pos) && map.IsWaterTile(pos) && !tail.Contains(pos))
            {
                silenceRange.Add(pos);
                break;
            }
        }

        //South
        for (int i = 1; i <= 4; i++)
        {
            var pos = new Pos(lastPos.X, lastPos.Y-i);
            if (map.IsPosOnBoard(pos) && map.IsWaterTile(pos) && !tail.Contains(pos))
            {
                silenceRange.Add(pos);
                break;
            }
        }

        //West
        for (int i = 1; i <= 4; i++)
        {
            var pos = new Pos(lastPos.X-i, lastPos.Y);
            if (map.IsPosOnBoard(pos) && map.IsWaterTile(pos) && !tail.Contains(pos))
            {
                silenceRange.Add(pos);
                break;
            }
        }

        return silenceRange;
    }

    private static List<Pos> GetTorpedoRange(Pos torpedoPos)
    {
        var torpedoRange = new List<Pos>();

        var minPos = new Pos(torpedoPos.X - 4, torpedoPos.Y - 4);
        var maxPos = new Pos(torpedoPos.X + 4, torpedoPos.Y + 4);

        for (int i = 0; i <= 4; i++)
        {
            if (i == 0)
            {
                torpedoRange.Add(new Pos(minPos.X, torpedoPos.Y));

                torpedoRange.Add(new Pos(maxPos.X, torpedoPos.Y));
            }

            if (i == 1)
            {
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - i));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + i));

                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - i));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + i));
            }

            if (i == 2)
            {
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 2));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 2));

                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - 2));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - 1));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + 2));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + 1));
            }

            if (i == 3)
            {
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 3));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 2));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 2));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 3));

                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - 3));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - 2));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y - 1));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + 1));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + 2));
                torpedoRange.Add(new Pos(maxPos.X - i, torpedoPos.Y + 3));
            }

            if (i == 4)
            {
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 4));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 3));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 2));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 1));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 2));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y - 3));
                torpedoRange.Add(new Pos(minPos.X + i, torpedoPos.Y + 4));
            }
        }

        //remove all tiles not on map and non water tiles
        torpedoRange.RemoveAll(r => !map.IsPosOnBoard(r) || !map.IsWaterTile(r));

        return torpedoRange;
    }

    private static List<Pos> GetTorpedoDamageRange(Pos torpado)
    {
        var torpedoDamageRange = new List<Pos>();
        for (int y = torpado.Y-1 ; y <= torpado.Y + 1; y++)
        {
            for (int x = torpado.X - 1; x <= torpado.X + 1; x++)
            {
                torpedoDamageRange.Add(new Pos(x, y));
            }
        }

        //remove all tiles not on map and non water tiles
        torpedoDamageRange.RemoveAll(r => !map.IsPosOnBoard(r) || !map.IsWaterTile(r));
        return torpedoDamageRange;
    }

    private static void ParseOpponentOrders(string opponentOrders, out CardinalPos oppoenetCardinalMove, 
        out int surfaceSector, out Pos torpedoPos, out bool silence)
    {
        const string MOVE = "MOVE ";
        const string SURFACE = "SURFACE ";
        const string TORPEDO = "TORPEDO ";
        const string SEPARATOR = "|";
        const string SILENCE = "SILENCE";

        oppoenetCardinalMove = CardinalPos.NA;
        surfaceSector = 0;
        silence = false;

        if (opponentOrders.Contains(SILENCE))
        {
            silence = true;
        }

        if (opponentOrders.Contains(MOVE))
        {
            var strOppoenetCardinalMove = opponentOrders[opponentOrders.IndexOf(MOVE) + MOVE.Length];
            //Err($"strOppoenetCardinalMove: {strOppoenetCardinalMove}");
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
            surfaceSector = int.Parse(opponentOrders[opponentOrders.IndexOf(SURFACE) + SURFACE.Length].ToString());
        }

        torpedoPos = null;
        if (opponentOrders.Contains(TORPEDO))
        {
            var startIndex = opponentOrders.IndexOf(TORPEDO) + TORPEDO.Length;
            var endIndex = opponentOrders.IndexOf(SEPARATOR, startIndex);
            if (endIndex < startIndex) endIndex = opponentOrders.Length;
            var stringPos = opponentOrders.Substring(startIndex, endIndex - startIndex);

            //Err(stringPos);
            torpedoPos = new Pos(stringPos);
        }

        TimerLogAndReset("ParseOpponentOrders");
    }

    private static void Err(string msg)
    {
        Console.Error.WriteLine(msg);
    }

    private static void TimerLogAndReset(string methodName)
    {
        Err($"{methodName} duration {Timer.ElapsedMilliseconds}ms.");
        Timer.Restart();
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

        var east = new Pos(pos.X + 1, pos.Y);
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
        bool result = false;
        if (pos.X >= sector.MinPos.X && pos.X <= sector.MaxPos.X &&
            pos.Y >= sector.MinPos.Y && pos.Y <= sector.MaxPos.Y)
        {
            result = true;
        }

        return result;
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

    public Pos(string pos)
    {
        var x = pos.Split(' ');
        X = int.Parse(x[0]);
        Y = int.Parse(x[1]);
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

        throw new Exception($"ToCardinal count not be calculated for {this} with currentPos {currnetPos}");
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