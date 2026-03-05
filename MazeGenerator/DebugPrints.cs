using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    internal static class DebugPrints
    {
        public static void PrintStartAndEndLocs(Maze m)
        {
            Console.WriteLine($"Start: {m.Start} (Cell exists: {m.StartCell != null})");
            Console.WriteLine($"End: {m.End} (Cell exists: {m.EndCell != null})");
        }
    }
}
