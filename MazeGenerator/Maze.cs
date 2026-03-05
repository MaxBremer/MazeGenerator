namespace MazeGenerator;

internal sealed class Maze
{
    private readonly Cell[,] _cells;

    public int Width { get; }
    public int Height { get; }
    public MazeBoundaryOpening? Start { get; private set; }
    public Cell? StartCell => Start.HasValue ? GetCell(Start.Value.X, Start.Value.Y) : null;
    public MazeBoundaryOpening? End { get; private set; }
    public Cell? EndCell => End.HasValue ? GetCell(End.Value.X, End.Value.Y) : null;

    public Maze(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        Width = width;
        Height = height;
        _cells = new Cell[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                _cells[x, y] = new Cell(x, y);
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"Coordinates ({x}, {y}) are outside the maze bounds.");
        }

        return _cells[x, y];
    }

    public List<Cell> GetAllCells()
    {
        List<Cell> cells = new();
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                cells.Add(_cells[x, y]);
            }
        }
        return cells;
    }

    public Cell GetRandomCell()
    {
        var x = Random.Shared.Next(Width);
        var y = Random.Shared.Next(Height);
        return GetCell(x, y);
    }

    public bool CanConnect(int x, int y, Direction direction)
    {
        var (dx, dy) = direction.ToOffset();
        return IsInBounds(x, y) && IsInBounds(x + dx, y + dy);
    }

    public void Connect(int x, int y, Direction direction)
    {
        if (!CanConnect(x, y, direction))
        {
            throw new InvalidOperationException($"Cannot connect cell ({x}, {y}) toward {direction} because no neighbor exists in that direction.");
        }

        var cell = GetCell(x, y);
        var (dx, dy) = direction.ToOffset();
        var neighbor = GetCell(x + dx, y + dy);

        cell.Open(direction);
        neighbor.Open(direction.Opposite());


        if (Program.ANIMATE)
        {
            Program.DrawFrame();
        }
    }

    //override that gets direction using method
    public void Connect(int x, int y, Cell to)
    {
        var from = GetCell(x, y);
        var direction = GetDirectionTo(from, to);
        Connect(x, y, direction);
    }

    public void Connect(Cell from, Cell to)
    {
        var direction = GetDirectionTo(from, to);
        Connect(from.X, from.Y, direction);
    }

    public Direction GetDirectionTo(Cell from, Cell to)
    {
        if (from.X == to.X)
        {
            if (from.Y == to.Y - 1) return Direction.South;
            if (from.Y == to.Y + 1) return Direction.North;
        }
        else if (from.Y == to.Y)
        {
            if (from.X == to.X - 1) return Direction.East;
            if (from.X == to.X + 1) return Direction.West;
        }
        throw new ArgumentException($"Cells ({from.X}, {from.Y}) and ({to.X}, {to.Y}) are not adjacent.");
    }

    public void SetStart(int x, int y, Direction direction)
    {
        Start = OpenBoundary(x, y, direction);
    }

    public void SetEnd(int x, int y, Direction direction)
    {
        End = OpenBoundary(x, y, direction);
    }

    private MazeBoundaryOpening OpenBoundary(int x, int y, Direction direction)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"Coordinates ({x}, {y}) are outside the maze bounds.");
        }

        if (CanConnect(x, y, direction))
        {
            throw new InvalidOperationException($"Cell ({x}, {y}) toward {direction} is not a boundary opening.");
        }

        var cell = GetCell(x, y);
        cell.Open(direction);

        return new MazeBoundaryOpening(x, y, direction);
    }

    private bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
}

internal readonly record struct MazeBoundaryOpening(int X, int Y, Direction Direction);
