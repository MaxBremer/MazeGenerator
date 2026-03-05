namespace MazeGenerator;

internal class Program
{
    private const int DefaultWidth = 8;
    private const int DefaultHeight = 5;

    static void Main(string[] args)
    {
        var width = ParseArgument(args, 0, DefaultWidth);
        var height = ParseArgument(args, 1, DefaultHeight);

        var maze = new Maze(width, height);
        BuildDemoPassages(maze);

        Console.WriteLine($"Maze ({width} x {height})");
        Console.WriteLine(MazeAsciiRenderer.Render(maze));
    }

    private static int ParseArgument(string[] args, int index, int defaultValue)
    {
        return args.Length > index && int.TryParse(args[index], out var value) && value > 0
            ? value
            : defaultValue;
    }

    private static void BuildDemoPassages(Maze maze)
    {
        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width - 1; x++)
            {
                if (y % 2 == 0)
                {
                    maze.Connect(x, y, Direction.East);
                }
            }

            if (y < maze.Height - 1)
            {
                var connectorColumn = y % 2 == 0 ? maze.Width - 1 : 0;
                maze.Connect(connectorColumn, y, Direction.South);
            }
        }
    }
}
