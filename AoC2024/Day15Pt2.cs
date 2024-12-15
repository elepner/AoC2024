using System.Diagnostics;
using System.Text;
using AoC2024;
using Xunit;
using Xunit.Abstractions;
namespace AoC2024Day;

using WarehouseCell = OneOf.OneOf<BigBox, WarehouseCellType>;

public class Day15(ITestOutputHelper toh)
{

    public const string Sample = """
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


    public const string TrickyOne = """
                                    ########
                                    #..O.O.#
                                    ##@OO..#
                                    #...O..#
                                    #.#.O..#
                                    #...O..#
                                    #......#
                                    ########

                                    >>
                                  """;


    public const string LargerExample = """
        ##########
        #..O..O.O#
        #......O.#
        #.OO..O.O#
        #..O@..O.#
        #O#..O...#
        #O..O..O.#
        #.OO.O.OO#
        #....O...#
        ##########

        <vv>^<v^>v>^vv^v>v<>v^v<v<^vv<<<^><<><>>v<vvv<>^v^>^<<<><<v<<<v^vv^v>^
        vvv<<^>^v^^><<>>><>^<<><^vv^^<>vvv<>><^^v>^>vv<>v<<<<v<^v>^<^^>>>^<v<v
        ><>vv>v^v^<>><>>>><^^>vv>v<^^^>>v^v^<^^>v^^>v^<^v>v<>>v^v^<v>v^^<^^vv<
        <<v<^>>^^^^>>>v^<>vvv^><v<<<>^^^vv^<vvv>^>v<^^^^v<>^>vvvv><>>v^<<^^^^^
        ^><^><>>><>^^<<^^v>>><^<v>^<vv>>v>>>^v><>^v><<<<v>>v<v<v>vvv>^<><<>^><
        ^>><>^v<><^vvv<^^<><v<<<<<><^v<<<><<<^^<v<^^^><^>>^<v^><<<^>>^v<v^v<v^
        >^>>^v>vv>^<<^v<>><<><<v<<v><>v<^vv<<<>^^v^>^^>>><<^v>>v^v><^^>>^<>vv^
        <><^^>^^^<><vvvvv^v<v<<>^v<v>v<<^><<><<><<<^^<<<^<<>><<><^^^>^^<>^>v<>
        ^^>vv<^v^v<vv>^<><v<^v>^^^>>>^^vvv^>vvv<>>>^<^>>>>>^<<^v>^vvv<>^<><<v>
        v^^>>><<^^<>>^v^<v^vv<>v^<<>^<^v^v><^<<<><<^<v><v<>vv>>v><v^<vv<>v^<<^
    """;

    private static readonly Dictionary<string, string> Samples = new Dictionary<string, string>() {
        {"Large", LargerExample},
        {"Simple", Sample},
        {"Medium", TrickyOne}
    };

    [Fact]
    public void ShouldSolvePt2()
    {
        var input = File.ReadAllText("TestAssets/day15.txt");
        var result = Solve(input);
        WriteLine(result.ToString());
    }

    [Fact]
    public void ShouldSolveLargeSample()
    {
        var result = Solve(LargerExample, (str) => WriteLine(str));

        WriteLine("Final result");

        Assert.Equal(9021, result);
    }

    [Fact]
    public void TrickyCaseWithDiamond()
    {

        string sample = """
                                    ########
                                    #......#
                                    #...O@.#
                                    ##.OO..#
                                    #...O..#
                                    #.#....#
                                    #......#
                                    #......#
                                    ########

