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
    public void ShouldSovleSample()
    {
        var index = SolutionDay5.BuildIndex(SampleData.orderingRules);
        Assert.Equal(143, SolutionDay5.SolvePt1(SampleData.pages, index, s => toh.WriteLine(s)));
    }

    [Fact]
    public void ShouldSolve()
    {
        var input = File.ReadAllText("TestAssets/day5.txt");
        var (pairs, pages) = ParseInput(input);

        var index = SolutionDay5.BuildIndex(pairs);
        var result = SolutionDay5.SolvePt1(pages, index);

        Assert.Equal(4790, result);
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
}
