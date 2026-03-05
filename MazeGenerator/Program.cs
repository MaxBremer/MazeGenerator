namespace MazeGenerator;

internal class Program
{
    private const int DefaultWidth = 8;
    private const int DefaultHeight = 4;

    static void Main(string[] args)
    {
        var width = ParseArgument(args, 0, DefaultWidth);
        var height = ParseArgument(args, 1, DefaultHeight);

        var maze = new Maze(width, height);
        MazeBuilders.TreeGrowing(maze, TreeGrowingMode.SouthMode);

        Console.WriteLine($"Maze ({width} x {height})");
        Console.WriteLine(MazeAsciiRenderer.Render(maze));
        DebugPrints.PrintStartAndEndLocs(maze);
    }

    private static int ParseArgument(string[] args, int index, int defaultValue)
    {
        return args.Length > index && int.TryParse(args[index], out var value) && value > 0
            ? value
            : defaultValue;
    }
}
