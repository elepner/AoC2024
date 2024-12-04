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
    public void SolveSamplePt1()
    {
        var result = ParseInput(Sample).SolvePt1();
        Assert.Equal(18, result);

    }

    [Fact]
    public void SolveSamplePt2()
    {
        var result = ParseInput(Sample).SolvePt2();
        Assert.Equal(9, result);
    }

    [Fact]
    public void SolvePt1()
    {
        var input = File.ReadAllText("TestAssets/day4.txt");
        var result = ParseInput(input).SolvePt1();
        Assert.Equal(2462, result);
    }


    [Fact]
    public void SolvePt2()
    {
        var input = File.ReadAllText("TestAssets/day4.txt");
        var result = ParseInput(input).SolvePt2();
        Assert.Equal(1877, result);
    }

    private string[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine).Select(x => x.Trim()).ToArray();
    }

}

static class SolutionDay4
{

    private static readonly List<string> Patterns = new List<string>() { "MMSS", "SSMM", "MSMS", "SMSM" };
    private static readonly List<(int, int)> Offsets = new List<(int, int)>() { (0, 0), (0, 1), (1, 0), (1, 1) }.Select(x => VScale(x, 2)).ToList();
    public static int SolvePt1(this string[] input)
    {

        var allStrings = AllDirections(input);
        var count = 0;
        foreach (var str in allStrings)
        {
            count += CountXmas(str);
            count += CountXmas(ReverseString(str));
        }
        return count;
    }

    public static int SolvePt2(this string[] input)
    {

        return Window(input).Count((corner) =>
        {

            var aLocation = VAdd(corner, (1, 1));
            var center = input.GetValue(aLocation);
            if (center != 'A')
            {
                return false;
            }

            var pattern = new string(Offsets.Select(offset => input.GetValue(VAdd(offset, corner))).ToArray());
            if (Patterns.Contains(pattern))
            {
                return true;
            }
            return false;
        });

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

    private static IEnumerable<(int, int)> Window(string[] field)
    {
        var (rows, cols) = GetDims(field);
        for (var i = 0; i < rows - 2; i++)
        {
            for (var j = 0; j < cols - 2; j++)
            {
                yield return (i, j);
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

    private static (int, int) VAdd((int, int) a, (int, int) b)
    {
        return (a.Item1 + b.Item1, a.Item2 + b.Item2);
    }

    private static (int, int) VScale((int, int) a, int scale)
    {
        return (a.Item1 * scale, a.Item2 * scale);
    }

    private static char GetValue(this string[] field, (int, int) coords)
    {
        return field[coords.Item1][coords.Item2];
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

}