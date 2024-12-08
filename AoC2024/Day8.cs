using Xunit;
using Xunit.Abstractions;

namespace AoC2024;
using Cell = OneOf.OneOf<Antenna, Antinode>;

public class Day8(ITestOutputHelper toh)
{
    private static readonly string Sample = """
                                            ............
                                            ........0...
                                            .....0......
                                            .......0....
                                            ....0.......
                                            ......A.....
                                            ............
                                            ............
                                            ........A...
                                            .........A..
                                            ............
                                            ............
                                            """;

    [Fact]
    public void ShouldSolveSample()
    {
        var result = SolutionDay8.SolvePt1(ParseInput(Sample), (s) => toh.WriteLine(s));
        Assert.Equal(14, result);
    }

    static Cell?[][] ParseInput(string input)
    {
        var result = input.Split(Environment.NewLine).Select(s => s.Select(c =>
        {
            Cell? result = null;
            if (c != '.')
            {
                result = Cell.FromT0(new Antenna(c));
            }
            return result;
        }).ToArray()).ToArray();

        return result;
    }
}

static class SolutionDay8
{
    public static int SolvePt1(Cell?[][] field, Action<string>? writeLine = null)
    {
        var groups = field.EnumerateCoords().Select(x => field.GetFieldValue(x)).Where(x => x.Value != null).GroupBy(x => x.Value).ToArray();


        foreach (var gr in groups)
        {
            foreach (var (el1, el2) in gr.ToArray().AllCombinations())
            {
                var delta = el2.Coordinates.Sub(el1.Coordinates);
                Assert.True(delta.Item1 >= 0);

                (int, (int, int))[] els = [(-1, el1.Coordinates), (1, el2.Coordinates)];

                foreach (var (scale, coords) in els)
                {
                    var target = coords.Add(delta.Scale(scale));
                    if (field.CheckBounds(target))
                    {
                        var value = field.GetVal(target);
                        if (value == null)
                        {
                            field.SetVal(target, new Antinode());
                        }
                    }
                }
            }
        }

        var lines = field.Select(line => line.Select((x) =>
        {
            if (x == null)
            {
                return ".";
            }
            return x.Value.Match(x => x.ToString(), y => y.ToString());
        })).Select(line => string.Join("", line));

        foreach (var line in lines)
        {
            writeLine?.Invoke(line);
        }
        return field.EnumerateCoords().Select(x => field.GetVal(x)).Count(val => val != null && val.Value.IsT1);
    }

    private static (int, int) GetDims<T>(this T[][] filed)
    {
        return (filed.Length, filed[0].Length);
    }
}

record Antenna(char Type)
{
    public override string ToString()
    {
        return Type.ToString();
    }
};
record Antinode()
{
    public override string ToString()
    {
        return "#";
    }
};

