using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AoC2024;
using DiskEntry = OneOf<Hole, DFile>;

public class Day9(ITestOutputHelper toh)
{
    private readonly string sample = "2333133121414131402";

    [Fact]
    public void ShouldCorrectlyProduceOutputString()
    {
        var input = ParseInput(sample);
        SolutionDay9.BruteForce(input);
        var resultStr = string.Join("", input);
        Assert.Equal("0099811188827773336446555566", resultStr);
    }

    [Fact]
    public void ShouldSolveSamplePt1()
    {
        Assert.Equal(1928, SolutionDay9.SolvePt1(ParseInput(sample)));
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var input = File.ReadAllText("TestAssets/day9.txt");
        var result = SolutionDay9.SolvePt1(ParseInput(input));

        Assert.Equal(6367087064415, result);
    }

    public List<int?> ParseInput(string input)
    {
        List<int?> result = new List<int?>();
        for (int i = 0; i < input.Length; i++)
        {
            var count = int.Parse(input[i].ToString());
            
            if (i % 2 == 0)
            {
                var id = i / 2;
                result.AddRange(Enumerable.Range(0, count).Select(x => (int?)id));
            }
            else
            {
                result.AddRange(Enumerable.Range(0, count).Select(x => (int?)null));
            }
        }

        return result;
    }

    [Fact]
    public void ShouldCorrectlyCompressSample()
    {
        var input = ParseInput2(sample);
        toh.WriteLine(input.AsString());
        SolutionDay9.CompressPt2(input, (x) => toh.WriteLine(x));

        Assert.Equal("00992111777.44.333....5555.6666.....8888..", input.AsString());
    }

    [Fact]
    public void ShouldSolveSamplePt2()
    {
        var input = ParseInput2(sample);
        var result = SolutionDay9.SolvePt2(input);
        Assert.Equal(2858, result);
    }

    [Fact]
    public void SolvePt2()
    {
        var input = ParseInput2(File.ReadAllText("TestAssets/day9.txt"));
        var result = SolutionDay9.SolvePt2(input);

        Assert.Equal(6390781891880, result);
    }

    public List<DiskEntry> ParseInput2(string input)
    {
        List<DiskEntry> result = new List<DiskEntry>();
        for (int i = 0; i < input.Length; i++)
        {
            var count = int.Parse(input[i].ToString());

            if (i % 2 == 0)
            {
                var id = i / 2;
                result.Add(new DFile(id, count));
            }
            else
            {
                if (count > 0)
                {
                    result.Add(new Hole(count));
                }
                
            }
        }
        return result;
    }
    
}

public static class SolutionDay9
{

    public static long SolvePt1(List<int?> input)
    {
        BruteForce(input);

        return input.TakeWhile(x => x.HasValue).Select((val, idx) => (val!.Value, idx)).Aggregate(0L, (acc, current) =>
        {
            return acc + current.idx * current.Value;
        });
    }

    public static void BruteForce(List<int?> input)
    {

        int holeLocation = 0;
        int lastFileLocation = input.Count - 1;

        while (true)
        {
            holeLocation = input.FindIndex(holeLocation, x => x == null);
            lastFileLocation = input.FindLastIndex(lastFileLocation, x => x != null);

            if (holeLocation >= lastFileLocation)
            {
                break;
            }

            input[holeLocation] = input[lastFileLocation];
            input[lastFileLocation] = null;
        }
    }

    public static string AsString(this List<DiskEntry> input)
    {
        var str = input.Select((entry) =>
        {
            return entry.Match(hole =>
            {
                return string.Join("", Enumerable.Range(0, hole.Size).Select(x => "."));
            }, file =>
            {
                return string.Join("", Enumerable.Range(0, file.Size).Select(x => file.Id));
            });
        });

        return string.Join("", str);
    }

    public static long SolvePt2(List<DiskEntry> input)
    {
        CompressPt2(input);
        return input.Select(diskEntry => diskEntry
                .Match(h => (Id: 0, h.Size), f => (Id: f.Id, f.Size)))
            .SelectMany((entry) => Enumerable.Range(0, entry.Size).Select(x => entry.Id))
            .Select((id, idx) => (id, idx))
            .Aggregate(0L, (acc, item) => acc + item.id * item.idx);
    }

    public static void CompressPt2(List<DiskEntry> input, Action<string>? log = null)
    {

        while (true)
        {
            var tryingToMoveIdx = input.FindLastIndex(x => x.IsT1 && !x.AsT1.Moved);
            if (tryingToMoveIdx < 0)
            {
                break;
            }
            var file = input[tryingToMoveIdx].AsT1;
            var holeIdx = input.FindIndex(x => x.IsT0 && x.AsT0.Size >= file.Size);
            file = file with { Moved = true };
            input[tryingToMoveIdx] = file;
            if (holeIdx < 0)
            {
                continue;
            }

            if (tryingToMoveIdx < holeIdx)
            {
                continue;
            }

            

            var hole = input[holeIdx].AsT0;
            var remainingSize = hole.Size - file.Size;

            //input[tryingToMoveIdx] = new Hole(file.Size);
            //MergeHoles(input, tryingToMoveIdx);

            //input[holeIdx] = file;

            

            input[tryingToMoveIdx] = new Hole(file.Size);
            input[holeIdx] = file;

            if (remainingSize > 0)
            {
                input.Insert(holeIdx + 1, new Hole(remainingSize));
            }

            
            MergeHoles(input, remainingSize > 0 ? tryingToMoveIdx + 1 : tryingToMoveIdx);
            if (remainingSize > 0)
            {
                MergeHoles(input, holeIdx + 1);
            }
            


            if (log != null)
            {
                log(input.AsString());
            }

        }
    }

    private static void MergeHoles(List<DiskEntry> input, int aroundIdx)
    {
        void MergeHolesInternal(int delta)
        {
            Assert.True(input[aroundIdx].IsT0);

            var idx = aroundIdx + delta;
            var toLeave = Math.Min(idx, aroundIdx);
            var toRemove = Math.Max(idx, aroundIdx);
            if (idx >= 0 && idx < input.Count)
            {
                var item = input[idx];
                if (item.IsT0)
                {
                    var newHoleSize = input[toLeave].AsT0.Size + input[toRemove].AsT0.Size;
                    input[toLeave] = new Hole(newHoleSize);
                    input.RemoveAt(toRemove);
                }
            }
        }

        MergeHolesInternal(1);
        MergeHolesInternal(-1);
    }
}

public record Hole(int Size);
public record DFile(int Id, int Size, bool Moved = false);