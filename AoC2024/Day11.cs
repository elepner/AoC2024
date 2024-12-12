using Xunit;
using Xunit.Abstractions;

namespace AoC2024;

public class Day11(ITestOutputHelper toh)
{
    private static readonly string Input = "6 11 33023 4134 564 0 8922422 688775";
    private static readonly string Sample = "125 17";

    [InlineData(1, "253000 1 7")]
    [InlineData(2, "253 0 2024 14168")]
    [InlineData(3, "512072 1 20 24 28676032")]
    [InlineData(4, "512 72 2024 2 0 2 4 2867 6032")]
    [InlineData(5, "1036288 7 2 20 24 4048 1 4048 8096 28 67 60 32")]
    [InlineData(6, "2097446912 14168 4048 2 0 2 4 40 48 2024 40 48 80 96 2 8 6 7 6 0 3 2")]
    [Theory]
    public void ShouldSolveSample(int blink, string expected)
    {

        var list = ParseInput(Sample);
        long count = 0;
        var result = new List<long>();
        foreach (var number in list)
        {
            long currentCount = 0;
            Simulate(number, 0, blink, ref currentCount, result);
            count += currentCount;
        }

        var actual = string.Join(" ", result);
        
        Assert.Equal(actual, expected);
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        long count = 0;
        var list = ParseInput(Input);
        foreach (var number in list)
        {
            long currentCount = 0;
            Simulate(number, 0, 25, ref currentCount);
            count += currentCount;
        }

        Assert.Equal(220999, count);
    }

    [Fact]
    public void ShouldSolvePt1WithCache()
    {
        var result = Solve(Input, (num, cache) =>
        {
            return Simulate2(num, 0, 25, cache);
        });
        Assert.Equal(220999, result);
    }


    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [Theory]
    
    public void SouldSolvePt2(int addToCacheAtDepth)
    {
        var result = Solve(Input, (num, cache) =>
        {
            return Simulate2(num, 0, 75, cache, addToCacheAtDepth);
        });

        Assert.Equal(261936432123724, result);
    }

    private long Solve(string inputString, Func<long, Dictionary<(long, long), long>, long> simulate)
    {
        var cache = new Dictionary<(long, long), long>();
        long result = 0;
        foreach (var num in ParseInput(inputString))
        {
            result += simulate(num, cache);
        }
        toh.WriteLine($"Cache size {cache.Count}");

        return result;
    }

    private void Simulate(long current, int depth, int maxDepth, ref long count, List<long>? results = null)
    {
        

        if (depth == maxDepth)
        {
            Interlocked.Increment(ref count);
            results?.Add(current);
            
            return;
        }

        foreach (var l in Next(current))
        {
            Simulate(l, depth + 1, maxDepth, ref count, results);
        }
    }

    public long Simulate2(long current, int depth, int maxDepth, Dictionary<(long, long), long> cache, int addToCacheAtDepth = 0)
    {
        var remainingDepth = maxDepth - depth;
        if (remainingDepth > addToCacheAtDepth)
        {
            if (cache.TryGetValue((current, depth), out var cached))
            {
                return cached;
            }
        }
        

        if (depth == maxDepth)
        {
            return 1;
        }


        long result = 0;
        foreach (var l in Next(current))
        {
            result += Simulate2(l, depth + 1, maxDepth, cache, addToCacheAtDepth);
        }

        
        if (remainingDepth > addToCacheAtDepth)
        {
            cache.TryAdd((current, depth), result);
        }
        
        return result;
    }

    public IEnumerable<long> Next(long current)
    {

        Assert.True(current >= 0);
        if (current == 0)
        {
            yield return 1;
        }
        else
        {
            var str = current.ToString();


            if (str.Length % 2 == 0)
            {
                yield return long.Parse(str.Substring(0, str.Length / 2));
                yield return long.Parse(str.Substring(str.Length / 2));
            }
            else
            {
                yield return current * 2024;
            }
        }

        

    }

    public static List<int> ParseInput(string input)
    {
        return input.Trim().Split(" ").Select(x => int.Parse(x)).ToList();
    }

}