using Xunit;
namespace AoC2024;

public class Day4
{
    private readonly string Sample = """
                                        MMMSXXMASM
                                        MSAMXMSMSA
                                        AMXSXMAAMM
                                        MSAMASMSMX
                                        XMASAMXAMM
                                        XXAMMXXAMA
                                        SMSMSASXSS
                                        SAXAMASAAA
                                        MAMMMXMMMM
                                        MXMXAXMASX
                                     """;

    [Fact]
    public void SolveSample()
    {
        var result = ParseInput(Sample).SolvePt1();
        Assert.Equal(18, result);

    }


    [Fact]
    public void Foo()
    {
        var field = ParseInput(Sample);

        var result = SolutionDay4.GetDirections((0, 0), field).Where(x => x != (1, 1)).ToArray();
    }

    private string[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
    }

}

static class SolutionDay4
{
    public static int SolvePt1(this string[] input)
    {
        var (maxV, maxH) = GetDims(input);
        var count = 0;
        for (var i = 0; i < maxV; i++)
        {
            for (var j = 0; j < maxH; j++)
            {
                if (input[i][j] == 'X')
                {
                    var result = 0;
                    input.FindXmas([(i, j)], "MAS".AsSpan(), ref result);
                    count += result;
                }
            }
        }
        // return EnumerateString(input).Aggregate(0, (acc, str) => acc + CountXmas(str.AsSpan()));
        return count;
    }

    private static bool FindChar(this string[] field, (int, int) current, char c, out (int, int) next)
    {

        var dirs = GetDirections(current, field).ToArray();
        var result = GetDirections(current, field)
        .Select(direction => new { c = field[direction.Item1][direction.Item2], coords = direction })
        .FirstOrDefault(obj => obj.c == c);

        next = result?.coords ?? (0, 0);

        return result != null;
    }

    public static IEnumerable<(int, int)> GetDirections((int, int) current, string[] field)
    {
        var (mavH, maxV) = field.GetDims();
        int[] steps = [-1, 0, 1];
        foreach (var dv in steps)
        {
            foreach (var dh in steps)
            {
                if (dv == 0 && dh == 0)
                {
                    continue;
                }
                var nextV = current.Item1 + dv;
                var nextH = current.Item2 + dh;
                if (CheckBounds(nextV, maxV) && CheckBounds(nextH, mavH))
                {
                    yield return (nextV, nextH);
                }
            }
        }
    }

    private static bool CheckBounds(int val, int max)
    {
        return 0 <= val && val < max;
    }

    private static (int, int) GetDims(this string[] field)
    {
        return (field.Length, field[0].Length);
    }


    private static void FindXmas(this string[] field, (int, int)[] path, ReadOnlySpan<char> pattern, ref int result)
    {
        if (pattern.Length == 0)
        {
            result++;
            return;
        }
        var current = path.Last();
        var toSearch = pattern[0];

        var nextSteps = GetDirections(current, field).Where(direction => !path.Contains(direction)).ToArray();

        foreach (var nextStep in nextSteps)
        {
            var nextChar = field[nextStep.Item1][nextStep.Item2];
            if (nextChar == toSearch)
            {
                field.FindXmas([.. path, nextStep], pattern[1..], ref result);
            }
        }

    }
}