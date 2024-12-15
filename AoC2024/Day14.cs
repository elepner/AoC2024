using Xunit;
using Xunit.Abstractions;

namespace AoC2024;

public class Day14(ITestOutputHelper toh)
{
    private static string Sample = """
                                    p=0,4 v=3,-3
                                    p=6,3 v=-1,-3
                                    p=10,3 v=-1,2
                                    p=2,0 v=2,-1
                                    p=0,0 v=1,3
                                    p=3,0 v=-2,-2
                                    p=7,6 v=-1,-3
                                    p=3,0 v=-1,-2
                                    p=9,3 v=2,3
                                    p=7,3 v=-1,2
                                    p=2,4 v=2,-3
                                    p=9,5 v=-3,-3
                                   """;

    [Fact]
    public static void SolveOne()
    {
        var result = ParseInput("p=2,4 v=2,-3");

        var after = result.Select(x => Simulate(x, (11, 7), 5)).Single();
        Assert.Equal((1, 3), after);
    }

    [Fact]
    public static void SolveSamplePt1()
    {
        Assert.Equal(12, SolvePt1(Sample, (7, 11), 100));
    }

    [Fact]
    public void CheckXmasTree()
    {
        var input = File.ReadAllText("TestAssets/day14.txt");
        var robots = ParseInput(input);

        var dims = (101, 103);


        var res = robots.Select(r => Simulate(r, dims, 7790)).ToArray();

        var result = res.ToHashSet();
        if (res.Length == result.Count)
        {
            var levels = Enumerable.Range(0, dims.Item2).Select(
            level => string.Join("", Enumerable.Range(0, dims.Item1)
            .Select(w => result.Contains((w, level)) ? "X" : " "))
            );

            foreach (var lvl in levels)
            {
                toh.WriteLine(lvl);
            }
        }
    }

    [Fact]
    public static void ShouldSolvePt1()
    {
        var result = SolvePt1(File.ReadAllText("TestAssets/day14.txt"), (101, 103), 100);
        Assert.Equal(228421332, result);
    }

    public static long SolvePt1(string input, (int, int) dims, int t)
    {
        var robots = ParseInput(input);

        var after = robots.Select(x => Simulate(x, dims, t));
        var result = Count(after, dims);
        return result;
    }

    public static long Count(IEnumerable<(int, int)> els, (int, int) dims)
    {
        var middle = dims.Map(x => x / 2);

        var quadrands = CollectionExtension
        .AllDirections()
        .Select((dir, i) => new
        {
            Vectors = (dir.GetVector(), dir.Rotate90(false).GetVector()),
            Id = i
        })
        .ToArray();

        var q = els
        .Where(x => x.Sub(middle).ToArray().All(x => x != 0))
        .GroupBy(v => quadrands.First(q => q.Vectors.ToArray().All(unitVector => unitVector.Dot(v.Sub(middle)) > 0)).Id)
        .Select(x => x.Count());

        return q.Aggregate(1L, (acc, curr) => curr * acc);

    }

    private static InitialCondition[] ParseInput(string input)
    {
        return input.Trim().Split(Environment.NewLine).Select((line) =>
        {
            var vectors = line
            .Split(' ')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(ParsePart)
            .ToArray();
            return new InitialCondition(vectors[0], vectors[1]);
        }).ToArray();
    }

    private static (int, int) ParsePart(string input)
    {
        var parts = input.Trim().Split("=")[1].Split(",").Select(x => int.Parse(x)).ToArray();
        return (parts[0], parts[1]);
    }

    private static (int, int) Simulate(InitialCondition start, (int, int) dims, int t)
    {
        return start.XY.Add(start.V.Scale(t)).MapWith(dims, (a, b) => mod(a, b));
    }

    private static int mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}

record InitialCondition((int, int) XY, (int, int) V);