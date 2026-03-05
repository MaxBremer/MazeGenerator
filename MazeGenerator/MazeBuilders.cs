using System.Security.Cryptography.X509Certificates;

namespace MazeGenerator;

internal static class MazeBuilders
{
    
    //Snakes back and forth till the end. Fully connected.
    public static void BuildSnakePassage(Maze m)
    {
        for (var y = 0; y < m.Height; y++)
        {
            for (var x = 0; x < m.Width - 1; x++)
            {
                m.Connect(x, y, Direction.East);
            }

            if (y < m.Height - 1)
            {
                var connectorColumn = y % 2 == 0 ? m.Width - 1 : 0;
                m.Connect(connectorColumn, y, Direction.South);
            }
        }

        SetStandardExits(m);
    }

    public static void RecursiveBacktracking(Maze m)
    {
        List<Cell> visited = new();
        SetRandomExitsVertical(m);
        Stack<Cell> toCheck = new();
        if(m.StartCell == null) throw new InvalidOperationException("Start cell is not set.");
        toCheck.Push(m.StartCell);

        while (toCheck.Count > 0)
        {
            var target = toCheck.Pop();
            visited.Add(target);
            List<Cell> options = GetAdjacentCells(m, target).Where(c => !visited.Contains(c)).ToList();

            if(options.Count > 1)
            {
                toCheck.Push(target); //Re-add current cell to stack so we can come back to it after exploring one of the options
            }

            if(options.Count > 0)
            {
                var next = options[Random.Shared.Next(options.Count)];
                var directionToNext = m.GetDirectionTo(target, next);
                m.Connect(target.X, target.Y, directionToNext);
                toCheck.Push(next);
            }

        }

    }

    public static void KruskalGeneration(Maze m)
    {
        SetRandomExitsVertical(m);

        List<List<Cell>> groups = new();
        List<Edge> edges = GetAllAdjacencyEdges(m);

        foreach (var cell in m.GetAllCells())
        {
            groups.Add(new List<Cell> { cell });
        }

        while(groups.Count > 1)
        {
            Edge choice = edges[Random.Shared.Next(edges.Count)];
            edges.Remove(choice); //Remove this edge from the list so we don't try to connect it again
            if(!areInSameGroup(groups, choice.Cell1, choice.Cell2))
            {
                m.Connect(choice.Cell1, choice.Cell2);
                var group1 = groupFromCell(groups, choice.Cell1);
                var group2 = groupFromCell(groups, choice.Cell2);
                //Merge group2 into group1 and remove group2

                group1.AddRange(group2);
                groups.Remove(group2);
            }

        }
    }
  
    public static void Prim(Maze m)
    {
        SetRandomExitsVertical(m);
        List<Cell> frontier = new();
        Cell startingCell = m.GetRandomCell();
        frontier.Add(startingCell);

        List<Cell> origins = new() { startingCell };

        List<Cell> visited = new();

        while (frontier.Count > 0)
        {
            int targetIndex = Random.Shared.Next(frontier.Count);
            Cell target = frontier[targetIndex];

            foreach(var cell in GetAdjacentCells(m, target))
            {
                if(!visited.Contains(cell) && !frontier.Contains(cell))
                {
                    frontier.Add(cell);
                    origins.Add(target);
                }
            }

            if(target != startingCell)
            {
                m.Connect(target, origins[targetIndex]);
            }

            frontier.Remove(target);
            origins.Remove(target);
            visited.Add(target);
        }
    }
    #region Helper Methods
    public static void SetStandardExits(Maze m)
    {
        m.SetStart(0, 0, Direction.North);
        m.SetEnd(m.Width - 1, m.Height - 1, Direction.South);
    }

    public static void SetRandomExitsVertical(Maze m)
    {
        var r = new Random();
        m.SetStart(r.Next(m.Width), 0, Direction.North);
        m.SetEnd(r.Next(m.Width), m.Height - 1, Direction.South);
    }

    public static List<Cell> GetAdjacentCells(Maze m, Cell c)
    {
        List<Cell> adjacent = new();
        foreach (var dir in Enum.GetValues<Direction>())
        {
            if (m.CanConnect(c.X, c.Y, dir))
            {
                var (dx, dy) = dir.ToOffset();
                adjacent.Add(m.GetCell(c.X + dx, c.Y + dy));
            }
        }
        return adjacent;
    }

    public static List<Edge> GetAllAdjacencyEdges(Maze m)
    {
        var edges = new List<Edge>();
        //Every cell has 4 potential edges. We only add the ones that are valid (i.e. not out of bounds)
        foreach(var cell in m.GetAllCells())
        {
            foreach(var dir in Enum.GetValues<Direction>())
            {
                if(m.CanConnect(cell.X, cell.Y, dir))
                {
                    var (dx, dy) = dir.ToOffset();
                    var adjacentCell = m.GetCell(cell.X + dx, cell.Y + dy);
                    edges.Add(new Edge { Cell1 = cell, Cell2 = adjacentCell });
                }
            }
        }

        return edges;
    }

    private static bool areInSameGroup(List<List<Cell>> groups, Cell c1, Cell c2)
    {
        foreach(var group in groups)
        {
            if(group.Contains(c1) && group.Contains(c2))
            {
                return true;
            }
        }
        return false;
    }

    private static List<Cell> groupFromCell(List<List<Cell>> groups, Cell cell)
    {
        foreach(var group in groups)
        {
            if(group.Contains(cell))
            {
                return group;
            }
        }
        throw new InvalidOperationException($"Cell ({cell.X}, {cell.Y}) is not in any group.");
    }
    #endregion
}
