using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    internal class Edge
    {
        //Simply a connection between two cells. Used for Kruskal's algorithm. Does not have a direction.
        public Cell Cell1 { get; set; }
        public Cell Cell2 { get; set; }
    }
}
