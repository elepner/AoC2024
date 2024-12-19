using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AoC2024;

public class Day19
{
    private const string Sample = """
                                  r, wr, b, g, bwu, rb, gb, br
                                  
                                  brwrr
                                  bggr
                                  gbbr
                                  rrbgbr
                                  ubwu
                                  bwurrg
                                  brgr
                                  bbrgwb
                                  """;

    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    [InlineData(7, false)]
    [Theory]
    public void ShouldCorrectlyCheckSample(int number, bool canMake)
    {
        var (inputs, toConstruct) = ParseInput(Sample);
        Assert.Equal(canMake, CanMakePattern(inputs, toConstruct[number]));
    }

    [Fact]
    public void ShouldCorrectlySolveSamplePt1()
    {
        Assert.Equal(6, SolvePt1(Sample));
    }

    [Fact]
    public void ShouldCorrectlySolvePt1()
    {
        Assert.Equal(371,SolvePt1(Utils.ReadTaskInput(19)));
    }


    [Fact]
    public void ShouldCorrectlySolveSamplePt2()
    {
        Assert.Equal(16, SolvePt2(Sample));
    }

    [Fact]
    public void ShouldCorrectlySolvePt2()
    {
        Assert.Equal(650354687260341, SolvePt2(Utils.ReadTaskInput(19)));
    }
    private int SolvePt1(string input)
    {
        var (inputs, toConstruct) = ParseInput(input);
        return toConstruct.Count(x => CanMakePattern(inputs, x));
    }

    private long SolvePt2(string input)
    {
        var (inputs, toConstruct) = ParseInput(input);
        var cache = new Dictionary<string, long>();
        return toConstruct.Select(x => CountPatterns(inputs, x, cache)).Aggregate(0l, (acc, curr) => acc +curr);
    }
    private bool CanMakePattern(string[] availableInputs, string targetPattern)
    {
        if (targetPattern.Length == 0)
        {
            return true;
        }
        var canUse = availableInputs.Where(x => targetPattern.StartsWith(x));

        foreach (var towel in canUse)
        {
            var rest = targetPattern.Substring(towel.Length);
            if (CanMakePattern(availableInputs, rest))
            {
                return true;
            }
        }

        return false;
    }


    private long CountPatterns(string[] availableInputs, string targetPattern, Dictionary<string, long> cache)
    {
        if (cache.ContainsKey(targetPattern))
        {
            return cache[targetPattern];
        }

        if (targetPattern.Length == 0)
        {
            return 1;
        }
        var canUse = availableInputs.Where(x => targetPattern.StartsWith(x));
        long count = 0;
        foreach (var towel in canUse)
        {
            var rest = targetPattern.Substring(towel.Length);
            count += CountPatterns(availableInputs, rest, cache);
        }
        cache[targetPattern] = count;
        return count;
    }

    private (string[], string[]) ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();

        var towels = lines[0].Split(",").Select(x => x.Trim()).ToArray();

        var targetPatterns = lines.Skip(2).ToArray();

        return (towels, targetPatterns);
    }
}