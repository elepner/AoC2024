
using System.Text.RegularExpressions;
using Xunit;
namespace AoC2024;

public class Day2
{

    readonly string inputStr = """
        7 6 4 2 1
        1 2 7 8 9
        9 7 6 2 1
        1 3 2 4 5
        8 6 4 4 1
        1 3 6 7 9
        """;


    [Fact]
    public void SolveSamplePt1()
    {
        Assert.Equal(2, Solve(inputStr, SolutionDay2.Solve));
    }

    [Fact]
    public void SolvePt1()
    {
        Assert.Equal(472, Solve(File.ReadAllText("TestAssets/day2.txt"), SolutionDay2.Solve));
    }

    private int Solve(string inputString, Func<int[][], int> solver)
    {
        return solver(ParseInput(inputString));
    }
    private static int[][] ParseInput(string input)
    {
        return input.Trim().Split(Environment.NewLine)
        .Select(static x => Regex.Split(x.Trim(), @"\s+").Select(static x => int.Parse(x)).ToArray())
        .ToArray();
    }
}

static class SolutionDay2
{
    public static int Solve(int[][] input)
    {
        return input.Where((line) =>
        {
            var next = line[1];
            var mul = next > line[0] ? 1 : -1;

            var fit = line.Zip(line.Skip(1)).All((tuple) =>
            {
                var diff = (tuple.Second - tuple.First) * mul;
                return diff is > 0 and <= 3;
            });
            return fit;
        }).Count();

    }

    public static int SolvePt2(int[][] input)
    {

        var left = input.Select(x => x[0]);

        var right = input.Select(x => x[1]);

        return left.Select(l => l * right.Count(x => x == l)).Sum();

    }
}