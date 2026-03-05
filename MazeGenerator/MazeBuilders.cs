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

        Dictionary<Cell, Cell> origins = new();

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

    public static void TreeGrowing(Maze m, TreeGrowingMode mode = DefaultMode)
    {
        SetRandomExitsVertical(m);

        var visited = new HashSet<Cell>();
        var frontier = new List<Cell>();

        var startingCell = m.GetRandomCell();
        frontier.Add(startingCell);
        visited.Add(startingCell);

        while (frontier.Count > 0)
        {
            var frontierIndex = SelectFrontierIndex(frontier.Count, mode);
            var target = frontier[frontierIndex];
            var options = GetAdjacentCells(m, target).Where(c => !visited.Contains(c)).ToList();

            if (options.Count == 0)
            {
                frontier.RemoveAt(frontierIndex);
                continue;
            }

            var next = SelectTreeGrowingNextCell(m, target, options, mode);
            m.Connect(target, next);

            visited.Add(next);
            frontier.Add(next);
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

    private static int SelectFrontierIndex(int frontierCount, TreeGrowingMode mode)
    {
        return mode switch
        {
            TreeGrowingMode.NewestCell or TreeGrowingMode.HorizontalBias or TreeGrowingMode.VerticalBias => frontierCount - 1,
            TreeGrowingMode.OldestCell => 0,
            _ => Random.Shared.Next(frontierCount)
        };
    }

    private static Cell SelectTreeGrowingNextCell(Maze maze, Cell from, List<Cell> options, TreeGrowingMode mode)
    {
        return mode switch
        {
            TreeGrowingMode.HorizontalBias => WeightedDirectionChoice(maze, from, options, horizontalWeight: 4, verticalWeight: 1),
            TreeGrowingMode.VerticalBias => WeightedDirectionChoice(maze, from, options, horizontalWeight: 1, verticalWeight: 4),
            TreeGrowingMode.NorthMode => WeightedDirectionChoiceFunc(maze, from, options, BuildDirectionPrefFunc(Direction.North)),
            TreeGrowingMode.SouthMode => WeightedDirectionChoiceFunc(maze, from, options, BuildDirectionPrefFunc(Direction.South)),
            TreeGrowingMode.EastMode => WeightedDirectionChoiceFunc(maze, from, options, BuildDirectionPrefFunc(Direction.East)),
            TreeGrowingMode.WestMode => WeightedDirectionChoiceFunc(maze, from, options, BuildDirectionPrefFunc(Direction.West)),
            _ => options[Random.Shared.Next(options.Count)]
        };
    }

    private static Cell WeightedDirectionChoice(Maze maze, Cell from, List<Cell> options, int horizontalWeight, int verticalWeight)
    {
        var weightedOptions = new List<(Cell Cell, int Weight)>();
        var totalWeight = 0;

        foreach (var option in options)
        {
            var direction = maze.GetDirectionTo(from, option);
            var isHorizontal = direction is Direction.East or Direction.West;
            var weight = isHorizontal ? horizontalWeight : verticalWeight;
            weightedOptions.Add((option, weight));
            totalWeight += weight;
        }

        var pick = Random.Shared.Next(totalWeight);
        var cumulativeWeight = 0;
        foreach (var (cell, weight) in weightedOptions)
        {
            cumulativeWeight += weight;
            if (pick < cumulativeWeight)
            {
                return cell;
            }
        }

        return weightedOptions[^1].Cell;
    }

    private static Cell WeightedDirectionChoiceFunc(Maze maze, Cell from, List<Cell> options, Func<Cell, Cell, int> weightFunc)
    {
        var weightedOptions = new List<(Cell Cell, int Weight)>();
        var totalWeight = 0;

        foreach (var option in options)
        {
            var optionsWeight = weightFunc(from, option);
           
            weightedOptions.Add((option, optionsWeight));
            totalWeight += optionsWeight;
        }

        var pick = Random.Shared.Next(totalWeight);
        var cumulativeWeight = 0;
        foreach (var (cell, weight) in weightedOptions)
        {
            cumulativeWeight += weight;
            if (pick < cumulativeWeight)
            {
                return cell;
            }
        }

        return weightedOptions[^1].Cell;
    }

    private static Func<Cell, Cell, int> BuildDirectionPrefFunc(Direction preferredDirection, int preferredWeight = 4, int otherWeight = 1)
    {
        return (from, to) =>
        {
            var weight = preferredDirection switch
            {
                Direction.North => to.Y < from.Y ? preferredWeight : otherWeight,
                Direction.East => to.X > from.X ? preferredWeight : otherWeight,
                Direction.South => to.Y > from.Y ? preferredWeight : otherWeight,
                Direction.West => to.X < from.X ? preferredWeight : otherWeight,
                _ => throw new ArgumentOutOfRangeException(nameof(preferredDirection), preferredDirection, null),
            };
            return weight;
        };
    }


    #endregion
}
