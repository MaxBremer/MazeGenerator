namespace MazeGenerator;

internal class Program
{
    public const bool DefaultAnimate = true;
    public const int ANIMATION_DELAY = 150;

    private const bool DefaultRemoveDeadEnds = true;

    private const int DefaultWidth = 8;
    private const int DefaultHeight = 6;
    private const MazeAlgorithm DefaultAlgorithm = MazeAlgorithm.RecursiveBacktracking;
    private const TreeGrowingMode DefaultTreeGrowingMode = MazeBuilders.DefaultMode;

    private static Maze MyMaze;

    private enum MazeAlgorithm
    {
        RecursiveBacktracking,
        TreeGrowing,
        Prim,
        Kruskal,
        SnakePassage
    }

    static void Main(string[] args)
    {
        var width = ParseArgument(args, 0, DefaultWidth);
        var height = ParseArgument(args, 1, DefaultHeight);
        var algorithm = ParseEnumArgument(args, 2, DefaultAlgorithm);
        var treeGrowingMode = ParseEnumArgument(args, 3, DefaultTreeGrowingMode);
        var animate = ParseBooleanArgument(args, 4, DefaultAnimate);
        var removeDeadEnds = ParseBooleanArgument(args, 5, DefaultRemoveDeadEnds);

        var maze = new Maze(width, height);
        MyMaze = maze;
        Maze.Do_Animation = animate;
        if (animate)
        {
            RedrawMaze();
        }

        BuildMaze(maze, algorithm, treeGrowingMode);

        if (removeDeadEnds)
        {
            MazeBuilders.RemoveDeadEnds(maze);
        }

        Console.Clear();
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

    private static void BuildMaze(Maze maze, MazeAlgorithm algorithm, TreeGrowingMode treeGrowingMode)
    {
        switch (algorithm)
        {
            case MazeAlgorithm.TreeGrowing:
                MazeBuilders.TreeGrowing(maze, treeGrowingMode);
                break;
            case MazeAlgorithm.Prim:
                MazeBuilders.Prim(maze);
                break;
            case MazeAlgorithm.Kruskal:
                MazeBuilders.KruskalGeneration(maze);
                break;
            case MazeAlgorithm.SnakePassage:
                MazeBuilders.BuildSnakePassage(maze);
                break;
            default:
                MazeBuilders.RecursiveBacktracking(maze);
                break;
        }
    }

    private static int ParseArgument(string[] args, int index, int defaultValue)
    {
        return args.Length > index && int.TryParse(args[index], out var value) && value > 0
            ? value
            : defaultValue;
    }

    private static bool ParseBooleanArgument(string[] args, int index, bool defaultValue)
    {
        return args.Length > index && bool.TryParse(args[index], out var value)
            ? value
            : defaultValue;
    }

    private static TEnum ParseEnumArgument<TEnum>(string[] args, int index, TEnum defaultValue) where TEnum : struct, Enum
    {
        return args.Length > index && Enum.TryParse<TEnum>(args[index], ignoreCase: true, out var value)
            ? value
            : defaultValue;
    }
}
