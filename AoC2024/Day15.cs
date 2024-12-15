using AoC2024;
using Xunit;
using Xunit.Abstractions;
namespace AoC2024Day15Pt1;

public class Day15(ITestOutputHelper toh)
{

    public static string Sample = """
                                    ########
                                    #..O.O.#
                                    ##@.O..#
                                    #...O..#
                                    #.#.O..#
                                    #...O..#
                                    #......#
                                    ########

                                    <^^>>>vv<v>>v<<
                                  """;

    [Fact]
    public void ShouldSolvePt1()
    {
        var input = File.ReadAllText("TestAssets/day15.txt");
        var result = Solve(input);
        Assert.Equal(1485257, result);
    }

    [Fact]
    public void SimpleSample()
    {
        var initial = ParseInput(Sample);
        Assert.Equal((6, 6), initial.Warehouse.GetDims());

        var warehouse = new Warehouse(initial.Warehouse, initial.RobotLocation);

        foreach (var action in initial.Actions)
        {
            var result = warehouse.Move(action);
        }

        foreach (var line in warehouse.field)
        {
            var str = string.Join("", line.Select(x => x switch
            {
                WarehouseCellType.Box => 'O',
                WarehouseCellType.Wall => '#',
                WarehouseCellType.Empty => ' '
            }));
            toh.WriteLine(str);
        }
        Assert.Equal(2028, warehouse.SumOfGps());
    }

    public static int Solve(string input)
    {
        var initial = ParseInput(input);

        var warehouse = new Warehouse(initial.Warehouse, initial.RobotLocation);
        foreach (var action in initial.Actions)
        {
            warehouse.Move(action);
        }
        return warehouse.SumOfGps();
    }

    public static (WarehouseCellType[][] Warehouse, (int, int) RobotLocation, Direction[] Actions) ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
        (int, int)? robot = null;
        var warehouse = lines.TakeWhile(x => !string.IsNullOrWhiteSpace(x)).Skip(1).SkipLast(1).Select((x, idx) =>
        {
            var (line, roboCol) = ParseLine(x);
            if (roboCol.HasValue)
            {
                robot = (idx, roboCol.Value);
            }
            return line;
        }).ToArray();

        var actions = lines.SkipWhile(x => !string.IsNullOrWhiteSpace(x)).SelectMany(actionsSeq => actionsSeq.Select(c =>
        {
            return c switch
            {
                '^' => Direction.N,
                'v' => Direction.S,
                '>' => Direction.E,
                '<' => Direction.W,
                _ => throw new ArgumentException($"Cannot recognize action, char {c} is not supported")
            };
        })).ToArray();

        return (warehouse, robot!.Value, actions);
    }

    public static (WarehouseCellType[], int?) ParseLine(string line)
    {
        int? robot = null;
        return (line.Skip(1).SkipLast(1).Select((c, idx) =>
        {
            if (c == '@')
            {
                robot = idx;
            }
            return c switch
            {
                '.' => WarehouseCellType.Empty,
                '#' => WarehouseCellType.Wall,
                'O' => WarehouseCellType.Box,
                '@' => WarehouseCellType.Empty,
                _ => throw new ArgumentException($"Char {c} is not supported")
            };
        }).ToArray(), robot);
    }
}

public class Warehouse
{
    public WarehouseCellType[][] field;
    private (int, int) robotLocation;
    public Warehouse(WarehouseCellType[][] initial, (int, int) robotLocation)
    {
        field = initial;
        this.robotLocation = robotLocation;
    }

    public bool Move(Direction direction)
    {
        var v = direction.GetVector();
        if (!field.CheckBounds(robotLocation.Add(v)))
        {
            return false;
        }
        var cellTowards = field.GetFieldValue(robotLocation.Add(v));

        if (MoveInternal(direction, WarehouseCellType.Empty, cellTowards.Coordinates))
        {
            robotLocation = cellTowards.Coordinates;
            return true;
        }
        return false;
    }

    public int SumOfGps()
    {
        return field.EnumerateCoords()
        .Select(x => field.GetFieldValue(x)).Where(x => x.Value == WarehouseCellType.Box)
        .Aggregate(0, (acc, current) => acc + (current.Coordinates.Item1 + 1) * 100 + (current.Coordinates.Item2 + 1));
    }

    private bool MoveInternal(Direction direction, WarehouseCellType prev, (int, int) cell)
    {
        var v = direction.GetVector();

        if (!field.CheckBounds(cell))
        {
            return false;
        }


        var moveWhat = field.GetVal(cell);
        if (moveWhat == WarehouseCellType.Empty)
        {
            field.SetVal(cell, prev);
            return true;
        }
        if (moveWhat == WarehouseCellType.Wall)
        {
            return false;
        }
        if (moveWhat == WarehouseCellType.Box)
        {
            var couldMove = MoveInternal(direction, WarehouseCellType.Box, cell.Add(v));
            if (couldMove)
            {
                field.SetVal(cell, prev);
            }
            return couldMove;
        }

        throw new ArgumentException("impossible");
        // var moveTo = cell.Add(v);

        // if (!field.CheckBounds(moveTo))
        // {
        //     return false;
        // }


    }
}

public enum WarehouseCellType
{
    Box, Wall, Empty
}