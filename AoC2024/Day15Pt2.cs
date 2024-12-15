using System.Text;
using AoC2024;
using Xunit;
using Xunit.Abstractions;
namespace AoC2024Day;

using WarehouseCell = OneOf.OneOf<BigBox, WarehouseCellType>;

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

    }

    [Fact]
    public void SimpleSample()
    {
        var initial = ParseInput(Sample);

        initial.Warehouse.Print((str) => toh.WriteLine(str));

        // Assert.Equal((6, 6), initial.Warehouse.GetDims());

        // var warehouse = new Warehouse(initial.Warehouse, initial.RobotLocation);

        // foreach (var action in initial.Actions)
        // {
        //     var result = warehouse.Move(action);
        // }

        // foreach (var line in warehouse.field)
        // {
        //     var str = string.Join("", line.Select(x => x switch
        //     {
        //         WarehouseCellType.Box => 'O',
        //         WarehouseCellType.Wall => '#',
        //         WarehouseCellType.Empty => ' '
        //     }));
        //     toh.WriteLine(str);
        // }
        // Assert.Equal(2028, warehouse.SumOfGps());
    }

    public static int Solve(string input)
    {
        throw new NotImplementedException();

    }

    public static (Warehouse Warehouse, Direction[] Actions) ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
        (int, int)? robot = null;
        var warehouse = lines.TakeWhile(x => !string.IsNullOrWhiteSpace(x)).Select((x, idx) =>
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

        return (new Warehouse(warehouse, robot!.Value), actions);
    }

    public static (WarehouseCell[], int?) ParseLine(string line)
    {
        int? robot = null;
        return (line.SelectMany((c, idx) =>
        {
            IEnumerable<WarehouseCell> cell;
            if (c == '@')
            {
                robot = idx * 2;
            }
            var two = Enumerable.Range(0, 2);

            IEnumerable<WarehouseCell> MakeBigBox()
            {
                var box = new BigBox();
                return two.Select((_) => (WarehouseCell)box);
            }
            cell = c switch
            {
                '.' => two.Select((_) => (WarehouseCell)WarehouseCellType.Empty),
                '#' => two.Select((_) => (WarehouseCell)WarehouseCellType.Wall),
                'O' => MakeBigBox(),
                '@' => two.Select((_) => (WarehouseCell)WarehouseCellType.Empty),

                _ => throw new ArgumentException($"Char {c} is not supported")
            };
            return cell;
        }).ToArray(), robot);
    }
}

public class Warehouse
{
    public WarehouseCell[][] field;
    private (int, int) robotLocation;
    public Warehouse(WarehouseCell[][] initial, (int, int) robotLocation)
    {
        field = initial;
        this.robotLocation = robotLocation;
    }

    public bool Move(Direction direction)
    {
        throw new NotImplementedException();

    }

    public int SumOfGps()
    {
        throw new NotImplementedException();
    }

    private bool MoveInternal(Direction direction, WarehouseCellType prev, (int, int) cell)
    {

        throw new ArgumentException("impossible");
    }

    public void Print(Action<string> writeline)
    {
        foreach (var line in field)
        {

            var str = string.Join("", line.Select((x, i) => x.Match((box) =>
            {
                var anotherBox = line[i - 1];
                if (anotherBox.IsT0 && anotherBox.AsT0 == box)
                {
                    return "";
                }
                return "[]";
            }, rest =>
            {
                return rest switch
                {
                    WarehouseCellType.Empty => " ",
                    WarehouseCellType.Wall => "#",
                    _ => throw new NotImplementedException()
                };
            })));
            writeline(str);
        }
    }
}

public record BigBox();

public enum WarehouseCellType
{
    Wall, Empty
}