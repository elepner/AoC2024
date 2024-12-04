using System.Diagnostics.Tracing;
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
        var field = ParseInput("""
        AAA
        BBB
        CCC
        DDD
        """);

        var result = SolutionDay4.EnumerateDiagonal(field).ToArray();
        var flipped = SolutionDay4.FlipDiagonals(result, SolutionDay4.GetDims(field)).ToArray();
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

        var allStrings = AllDirections(input);
        var count = 0;
        foreach (var str in allStrings)
        {
            count += CountXmas(str);
            count += CountXmas(ReverseString(str));
        }
        // var (maxV, maxH) = GetDims(input);
        // var count = 0;
        // for (var i = 0; i < maxV; i++)
        // {
        //     for (var j = 0; j < maxH; j++)
        //     {
        //         if (input[i][j] == 'X')
        //         {
        //             var result = 0;
        //             input.FindXmas([(i, j)], "MAS".AsSpan(), ref result);
        //             count += result;
        //         }
        //     }
        // }
        // return EnumerateString(input).Aggregate(0, (acc, str) => acc + CountXmas(str.AsSpan()));
        return count;
    }

    public static IEnumerable<string> AllDirections(string[] input)
    {
        var (rows, cols) = GetDims(input);
        foreach (var str in input)
        {
            yield return str;
        }

        for (var i = 0; i < cols; i++)
        {
            yield return new string(input.Select(x => x[i]).ToArray());
        }

        var diags = EnumerateDiagonal(input).ToArray();
        var flipped = FlipDiagonals(diags, (rows, cols));

        var diagLines = diags.Concat(flipped).Select((diag) => diag.Select(coords => input[coords.Item1][coords.Item2]).ToArray()).Select(x => new string(x));

        foreach (var diag in diagLines)
        {
            yield return diag;
        }
    }

    public static IEnumerable<List<(int, int)>> FlipDiagonals(IEnumerable<List<(int, int)>> input, (int, int) dims)
    {
        var (rows, cols) = dims;
        foreach (var diag in input)
        {
            var result = new List<(int, int)>(diag.Count);
            foreach (var (row, col) in diag)
            {
                result.Add((row, cols - col - 1));
            }

            yield return result;
        }
    }

    public static IEnumerable<List<(int, int)>> EnumerateDiagonal(this string[] field)
    {
        var (rows, cols) = GetDims(field);

        for (int startRow = 0; startRow < rows; startRow++)
        {
            List<(int, int)> diagonal = new();
            for (int r = startRow, c = 0; r < rows && c < cols; r++, c++)
            {
                diagonal.Add((r, c));
            }
            yield return diagonal;
        }

        // Iterate over diagonals starting from each column of the first row (except [0,0])
        for (int startCol = 1; startCol < cols; startCol++)
        {
            List<(int, int)> diagonal = new();
            for (int r = 0, c = startCol; r < rows && c < cols; r++, c++)
            {
                diagonal.Add((r, c));
            }
            yield return diagonal;
        }
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

    public static (int, int) GetDims(this string[] field)
    {
        return (field.Length, field[0].Length);
    }

    private static string ReverseString(string input)
    {
        char[] charArray = input.ToCharArray();
        Array.Reverse(charArray);
        string reversed = new string(charArray);

        return reversed;
    }

    private static int CountXmas(string input)
    {
        var xmas = "XMAS";
        var span = input.AsSpan();
        var count = 0;
        while (true)
        {
            var idx = span.IndexOf(xmas);
            if (idx < 0)
            {
                return count;
            }
            count++;
            span = span.Slice(idx + xmas.Length);
        }
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