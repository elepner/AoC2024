using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf;
using Xunit;


namespace AoC2024;
using Cell = OneOf.OneOf<CellType, VisitedCell>;
public class Day6Pt2
{
    private readonly string Sample = """
                                     ....#.....
                                     .........#
                                     ..........
                                     ..#.......
                                     .......#..
                                     ..........
                                     .#..^.....
                                     ........#.
                                     #.........
                                     ......#...
                                     """;

    [Fact]
    public void ShouldSolveSample()
    {
        var state = Parse(Sample);
        var result = SolutionDay6.Solve(state);
        Assert.Equal(6, result);
    }

    [Fact]
    public void ShouldSolveBruteForcePt1()
    {
        var state = Parse(File.ReadAllText("TestAssets/day6.txt"));
        var result = SolutionDay6.Solve(state);
        Assert.Equal(1784, result);
    }

    static State Parse(string input)
    {
        var charMapping = new Dictionary<char, OneOf<CellType, Direction>>()
        {
            {'.', CellType.Cell},
            {'#', CellType.Obstacle},
            {'^', Direction.N},
            {'>', Direction.E},
            {'<', Direction.W},
        };
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
        GuardInfo? guardInfo = null;

        var rows = lines.Length;
        var cols = lines[0].Length;
        Cell[,] field = new Cell[rows, cols];


        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var cell = charMapping[lines[i][j]];
                var row = i;
                var col = j;
                cell.Switch((cellType) =>
                {
                    field[row, col] = cellType;
                }, (direction) =>
                {
                    if (guardInfo != null)
                    {
                        throw new ArgumentException("Multiple guards are not allowed");
                    }

                    guardInfo = new GuardInfo((row, col), direction);
                    field[row, col] = new VisitedCell(0);
                });
            }
        }

        return new State(field, guardInfo);
    }
}

static class SolutionDay6
{

    public static State? MakeStep(State state)
    {
        var guardDirection = GetVector(state.Guard.Direction);
        var cellInFrontOfGuard = VAdd(state.Guard.Location, guardDirection);

        if (!CheckBounds(cellInFrontOfGuard.Item1, state.Field.GetLength(0)) ||
            !CheckBounds(cellInFrontOfGuard.Item2, state.Field.GetLength(1)))
        {
            return null;
        }

        var cellTypeInFrontOfGuard = state.Field.GetCell(cellInFrontOfGuard);

        return cellTypeInFrontOfGuard.Match((cell) =>
        {
            if (cell == CellType.Obstacle)
            {
                return state with { Guard = state.Guard with { Direction = (Direction)(((int)state.Guard.Direction + 1) % 4) } };
            }
            state.Field[cellInFrontOfGuard.Item1, cellInFrontOfGuard.Item2] = VisitedCell.FromDirection(state.Guard.Direction);
            return state with { Guard = state.Guard with { Location = cellInFrontOfGuard } };
        }, (visitedCell) =>
        {
            if (visitedCell.VisitedFromGivenDirection(state.Guard.Direction))
            {
                throw new CycleFoundException();
            }
            state.Field[cellInFrontOfGuard.Item1, cellInFrontOfGuard.Item2] = visitedCell.AddDirection(state.Guard.Direction);
            return state with { Guard = state.Guard with { Location = cellInFrontOfGuard } };
        });

        // return state with { Guard = state.Guard with { Location = cellInFrontOfGuard } };
    }

    private static bool IsCycled(State state)
    {
        State? s = state;
        while (s != null)
        {
            try
            {
                s = MakeStep(s);
            }
            catch (CycleFoundException)
            {
                return true;
            }

        }

        return false;
    }

    public static int Solve(State state)
    {
        State? s = state;

        var field = state.Field;


        var initial = Copy(field);

        if (IsCycled(state with { Field = initial }))
        {
            throw new ArgumentException("Initial should not be cycled");
        }



        var count = 0;

        Parallel.For(0, field.GetLength(0), (i) =>
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if (!initial[i, j].IsT1) continue;

                var cellType = field[i, j];
                if (cellType is not { IsT0: true, AsT0: CellType.Cell }) continue;

                var cp = Copy(field);
                cp[i, j] = CellType.Obstacle;
                if (IsCycled(state with { Field = cp }))
                {
                    Interlocked.Increment(ref count);
                }
            }
        });

        return count;
    }

    private static Cell[,] Copy(Cell[,] field)
    {
        var result = new Cell[field.GetLength(0), field.GetLength(1)];

        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                result[i, j] = field[i, j];
            }
        }
        return result;
    }

    private static (int, int) GetVector(Direction direction)
    {

        return direction switch
        {
            Direction.N => (-1, 0),
            Direction.S => (1, 0),
            Direction.E => (0, 1),
            Direction.W => (0, -1),
            _ => throw new ArgumentException()
        };
    }

    private static (int, int) VAdd((int, int) v1, (int, int) v2)
    {
        return (v1.Item1 + v2.Item1, v1.Item2 + v2.Item2);
    }

    private static Cell GetCell(this Cell[,] field, (int, int) v)
    {
        return field[v.Item1, v.Item2];
    }

    private static bool CheckBounds(int val, int max)
    {
        return val >= 0 && val < max;
    }

}

class CycleFoundException : Exception
{

}

record State(Cell[,] Field, GuardInfo Guard);

enum CellType
{
    Obstacle,
    Cell
}


record struct VisitedCell(byte VisitedFrom)
{
    public static VisitedCell FromDirection(Direction direction)
    {
        return new VisitedCell((byte)(1 << (byte)direction));
    }

    public bool VisitedFromGivenDirection(Direction direction)
    {
        var mask = (byte)(1 << (byte)direction);
        return (VisitedFrom & mask) != 0;
    }

    public VisitedCell AddDirection(Direction direction)
    {
        var mask = (byte)(1 << (byte)direction);
        return new VisitedCell((byte)(mask | VisitedFrom));
    }
}
record GuardInfo((int, int) Location, Direction Direction);
