
using System.Text.RegularExpressions;
using Xunit;
namespace AoC2024;

public class Day1
{

    readonly string inputStr = """
        3   4
        4   3
        2   5
        1   3
        3   9
        3   3
        """;

    [Fact]
    public void SampleCase()
    {
        Assert.Equal(11, Solve(inputStr, Solution.Solve));
    }

    [Fact]
    public void SampeCasePt2()
    {
        Assert.Equal(31, Solve(inputStr, Solution.SolvePt2));

    }

    [Fact]
    public void RealSolution()
    {
        var input = File.ReadAllText("TestAssets/day1.txt");
        Assert.Equal(1660292, Solve(input, Solution.Solve));
    }

    [Fact]
    public void RealSolutionPt2()
    {
        var input = File.ReadAllText("TestAssets/day1.txt");
        Assert.Equal(22776016, Solve(input, Solution.SolvePt2));
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

static class Solution
{
    public static int Solve(int[][] input)
    {
        var arr1 = input.Select(x => x[0]).ToArray();
        var arr2 = input.Select(x => x[1]).ToArray();

        Array.Sort(arr1);
        Array.Sort(arr2);

        return arr1.Zip(arr2).Select((els) =>
        {
            var (f, s) = els;
            return Math.Abs(f - s);
        }).Sum();
    }

    public static int SolvePt2(int[][] input)
    {

        var left = input.Select(x => x[0]);

        var right = input.Select(x => x[1]);

        return left.Select(l => l * right.Count(x => x == l)).Sum();

    }
}