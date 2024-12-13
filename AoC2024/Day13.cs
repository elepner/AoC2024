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
    public static void CheckSolutions()
    {
        var result = ParseInput(Sample).Select(x => x.Solve()).ToArray();

        (long, long)?[] expected = [
            (80, 40),
            null,
            (38, 86),
            null
        ];

        Assert.Equal(expected, result);
    }

    [Fact]
    public static void CheckSolutionsPt2()
    {
        var result = ParseInput(Sample).Select(x => Tweak(x).Solve()).Select(x => x.HasValue).ToArray();

        bool[] expected = [
            false,
            true,
            false,
            true
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
        
        Assert.Equal(35082, result);
    }



    [Fact]
    public static void ShouldSolvePt2()
    {
        
        var result = SolvePt2(File.ReadAllText("TestAssets/day13.txt"));
        
        Assert.True(result > 80212778885414);
    }


    private static Equation Tweak(Equation eq)
    {
        var increase = 10000000000000;
        return eq with { X = eq.X + increase, Y = eq.Y + increase };
    }

    private static long SolvePt2(string input)
    {
        return Solve(input, Tweak);
    }

    private static long Solve(string input, Func<Equation, Equation>? preprocess = null)
    {

        preprocess ??= (x) => x;

        return ParseInput(input).Select(eq => preprocess(eq)).Select(x => x.Solve())
            .Select((solution) =>
            {
                if (solution == null)
                {
                    return 0;
                }

                return solution.Value.Item1 * 3 + solution.Value.Item2;
            })
            .Aggregate(0l, (acc, val) => acc + val);
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

            return new Equation(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
        }).ToArray();

        return equations;
    }
}

record Equation(long a, long b, long c, long d, long X, long Y)
{
    public (long, long)? Solve()
    {
        var M = (X - (double)c * Y / d) / (a - (double)b * c / d);


        if (!CheckDouble(M, out var mResult))
        {
            return null;
        }

        var N = (X - M * a) / c;

        if (!CheckDouble(N, out var nResult))
        {
            return null;
        }


        return (mResult, nResult);
    }
    private static bool CheckDouble(double val, out long result)
    {
        result = -1;
        if (val < 0)
        {
            return false;
        }

        double rounded = Math.Round(val);
        double fractionalPart = val - rounded;
        if (Math.Abs(fractionalPart) > 0.001)
        {
            return false;
        }

        result = (long)rounded;
        return true;
    }
};
