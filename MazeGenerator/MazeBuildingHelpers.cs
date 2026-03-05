using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    internal static class MazeBuildingHelpers
    {
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
            foreach (var cell in m.GetAllCells())
            {
                foreach (var dir in Enum.GetValues<Direction>())
                {
                    if (m.CanConnect(cell.X, cell.Y, dir))
                    {
                        var (dx, dy) = dir.ToOffset();
                        var adjacentCell = m.GetCell(cell.X + dx, cell.Y + dy);
                        edges.Add(new Edge { Cell1 = cell, Cell2 = adjacentCell });
                    }
                }
            }

            return edges;
        }

        internal static bool areInSameGroup(List<List<Cell>> groups, Cell c1, Cell c2)
        {
            foreach (var group in groups)
            {
                if (group.Contains(c1) && group.Contains(c2))
                {
                    return true;
                }
            }
            return false;
        }

        internal static List<Cell> groupFromCell(List<List<Cell>> groups, Cell cell)
        {
            foreach (var group in groups)
            {
                if (group.Contains(cell))
                {
                    return group;
                }
            }
            throw new InvalidOperationException($"Cell ({cell.X}, {cell.Y}) is not in any group.");
        }

        internal static int SelectFrontierIndex(int frontierCount, TreeGrowingMode mode)
        {
            return mode switch
            {
                TreeGrowingMode.NewestCell or TreeGrowingMode.HorizontalBias or TreeGrowingMode.VerticalBias => frontierCount - 1,
                TreeGrowingMode.OldestCell => 0,
                _ => Random.Shared.Next(frontierCount)
            };
        }

        internal static Cell SelectTreeGrowingNextCell(Maze maze, Cell from, List<Cell> options, TreeGrowingMode mode)
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

        internal static Cell WeightedDirectionChoice(Maze maze, Cell from, List<Cell> options, int horizontalWeight, int verticalWeight)
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

        internal static Cell WeightedDirectionChoiceFunc(Maze maze, Cell from, List<Cell> options, Func<Cell, Cell, int> weightFunc)
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

        internal static Func<Cell, Cell, int> BuildDirectionPrefFunc(Direction preferredDirection, int preferredWeight = 4, int otherWeight = 1)
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
    }
}