                                    <>vv<v<^
                                  """;
        Solve(sample, (str) => WriteLine(str));
    }

    [InlineData("Large")]
    [InlineData("Simple")]
    [InlineData("Medium")]
    [Theory]
    public void SimpleSample(string input)
    {
        var initial = ParseInput(Samples[input]);

        initial.Warehouse.Print(WriteLine);
        for (int i = 0; i < initial.Actions.Length; i++)
        {
            var action = initial.Actions[i];
            WriteLine("Moving to: " + action.ToString());
            initial.Warehouse.Move(action);
            initial.Warehouse.Print(WriteLine);
            WriteLine("-----Step completed------");
        }

    }

    public static int Solve(string input, Action<string>? writeLine = null)
    {
        var task = ParseInput(input);
        for (int i = 0; i < task.Actions.Length; i++)
        {
            var action = task.Actions[i];
            Debug.WriteLine($"Moving to {action}");
            //if (i > 426)

            task.Warehouse.Print((str) => System.Diagnostics.Debug.WriteLine(str));

            task.Warehouse.Move(action);
            Debug.WriteLine($"Result after move is {action}");

            task.Warehouse.Print((str) => System.Diagnostics.Debug.WriteLine(str));


            task.Warehouse.CheckField();
        }
        if (writeLine != null)
        {
            task.Warehouse.Print((str) => writeLine(str));

        }
        return task.Warehouse.SumOfGps();
    }

    public static (Warehouse Warehouse, Direction[] Actions) ParseInput(string input)
    {
        int boxId = 0;
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
        (int, int)? robot = null;
        var warehouse = lines.TakeWhile(x => !string.IsNullOrWhiteSpace(x)).Select((x, idx) =>
        {
            var (line, roboCol) = ParseLine(x, ref boxId);
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
    private void WriteLine(string str)
    {
        System.Diagnostics.Debug.WriteLine(str);
        toh.WriteLine(str);
    }
    private static (WarehouseCell[], int?) ParseLine(string line, ref int boxId)
    {
        int? robot = null;
        var id = boxId;
        var result = (line.SelectMany((c, idx) =>
        {
            IEnumerable<WarehouseCell> cell;
            if (c == '@')
            {
                robot = idx * 2;
            }
            var two = Enumerable.Range(0, 2);

            IEnumerable<WarehouseCell> MakeBigBox()
            {
                var box = new BigBox(id++);
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
        boxId = id;
        return result;
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
        var target = direction.GetVector().Add(robotLocation);

        var action = MoveInternal(direction, target, new HashSet<int>());
        if (action != null)
        {
            action();
            robotLocation = target;
        }

        return action != null;
    }


    private Action? MoveInternal(Direction direction, (int, int) cell, HashSet<int> pushedBoxes)
    {
        var v = direction.GetVector();
        var target = cell.Add(v);
        if (!field.CheckBounds(target))
        {
            return null;
        }
        var toMove = cell.GetFieldValue(field).Value;
        var targetCell = field.GetVal(target);

        return toMove.Match(box =>
        {

            Action? moveAction = null;
            if (direction is Direction.W or Direction.E)
            {
                if (!field.GetVal(target).IsT0)
                {
                    Print((str) => System.Diagnostics.Debug.WriteLine(str));

                }
                if (field.GetVal(target).AsT0 != box)
                {
                    throw new Exception("impossible");
                }
                var action = MoveInternal(direction, target.Add(v), pushedBoxes);
                if (action != null)
                {
                    moveAction = () =>
                    {
                        action();
                        field.SetVal(cell, WarehouseCellType.Empty);
                        field.SetVal(target.Add(v), toMove);
                    };
                }
            }
            else
            {

                var foo = new int[] { -1, 1 }.Select(i => cell.Add((0, i))).Where(x => field.GetVal(x).IsT0 && field.GetVal(x).AsT0 == box).ToArray();
                if (foo.Length == 0)
                {
                    var bar = new int[] { -1, 1 }.Select(i => cell.Add((0, i))).Where(x => field.GetVal(x).IsT0).ToArray();

                    Print((str) => Debug.WriteLine(str));
                }

                var otherPart = new int[] { -1, 1 }.Select(i => cell.Add((0, i))).Single(x => field.GetVal(x).IsT0 && field.GetVal(x).AsT0 == box).GetFieldValue(field);

                var action1 = MoveInternal(direction, target, pushedBoxes);
                var action2 = MoveInternal(direction, otherPart.Coordinates.Add(v), pushedBoxes);

                if (action1 == null || action2 == null)
                {
                    return null;
                }

                if (pushedBoxes.Contains(box.Id))
                {
                    moveAction = () => { };
                }
                else
                {
                    moveAction = () =>
                                    {
                                        action1();
                                        action2();
                                        field.SetVal(cell, WarehouseCellType.Empty);
                                        field.SetVal(target, toMove);

                                        field.SetVal(otherPart.Coordinates, WarehouseCellType.Empty);
                                        field.SetVal(otherPart.Coordinates.Add(v), toMove);
                                    };
                    pushedBoxes.Add(box.Id);
                }


            }
            return moveAction;
        }, simpleCell =>
        {
            if (simpleCell == WarehouseCellType.Wall)
            {
                return null;
            }
            else if (simpleCell == WarehouseCellType.Empty)
            {
                return () => { };
            }
            throw new Exception("ffff");
        });



        throw new ArgumentException("impossible");
    }

    public int SumOfGps()
    {

        return field
        .EnumerateCoords()
        .Select(x => x.GetFieldValue(field))
        .Where(x => x.Value.IsT0).GroupBy(x => x.Value.AsT0.Id)
        .Select(x => x.MinBy(y => y.Coordinates.Item2))
        .Aggregate(0, (acc, current) => acc + (current.Coordinates.Item1) * 100 + (current.Coordinates.Item2));
    }

    public void CheckField()
    {
        var (rows, cols) = field.GetDims();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var cell = field.GetVal((i, j));
                if (cell.IsT0)
                {
                    var next = field.GetVal((i, j + 1));
                    if (!next.IsT0)
                    {
                        throw new Exception($"Broken cell Id: {cell.AsT0.Id} {cell.AsT0}, {(i, j)}");
                    }

                    if (next.AsT0.Id != cell.AsT0.Id)
                    {
                        throw new Exception($"Broken cell, Id do not match Id: {cell.AsT0.Id} {cell.AsT0}, {(i, j)}");
                    }
                    j++;
                }
            }
        }
    }

    public void Print(Action<string> writeline)
    {
        for (int row = 0; row < field.Length; row++)
        {
            WarehouseCell[] line = field[row];
            var str = string.Join("", line.Select((x, col) =>
            {
                if (this.robotLocation == (row, col))
                {
                    return "+";
                }
                return x.Match((box) =>
                {

                    return box.ToString();
                }, rest =>
                {
                    return rest switch
                    {
                        WarehouseCellType.Empty => ".",
                        WarehouseCellType.Wall => "#",
                        _ => throw new NotImplementedException()
                    };
                }); ;
            }));
            writeline(str);
        }
    }
}

class BoxIdCounter
{
    public int Count { get; set; }
}

public record BigBox(int Id)
{
    public override string ToString()
    {
        if (Id <= 9)
        {
            return Id.ToString();
        }
        else if (Id is > 250 && Id < 300)
        {
            return ToChar(Id - 250).ToString();

        }
        return ToChar(Id % 52).ToString();
    }

    private static char ToChar(int val)
    {
        if (val <= 26)
        {
            // Map 1-26 to 'A'-'Z'
            return (char)('A' + (val - 1));
        }
        else
        {
            // Map 27-52 to 'a'-'z'
            return (char)('a' + (val - 27));
        }
    }
}

public enum WarehouseCellType
{
    Wall, Empty
}