using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AoC2024Pt1;

public class Day6
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
        Assert.Equal(41, result);
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var state = Parse(File.ReadAllText("TestAssets/day6.txt"));
        var result = SolutionDay6.Solve(state);
        Assert.Equal(5131, result);
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
        CellType[,] field = new CellType[rows, cols];
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
                    field[row, col] = CellType.VisitedCell;
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


        if (cellTypeInFrontOfGuard == CellType.Obstacle)
        {
            return state with { Guard = state.Guard with { Direction = (Direction)(((int)state.Guard.Direction + 1) % 4) } };
        }
        state.Field[cellInFrontOfGuard.Item1, cellInFrontOfGuard.Item2] = CellType.VisitedCell;
        return state with { Guard = state.Guard with { Location = cellInFrontOfGuard } };
    }

    public static int Solve(State state)
    {
        State? s = state;
        while (s != null)
        {
            s = MakeStep(s);
        }

        var count = 0;
        foreach (var cellType in state.Field)
        {
            if (cellType == CellType.VisitedCell)
            {
                count++;
            }

        }

        return count;
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

    private static CellType GetCell(this CellType[,] field, (int, int) v)
    {

        return field[v.Item1, v.Item2];

    }

    private static bool CheckBounds(int val, int max)
    {
        return val >= 0 && val < max;
    }

}

record State(CellType[,] Field, GuardInfo Guard);

enum CellType
{
    Obstacle,
    Cell,
    VisitedCell
}

record GuardInfo((int, int) Location, Direction Direction);

enum Direction
{
    N = 0,
    E = 1,
    S = 2,
    W = 3
}