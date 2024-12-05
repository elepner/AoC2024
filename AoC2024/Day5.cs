using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace AoC2024;

public class Day5(ITestOutputHelper toh)
{
    private static readonly string Sample = """
                                            47|53
                                            97|13
                                            97|61
                                            97|47
                                            75|29
                                            61|13
                                            75|53
                                            29|13
                                            97|29
                                            53|29
                                            61|53
                                            97|53
                                            61|29
                                            47|13
                                            75|47
                                            97|75
                                            47|61
                                            75|61
                                            47|29
                                            75|13
                                            53|13

                                            75,47,61,53,29
                                            97,61,53,29,13
                                            75,29,13
                                            75,97,47,61,53
                                            61,13,29
                                            97,13,75,29,47
                                            """;

    static readonly (IEnumerable<(int, int)> orderingRules, int[][] pages) SampleData = ParseInput(Sample);


    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    public void ShouldSolveSamples(int updateNumber, bool expected)
    {
        var index = SolutionDay5.BuildIndex(SampleData.orderingRules);
        var actual = SolutionDay5.IsOk(SampleData.pages[updateNumber], index);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ShouldFixSequence()
    {
        var index = SolutionDay5.BuildIndex(SampleData.orderingRules);
        var seq = new int[] { 75, 97, 47, 61, 53 };
        var fixedSeq = SolutionDay5.Fix(seq, index);
        var isOk = SolutionDay5.IsOk(fixedSeq, index);
        Assert.True(isOk);
    }



    [Fact]
    public void ShouldSovleSample()
    {
        var index = SolutionDay5.BuildIndex(SampleData.orderingRules);
        Assert.Equal(143, SolutionDay5.SolvePt1(SampleData.pages, index, s => toh.WriteLine(s)));
    }

    [Fact]
    public void ShouldSovleSamplePt2()
    {
        var index = SolutionDay5.BuildIndex(SampleData.orderingRules);
        Assert.Equal(123, SolutionDay5.SolvePt2(SampleData.pages, index, s => toh.WriteLine(s)));
    }

    

    [Fact]
    public void ShouldSolve()
    {
        var input = File.ReadAllText("TestAssets/day5.txt");
        var (pairs, pages) = ParseInput(input);

        var index = SolutionDay5.BuildIndex(pairs);


        var allConnected = index.Values.All(x => x.In.Count + x.Out.Count == index.Count - 1);

        Assert.True(allConnected);

        var result = SolutionDay5.SolvePt1(pages, index);

        Assert.Equal(4790, result);
    }

    [Fact]
    public void ShouldSovlePt2()
    {
        var input = File.ReadAllText("TestAssets/day5.txt");
        var (pairs, pages) = ParseInput(input);

        var index = SolutionDay5.BuildIndex(pairs);

        var result = SolutionDay5.SolvePt2(pages, index);
        toh.WriteLine($"Result is {result}");
        Assert.Equal(6319, result);
    }

    [Fact]
    public void ShouldFixAll()
    {
        var input = File.ReadAllText("TestAssets/day5.txt");
        var (pairs, pages) = ParseInput(input);
        var index = SolutionDay5.BuildIndex(pairs);
        var broken = pages.Where(page => !SolutionDay5.IsOk(page, index));

        foreach (var el in broken)
        {
            Debug.WriteLine($"Fixing sequence {string.Join(",", el)}");
            var fix = SolutionDay5.Fix(el, index);
            Debug.WriteLine($"Fixed sequence {string.Join(",", fix)}");
            Assert.True(SolutionDay5.IsOk(fix, index));
        }
    }

    private static (IEnumerable<(int, int)>, int[][]) ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).Select((x => x.Trim())).ToArray();

        var pairs = lines.TakeWhile(x => !string.IsNullOrEmpty(x)).ToArray();



        return (pairs.Select(pair =>
            {
                var parts = pair.Split('|');
                return (int.Parse(parts[0]), int.Parse(parts[1]));
            }),
            lines.Skip(pairs.Length + 1)
                .Select(str => str.Split(",").Select(int.Parse)
                    .ToArray())
                .ToArray());
    }
}

