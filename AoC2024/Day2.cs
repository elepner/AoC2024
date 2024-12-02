
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
namespace AoC2024;

public class Day2(ITestOutputHelper toh)
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

    [Fact]
    public void SolvePt2()
    {
        var result = Solve(File.ReadAllText("TestAssets/day2.txt"), SolutionDay2.SolvePt2);
        toh.WriteLine($"Solutions count {result}");
        Assert.True(result > 500);
        Assert.NotEqual(602, Solve(File.ReadAllText("TestAssets/day2.txt"), SolutionDay2.SolvePt2));
        Assert.NotEqual(513, Solve(File.ReadAllText("TestAssets/day2.txt"), SolutionDay2.SolvePt2));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    public void SolvePt2Line(int lineNo, bool expectedResult)
    {
        var result = SolutionDay2.IsLineOk(ParseInput(inputStr)[lineNo]);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(new int[] { 19, 20, 21, 22, 23, 25, 26, 30 })]
    [InlineData(new int[] { 1, 10, 12 })]
    [InlineData(new int[] { 10, 12, 33 })]
    [InlineData(new int[] { 10, 12, 13, 33 })]
    [InlineData(new int[] { 10, 11, 50, 12, 13 })]
    [InlineData(new int[] { 11, 50, 12 })]

    public void Foo(int[] arr)
    {
        Assert.True(SolutionDay2.IsLineOk(arr));
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

        var foo = input.Where(x => !IsLineOk(x)).ToArray();

        return input.Where(x => IsLineOk(x)).Count();

    }

    public static bool IsLineOk(int[] line, int? direction = null, int depth = 0)
    {
        if (depth > 1)
        {
            return false;
        }
        if (direction == null)
        {
            direction = GetDirection(line);
        }

        if (direction == 0)
        {
            return IsLineOk(line.Where((x, idx) => idx != 0).ToArray(), null, depth + 1) || IsLineOk(line.Where((x, idx) => idx != 1).ToArray(), null, depth + 1);
        }

        for (int i = 0; i < line.Length - 1; i++)
        {
            int current = line[i];
            int next = line[i + 1];
            int diff = (next - current) * direction.Value;
            if (diff is > 0 and <= 3)
            {
                continue;
            }

            var rest = line.Where((x, idx) => idx != i).ToArray();

            var isOk = IsLineOk(rest, null, depth + 1);

            if (!isOk && i == line.Length - 2)
            {
                return IsLineOk(line.Where((x, idx) => idx != line.Length - 1).ToArray(), null, depth + 1);
            }

            return isOk;
        }
        return true;
    }

    private static int GetDirection(int[] line)
    {
        var first = line[1];
        var second = line[0];
        return first.CompareTo(second);

    }
}