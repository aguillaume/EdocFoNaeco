using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{

    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var map = new Map(10 , 10);
            var tail = new List<Pos>();
            var start = new Pos(0,0);
            var goal = new Pos(5,5);

            map.PopulateMap(0, "..........");
            map.PopulateMap(1, "..........");
            map.PopulateMap(2, "..........");
            map.PopulateMap(3, "..........");
            map.PopulateMap(4, "..........");
            map.PopulateMap(5, "..........");
            map.PopulateMap(6, "..........");
            map.PopulateMap(7, "..........");
            map.PopulateMap(8, "..........");
            map.PopulateMap(9, "..........");

            var a = new AStarPathFiner();
            List<Pos> b = a.FindPath(map, tail, start, goal);
        }
        
        [TestMethod]
        public void TestMethod2()
        {
            var map = new Map(10, 10);
            var tail = new List<Pos>();
            var start = new Pos(0, 9);
            var goal = new Pos(9, 0);

            map.PopulateMap(0, "..........");
            map.PopulateMap(1, ".xxxxxxx..");
            map.PopulateMap(2, "........xx");
            map.PopulateMap(3, "xxxxx...x.");
            map.PopulateMap(4, "......xx..");
            map.PopulateMap(5, ".xxxxxx...");
            map.PopulateMap(6, "......x...");
            map.PopulateMap(7, "....xxx...");
            map.PopulateMap(8, "xxx.......");
            map.PopulateMap(9, "..........");

            var astar = new AStarPathFiner();
            List<Pos> path = astar.FindPath(map, tail, start, goal);
            Console.WriteLine(path.Select(x => x.ToString()).Aggregate((a, b) => $"{a} | {b} "));
        }

        [TestMethod]
        public void TestMethod3()
        {
            List<Pos> path = new List<Pos>();
            Console.WriteLine(path);
            Console.WriteLine(path.Select(x => x.ToString()));
            Console.WriteLine(path.Select(x => x.ToString()).Aggregate((a, b) => $"{a} | {b} "));
        }
    }
}
