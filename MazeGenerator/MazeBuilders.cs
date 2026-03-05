namespace MazeGenerator;


internal enum TreeGrowingMode
{
    RandomCell,
    NewestCell,
    OldestCell,
    HorizontalBias,
    VerticalBias,
    NorthMode,
    SouthMode,
    EastMode,
    WestMode
}

internal static class MazeBuilders
{
    
    public const TreeGrowingMode DefaultMode = TreeGrowingMode.VerticalBias;

    //defs:
    //Fully connected: There is a path between any two cells in the maze. No isolated sections.
    //Branching: At least one cell that has connections in 3 or more directions. A maze with no branching is essentially just a single long corridor snaking around.
    //Perfect: A maze that is both fully connected and has no loops. Exactly one path between any two cells. 
    //Braided: Branching with no dead-ends.

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

        MazeBuildingHelpers.SetStandardExits(m);
    }

    //Simulates individual traversal, basically depth-first-search.
    public static void RecursiveBacktracking(Maze m)
    {
        List<Cell> visited = new();
        MazeBuildingHelpers.SetRandomExitsVertical(m);
        Stack<Cell> toCheck = new();
        if(m.StartCell == null) throw new InvalidOperationException("Start cell is not set.");
        toCheck.Push(m.StartCell);

        while (toCheck.Count > 0)
        {
            var target = toCheck.Pop();
            visited.Add(target);
            List<Cell> options = MazeBuildingHelpers.GetAdjacentCells(m, target).Where(c => !visited.Contains(c)).ToList();

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

    //Usually used with weighted edges.
    //Kruskals is a minimum spanning tree algorithm. Each cell is its own group; randomly select an edge and if it connects two different groups, connect it and merge the groups.
    //Repeat until all cells are in the same group.
    //Perfect, branching. Minimum spanning tree.
    public static void KruskalGeneration(Maze m)
    {
        MazeBuildingHelpers.SetRandomExitsVertical(m);

        List<List<Cell>> groups = new();
        List<Edge> edges = MazeBuildingHelpers.GetAllAdjacencyEdges(m);

        foreach (var cell in m.GetAllCells())
        {
            groups.Add(new List<Cell> { cell });
        }

        while(groups.Count > 1)
        {
            Edge choice = edges[Random.Shared.Next(edges.Count)];
            edges.Remove(choice); //Remove this edge from the list so we don't try to connect it again
            if(!MazeBuildingHelpers.areInSameGroup(groups, choice.Cell1, choice.Cell2))
            {
                m.Connect(choice.Cell1, choice.Cell2);
                var group1 = MazeBuildingHelpers.groupFromCell(groups, choice.Cell1);
                var group2 = MazeBuildingHelpers.groupFromCell(groups, choice.Cell2);
                //Merge group2 into group1 and remove group2

                group1.AddRange(group2);
                groups.Remove(group2);
            }

        }
    }


    //Prim's is a minimum spanning tree algorithm.
    //Start with a random cell and add it to the maze.
    //Add all of its edges to a frontier list.
    //Randomly select an edge from the frontier; if it connects a cell in the maze to a cell outside the maze, connect it and add the new cell's edges to the frontier. Repeat until the frontier is empty.
    public static void Prim(Maze m)
    {
        MazeBuildingHelpers.SetRandomExitsVertical(m);
        List<Cell> frontier = new();
        Cell startingCell = m.GetRandomCell();
        frontier.Add(startingCell);

        Dictionary<Cell, Cell> origins = new();

        List<Cell> visited = new();

        while (frontier.Count > 0)
        {
            int targetIndex = Random.Shared.Next(frontier.Count);
            Cell target = frontier[targetIndex];

            foreach(var cell in MazeBuildingHelpers.GetAdjacentCells(m, target))
            {
                if(!visited.Contains(cell) && !frontier.Contains(cell))
                {
                    frontier.Add(cell);
                    origins[cell] = target;
                }
            }

            if(origins.TryGetValue(target, out var origin))
            {
                m.Connect(target, origin);
            }

            frontier.RemoveAt(targetIndex);
            origins.Remove(target);
            visited.Add(target);
        }
    }

    //Tree growing is a generalization of Prim's algorithm.
    //Instead of randomly selecting from the frontier, you can apply different strategies to select the next cell/edge.
    //Technically don't need Prims: this on Random mode == Prims.
    public static void TreeGrowing(Maze m, TreeGrowingMode mode = DefaultMode)
    {
        MazeBuildingHelpers.SetRandomExitsVertical(m);

        var visited = new HashSet<Cell>();
        var frontier = new List<Cell>();

        var startingCell = m.GetRandomCell();
        frontier.Add(startingCell);
        visited.Add(startingCell);

        while (frontier.Count > 0)
        {
            var frontierIndex = MazeBuildingHelpers.SelectFrontierIndex(frontier.Count, mode);
            var target = frontier[frontierIndex];
            var options = MazeBuildingHelpers.GetAdjacentCells(m, target).Where(c => !visited.Contains(c)).ToList();

            if (options.Count == 0)
            {
                frontier.RemoveAt(frontierIndex);
                continue;
            }

            var next = MazeBuildingHelpers.SelectTreeGrowingNextCell(m, target, options, mode);
            m.Connect(target, next);

            visited.Add(next);
            frontier.Add(next);
        }
    }

    public static void RemoveDeadEnds(Maze m)
    {
        var deadEnds = MazeBuildingHelpers.FindDeadEnds(m);
        
        foreach (var cell in deadEnds)
        {
            var dirOpts = Enum.GetValues<Direction>().Where(dir => m.CanConnect(cell.X, cell.Y, dir) && !cell.IsConnected(dir)).ToList();
            m.Connect(cell.X, cell.Y, dirOpts[Random.Shared.Next(dirOpts.Count)]); 
        }
    }
}
