namespace MazeGenerator;

internal class Program
{
    public const bool ANIMATE = false;
    public const int ANIMATION_DELAY = 50;

    private const bool REMOVE_DEAD_ENDS = true;

    private const int DefaultWidth = 8;
    private const int DefaultHeight = 6;

    private static Maze MyMaze;

    static void Main(string[] args)
    {
        var width = ParseArgument(args, 0, DefaultWidth);
        var height = ParseArgument(args, 1, DefaultHeight);

        var maze = new Maze(width, height);
        MyMaze = maze;
        if (ANIMATE)
        {
            RedrawMaze();
        }
        //MazeBuilders.TreeGrowing(maze, TreeGrowingMode.VerticalBias);
        MazeBuilders.RecursiveBacktracking(maze);

        if (REMOVE_DEAD_ENDS)
        {
            MazeBuilders.RemoveDeadEnds(maze);
        }

        Console.WriteLine($"Maze ({width} x {height})");
        Console.WriteLine(MazeAsciiRenderer.Render(maze));
        DebugPrints.PrintStartAndEndLocs(maze);
    }

    public static void RedrawMaze()
    {
        Console.WriteLine(MazeAsciiRenderer.Render(MyMaze));
    }

    public static void DrawFrame()
    {
        Console.Clear();
        RedrawMaze();
        Thread.Sleep(ANIMATION_DELAY);
    }

    private static int ParseArgument(string[] args, int index, int defaultValue)
    {
        return args.Length > index && int.TryParse(args[index], out var value) && value > 0
            ? value
            : defaultValue;
    }
}
