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

        initial.Warehouse.Print(WriteLine);
        for (int i = 0; i < initial.Actions.Length; i++)
        {
            var action = initial.Actions[i];
            WriteLine("Moving to: " + action.ToString());
            initial.Warehouse.Move(action);
            initial.Warehouse.Print(WriteLine);
            WriteLine("-----Step completed------");
        }
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

        var action = MoveInternal(direction, target);
        if (action != null)
        {
            action();
            robotLocation = target;
        }

        return action != null;
    }


    private Action? MoveInternal(Direction direction, (int, int) cell)
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
                var action = MoveInternal(direction, target.Add(v));
                if (action != null)
                {
                    moveAction = () =>
                    {
                        field.SetVal(cell, WarehouseCellType.Empty);
                        field.SetVal(target.Add(v), toMove);
                    };
                }
            }
            else
            {
                var otherPart = new int[] { -1, 1 }.Select(i => cell.Add((0, i))).Single(x => field.GetVal(x).IsT0 && field.GetVal(x).AsT0 == box).GetFieldValue(field);

                var action1 = MoveInternal(direction, target);
                var action2 = MoveInternal(direction, otherPart.Coordinates.Add(v));

                if (action1 == null || action2 == null)
                {
                    return null;
                }
                moveAction = () =>
                {
                    action1();
                    action2();
                    field.SetVal(cell, WarehouseCellType.Empty);
                    field.SetVal(target, toMove);

                    field.SetVal(otherPart.Coordinates, WarehouseCellType.Empty);
                    field.SetVal(otherPart.Coordinates.Add(v), toMove);
                };
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
        throw new NotImplementedException();
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
                    return "@";
                }
                return x.Match((box) =>
                {

                    return box.Id.ToString();
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

public record BigBox(int Id);

public enum WarehouseCellType
{
    Wall, Empty
}