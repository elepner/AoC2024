using Microsoft.AspNetCore.Components.Forms;
using Xunit;
using Xunit.Abstractions;
namespace AoC2024;


public class Day7
{
    private static string Sample = """
                                190: 10 19
                                3267: 81 40 27
                                83: 17 5
                                156: 15 6
                                7290: 6 8 6 15
                                161011: 16 10 13
                                192: 17 8 14
                                21037: 9 7 18 13
                                292: 11 6 16 20
                                """;

    [Fact]
    public void ShouldSolveSample()
    {
        var result = Solve(Sample, [Operator.Plus, Operator.Mul]);

        Assert.Equal(3749, result);
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var input = File.ReadAllText("TestAssets/day7.txt");
        Assert.Equal(3312271365652, Solve(input, [Operator.Plus, Operator.Mul]));
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var input = File.ReadAllText("TestAssets/day7.txt");
        Assert.Equal(509463489296712, Solve(input, [Operator.Plus, Operator.Mul, Operator.Concat]));
    }

    private static long Solve(string input, Operator[] availableOps)
    {
        return ParseInput(input).Aggregate(0L, (acc, pair) =>
        {
            if (SolutionDay7.IsSolvable(pair.Item1, pair.Item2, availableOps))
            {
                return acc + pair.Item1;
            }
            return acc;
        });
    }

    public static (long, long[])[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine).Select(line =>
        {
            var parts = line.Split(": ");
            var target = long.Parse(parts[0]);
            var seq = parts[1].Split(" ").Select(long.Parse).ToArray();
            return (target, seq);
        }).ToArray();
    }
}

class SolutionDay7
{
    private static long FuncOperator(Operator op, long a, long b)
    {
        return op switch
        {
            Operator.Plus => a + b,
            Operator.Mul => a * b,
            Operator.Concat => long.Parse($"{a}{b}"),
            _ => throw new ArgumentException()
        };
    }

    public static bool IsSolvable(long target, long[] seq, Operator[] availableOps)
    {
        var seg = new ArraySegment<long>(seq);
        return IsSolvable(seg.Slice(1), target, seq[0], availableOps);
    }

    private static bool IsSolvable(ArraySegment<long> rest, long target, long currentAggregate, Operator[] availableOps)
    {
        var diff = target - currentAggregate;
        if (diff < 0)
        {
            return false;
        }

        if (diff == 0 && rest.Count == 0)
        {
            return true;
        }

        if (rest.Count == 0)
        {
            return false;
        }

        var nextNum = rest[0];
        var nextRest = rest.Slice(1);

        foreach (var op in availableOps)
        {
            var nextAggregate = FuncOperator(op, currentAggregate, nextNum);
            bool result = IsSolvable(nextRest, target, nextAggregate, availableOps);
            if (result)
            {
                return result;
            }
        }

        return false;
    }

}

enum Operator
{
    Plus,
    Mul,
    Concat
}