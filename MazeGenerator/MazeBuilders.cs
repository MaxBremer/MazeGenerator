namespace MazeGenerator;

internal static class MazeBuilders
{
    public static void BuildSnakePassage(Maze maze)
    {
        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width - 1; x++)
            {
                maze.Connect(x, y, Direction.East);
            }

            if (y < maze.Height - 1)
            {
                var connectorColumn = y % 2 == 0 ? maze.Width - 1 : 0;
                maze.Connect(connectorColumn, y, Direction.South);
            }
        }

        maze.SetStart(0, 0, Direction.North);
        maze.SetEnd(maze.Width - 1, maze.Height - 1, Direction.South);
    }
}
