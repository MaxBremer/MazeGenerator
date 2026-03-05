namespace MazeGenerator;

internal enum Direction
{
    North,
    East,
    South,
    West,
}

internal static class DirectionExtensions
{
    public static Direction Opposite(this Direction direction) =>
        direction switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };

    public static (int DeltaX, int DeltaY) ToOffset(this Direction direction) =>
        direction switch
        {
            Direction.North => (0, -1),
            Direction.East => (1, 0),
            Direction.South => (0, 1),
            Direction.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
}
