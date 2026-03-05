namespace MazeGenerator;

internal sealed class Cell
{
    private readonly HashSet<Direction> _openDirections = [];

    public int X { get; }
    public int Y { get; }

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool IsConnected(Direction direction) => _openDirections.Contains(direction);

    public void Open(Direction direction) => _openDirections.Add(direction);
}
