using System.Text.RegularExpressions;
using Xunit;

namespace AoC2024;

public class Day13
{
    public static readonly string Sample = """
                                           Button A: X+94, Y+34
                                           Button B: X+22, Y+67
                                           Prize: X=8400, Y=5400
                                           
                                           Button A: X+26, Y+66
                                           Button B: X+67, Y+21
                                           Prize: X=12748, Y=12176
                                           
                                           Button A: X+17, Y+86
                                           Button B: X+84, Y+37
                                           Prize: X=7870, Y=6450
                                           
                                           Button A: X+69, Y+23
                                           Button B: X+27, Y+71
                                           Prize: X=18641, Y=10279
                                           """;

    [Fact]
    public static void CheckSoluvitons()
    {
        var result = ParseInput(Sample).Select(x => x.Solve()).ToArray();

        (int, int)?[] expected = [
            (80, 40),
            null,
            (38, 86),
            null
        ];

        Assert.Equal(expected, result);
    }

    [Fact]
    public static void ShouldSolveSample()
    {
        Assert.Equal(480, Solve(Sample));
    }

    [Fact]
    public static void ShouldSolvePt1()
    {
        var result = Solve(File.ReadAllText("TestAssets/day13.txt"));
        Assert.True(result > 27647);
    }

    private static int Solve(string input)
    {
        return ParseInput(input).Select(x => x.Solve()).Where(x => x != null)
            .Select((solution) => solution.Value.Item1 * 3 + solution.Value.Item2)
            .Aggregate(0, (acc, val) => acc + val);
    } 

    static Regex pattern = new Regex(@"(?<number>\d+)");
    
    static Equation[] ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).ToArray();
        
        var equations = lines.Chunk(4).Select(chunk =>
        {
            

            var numbers = Enumerable.Range(0, 3).SelectMany((i) =>
            {
                var match = pattern.Matches(chunk[i]).ToArray().Select(x => x.Value).Select(int.Parse);
                return match;
            }).ToArray();
            //var eq = //Enumerable.Empty<string>().Concat(new int[]{0, 1}.Select())

            return new Equation(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
        }).ToArray();



        return equations;
    }
}

record Equation(int a, int b, int c, int d, int X, int Y)
{
    public (int, int)? Solve()
    {
        var M = (X - (double)c / d * Y) / (a - (double)b * c / d);
        if (M < 0)
        {
            return null;
        }
        double fractionalPart = M - Math.Truncate(M);
        if (Math.Abs(fractionalPart) > 0.000001)
        {
            return null;
        }

        var N = (X - M * a) / c;

        

        return ((int)M, (int)N);
    }
};