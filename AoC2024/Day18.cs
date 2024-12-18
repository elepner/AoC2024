using Xunit;
using Xunit.Abstractions;

namespace AoC2024;

public class Day18(ITestOutputHelper toh)
{
    [Fact]
    public void ShouldSolveSamplePt1()
    {
        var result = Solve(Sample.Input, 12, (7, 7));
        Assert.Equal(22, result);
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var result = Solve(File.ReadAllText("TestAssets/day17.txt"), 1024, (71, 71));
        Assert.Equal(324, result);
    }

    private static HashSet<(int, int)> ParseInput(string input, int count)
    {
        var points = input.Trim().Split(Environment.NewLine).Take(count).Select(x =>
        {
            var pairs = x.Trim().Split(",").Select(int.Parse).ToArray();
            return (pairs[1], pairs[0]);
        });
        return [..points];
    }

    public int Solve(string input, int corruptedCount, (int, int) dims)
    {
        var corrupted = ParseInput(input, corruptedCount);
        return FindCost(corrupted, dims);
    }

    private static int FindCost(HashSet<(int, int)> points, (int, int) dims)
    {
        var start = (0, 0);
        var end = dims.Sub((1, 1));

        var front = new Dictionary<(int, int), int> { { start, 0 } };
        var visited = new Dictionary<(int, int), int>();

        while (!visited.ContainsKey(end))
        {
            var current = front.MinBy(x => x.Value);

            front.Remove(current.Key);
            visited.Add(current.Key, current.Value);

            var neighbours = GetNeighbours(points, current.Key, dims).ToArray();

            foreach (var point in neighbours)
            {
                var currentCost = current.Value + 1;
                if (visited.ContainsKey(point))
                {
                    continue;
                }

                if (front.TryGetValue(point, out var existingCost))
                {
                    if (currentCost < existingCost)
                    {
                        front[point] = currentCost;
                    }
                }
                else
                {
                    front[point] = currentCost;
                }
                
            }

        }

        return visited[end];
    }

    private static IEnumerable<(int, int)> GetNeighbours(HashSet<(int, int)> corruptedMemory, (int, int) point, (int, int) dims)
    {
        return point.Around().Select(x => x.Point).Where(p => p.WithinBounds(dims)).Where(p => !corruptedMemory.Contains(p));
    }


    private static IEnumerable<string> ToPrint(HashSet<(int, int)> corrupted, HashSet<(int, int)> visited, (int, int) dims)
    {
        foreach (var i in Enumerable.Range(0, dims.Item1))
        {
            yield return string.Join("", Enumerable.Range(0, dims.Item2).Select(j =>
            {
                var p = (i, j);
                if (corrupted.Contains(p))
                {
                    return "#";
                }

                return ".";
            }));
        }
    }
}

static class Sample
{
    public const string Input = """
                                5,4
                                4,2
                                4,5
                                3,0
                                2,1
                                6,3
                                2,4
                                1,5
                                0,6
                                3,3
                                2,6
                                5,1
                                1,2
                                5,5
                                2,5
                                6,5
                                1,4
                                0,4
                                6,4
                                1,1
                                6,1
                                1,0
                                0,5
                                1,6
                                2,0
                                """;
}