static class SolutionDay5
{
    public static int SolvePt1(int[][] seqs, Dictionary<int, Node<int>> index, Action<string>? log = null)
    {
        return seqs.Where(seq => IsOk(seq, index, log)).Select(x => x[x.Length / 2]).Aggregate(0, (acc, curr) => acc + curr);
    }

    public static int SolvePt2(int[][] seqs, Dictionary<int, Node<int>> index, Action<string>? log = null)
    {
        return seqs.Where(seq => !IsOk(seq, index, log)).AsParallel().Select(seq => Fix(seq, index)).Select(x => x[x.Length / 2]).Aggregate(0, (acc, curr) => acc + curr);
    }

    public static bool IsOk(int[] seq, Dictionary<int, Node<int>> index, Action<string>? log = null)
    {
        for (int i = 0; i < seq.Length; i++)
        {
            var el = seq[i];
            if (!index.TryGetValue(el, out var node))
            {
                log?.Invoke($"Not in index {el}");
                continue;
            }

            for (int j = i + 1; j < seq.Length; j++)
            {
                var next = seq[j];
                if (node.IsIncoming(next))
                {
                    return false;
                }
            }

        }
        return true;
    }

    public static int[] Fix(int[] seq, Dictionary<int, Node<int>> index)
    {
        return Fix(seq, true, new[] { seq[0] }, index);
    }

    public static int[]? Fix(int[] seq, bool isForward, int[] currentPath, Dictionary<int, Node<int>> index)
    {
        var current = isForward ? currentPath[^1] : currentPath[0];

        var node = index[current];

        var remaining = seq.Where(x => !currentPath.Contains(x)).ToArray();

        if (remaining.Length == 0)
        {
            if (IsOk(currentPath, index))
            {
                return currentPath;
            }

            throw new ArgumentException($"Found path {string.Join(",", currentPath)}, but it's not ok");
        }

        var forward = (GetOptions(node.Out), true);
        var backward = (GetOptions(node.In), false);

        var options = isForward ? forward : backward;

        Node<int>[] GetOptions(IReadOnlyDictionary<int, Node<int>> nodes)
        {
            return nodes.Where(x => remaining.Contains(x.Key)).Select(x => x.Value).ToArray();
        }

        if (isForward && options.Item1.Length == 0)
        {
            return Fix(seq, false, currentPath, index);
        }


        foreach (var option in options.Item1)
        {
            var newPath = isForward
                ? currentPath.Append(option.Id).ToArray()
                : currentPath.Prepend(option.Id).ToArray();
            var result = Fix(seq, isForward, newPath, index);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }



    public static Dictionary<int, Node<int>> BuildIndex(IEnumerable<(int, int)> pairs)
    {
        var result = new Dictionary<int, Node<int>>();

        foreach (var (l, r) in pairs)
        {
            Node<int> node;
            var lNode = GetOrCreateNode(l);
            var rNode = GetOrCreateNode(r);

            lNode.ConnectOut(rNode);
        }

        return result;
        Node<int> GetOrCreateNode(int val)
        {
            Node<int> node;
            if (!result.TryGetValue(val, out node))
            {
                node = new Node<int>(val);
                result.Add(val, node);
            }

            return node;
        }
    }
}




class Node<TId> where TId : notnull
{
    private Dictionary<TId, Node<TId>> _out = new();
    private Dictionary<TId, Node<TId>> _in = new();

    public Node(TId id)
    {
        Id = id;
    }
    public TId Id { get; }

    public void ConnectOut(Node<TId> other)
    {
        _out[other.Id] = other;
        other._in[Id] = this;
    }

    public bool IsIncoming(TId otherId)
    {
        return _in.ContainsKey(otherId);
    }

    public IReadOnlyDictionary<TId, Node<TId>> Out => _out.AsReadOnly();
    public IReadOnlyDictionary<TId, Node<TId>> In => _in.AsReadOnly();


    public override string ToString()
    {
        return $"({string.Join(", ", _in.Select(x => x.Key))}) -> {Id} -> ({string.Join(", ", _out.Select(x => x.Key))})";
    }
}
