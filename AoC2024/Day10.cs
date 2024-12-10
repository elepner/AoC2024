using System.Diagnostics;
using Xunit;

namespace AoC2024;

public class Day10
{
    private static readonly string Sample = """
                                            89010123
                                            78121874
                                            87430965
                                            96549874
                                            45678903
                                            32019012
                                            01329801
                                            10456732
                                            """;
    [Fact]
    public void ShouldSolveSample()
    {
        var input = ParseInput(Sample);
        var result = SolutionDy10.Solve(input);
        Assert.Equal(36, result);
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var field = ParseInput(File.ReadAllText("TestAssets/day10.txt"));
        var result = SolutionDy10.Solve(field);
        Assert.Equal(582, result);
    }

    [Fact]
    public void ShouldSolveSamplePt2()
    {
        var input = ParseInput(Sample);
        var result = SolutionDy10.SolvePt2(input);
        Assert.Equal(81, result);
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var field = ParseInput(File.ReadAllText("TestAssets/day10.txt"));
        var result = SolutionDy10.SolvePt2(field);
        Assert.Equal(1302, result);
    }

    private static MapHeight[][] ParseInput(string input)
    {
        return input
        .Split(Environment.NewLine)
        .Select(line => line.Select(c => new MapHeight(int.Parse(c.ToString()))).ToArray()).ToArray();
    }

}

static class SolutionDy10
{
    public static int Solve(MapHeight[][] field)
    {
        int count = 0;
        var zeros = field.EnumerateCoords().Select(xy => xy.GetFieldValue(field)).Where(x => x.Value.Height == 0);

        foreach (var zero in zeros)
        {
            var result = Bfs(field, zero.Coordinates);
            count += result;
        }
        return count;
    }

    public static int SolvePt2(MapHeight[][] field)
    {
        int count = 0;
        var zeros = field.EnumerateCoords().Select(xy => xy.GetFieldValue(field)).Where(x => x.Value.Height == 0);

        foreach (var zero in zeros)
        {
            Dfs(field, zero.Coordinates, ref count);
        }
        return count;
    }

    public static int Bfs(MapHeight[][] field, (int, int) start)
    {
        var value = field.GetVal(start);
        var front = new HashSet<(int, int)>()
        {
            start
        };
        var visited = new HashSet<(int, int)>();

        while (front.Count > 0)
        {
            var currentFront = front.ToArray();
            foreach (var el in currentFront)
            {
                front.Remove(el);
                visited.Add(el);
                var currentCell = el.GetFieldValue(field);

                var canGoTo = field.GetCanGoTo(el).Where(x => !front.Contains(x)
                                                            && !visited.Contains(x));

                foreach (var canGo in canGoTo)
                {
                    front.Add(canGo);
                }

            }
        }

        return visited.Count(x => x.GetFieldValue(field).Value.Height == 9);

    }
    private static void Dfs(MapHeight[][] field, (int, int) current, ref int finishCount)
    {
        var value = field.GetVal(current);

        if (value.Height == 9)
        {
            finishCount++;
            return;
        }

        var canGoTo = field.GetCanGoTo(current);

        foreach (var next in canGoTo)
        {
            Dfs(field, next, ref finishCount);
        }
    }

    private static IEnumerable<(int, int)> GetCanGoTo(this MapHeight[][] field, (int, int) point)
    {
        var value = field.GetVal(point);
        return CollectionExtension
            .AllDirections()
            .Select(x => x.GetVector())
            .Select(v => v.Add(point))
            .WithinBoundsOf(field)
            .Where(x => field.GetVal(x).Height == value.Height + 1);
    }
}

record struct MapHeight(int Height)
{
    public override string ToString()
    {
        return Height.ToString();
    }
}