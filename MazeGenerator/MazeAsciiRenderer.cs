using System.Text;

namespace MazeGenerator;

internal static class MazeAsciiRenderer
{
    public static string Render(Maze maze)
    {
        var output = new StringBuilder();

        output.Append('+');
        for (var x = 0; x < maze.Width; x++)
        {
            output.Append("---+");
        }

        output.AppendLine();

        for (var y = 0; y < maze.Height; y++)
        {
            var middleLine = new StringBuilder("|");
            var bottomLine = new StringBuilder("+");

            for (var x = 0; x < maze.Width; x++)
            {
                var cell = maze.GetCell(x, y);

                middleLine.Append("   ");
                middleLine.Append(cell.IsConnected(Direction.East) ? ' ' : '|');

                bottomLine.Append(cell.IsConnected(Direction.South) ? "   " : "---");
                bottomLine.Append('+');
            }

            output.AppendLine(middleLine.ToString());
            output.AppendLine(bottomLine.ToString());
        }

        return output.ToString();
    }
}
