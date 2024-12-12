using Xunit;
using Xunit.Abstractions;

namespace AoC2024;

using Garden = GardenCell[][];

public class Day12(ITestOutputHelper toh)
{
    public static string SmallSample = """
                                       AAAA
                                       BBCD
                                       BBCC
                                       EEEC
                                       """;

    public static string LargeSample = """
                                       RRRRIICCFF
                                       RRRRIICCCF
                                       VVRRRCCFFF
                                       VVRCCCJFFF
                                       VVVVCJJCFE
                                       VVIVCCJJEE
                                       VVIIICJJEE
                                       MIIIIIJJEE
                                       MIIISIJEEE
                                       MMMISSJEEE
                                       """;

    [Fact]
    public void ShouldSolveSamples()
    {
        Assert.Equal(140, SolvePt1(SmallSample));
        Assert.Equal(1930, SolvePt1(LargeSample));
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var result = SolvePt1(File.ReadAllText("TestAssets/day12.txt"));
        Assert.Equal(1461806, result);
    }

    [InlineData('A')]
    [InlineData('B')]
    [InlineData('C')]
    [InlineData('D')]
    [InlineData('E')]
    [Theory]
    public void ShouldFindAllAreasAndPerimeters(char toCheck)
    {
        var garden = ParseInput(SmallSample);
        var allRegions = FindAllRegions(garden).ToArray();

        var expected = new Dictionary<char, (int area, int perimeter, int straightLines)>()
        {
            {'A', (4, 10, 4)},
            {'B', (4, 8, 4)},
            {'C', (4, 10, 8)},
            {'D', (1, 4, 4)},
            {'E', (3, 8, 4)},
        };

        foreach (var region in allRegions)
        {
            if (region.Value.Id != toCheck) continue;
            var (area, perimeter, straightLines) = expected[region.Value.Id];

            toh.WriteLine(region.Value.Id.ToString());
            toh.WriteLine("---area----");
            toh.WriteLine(string.Join(", ", region.visited));
            toh.WriteLine("---perimeter----");
            toh.WriteLine(string.Join(", ", region.perimeter));
            Assert.Equal(area, region.visited.Count);
            Assert.Equal(perimeter, region.perimeter);

            var sl = CountStraightLines(region.perimeterPlots, region.visited);


            Assert.Equal(straightLines, sl);
        }
    }



    public int SolvePt1(string input)
    {
        return FindAllRegions(ParseInput(input)).Aggregate(0, (acc, current) =>
        {
            return acc + current.perimeter * current.visited.Count;
        });
    }

    [Fact]
    public void ShouldSolvePt2LargeSample()
    {
        var result = SolvePt2(LargeSample);
        Assert.Equal(1206, result);
    }

    [Fact]
    public void ShouldSolvePt2SmallSample()
    {
        var result = SolvePt2(SmallSample);
        Assert.Equal(80, result);
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var result = SolvePt2(File.ReadAllText("TestAssets/day12.txt"));
        Assert.True(result > 867147);
    }

    [InlineData("""
                OOOOO
                OXOXO
                OOOOO
                OXOXO
                OOOOO
                """, 436)]
    [InlineData("""
                EEEEE
                EXXXX
                EEEEE
                EXXXX
                EEEEE
                """, 236)]
    [InlineData("""
                AAAAAA
                AAABBA
                AAABBA
                ABBAAA
                ABBAAA
                AAAAAA
                """, 368)]
    [Theory]
    public void OtherSamples(string input, int expectedCost)
    {
        var result = SolvePt2(input);
        Assert.Equal(expectedCost, result);
    }

    public int SolvePt2(string input)
    {
        var regions = FindAllRegions(ParseInput(input));
        var result = 0;
        foreach (var region in regions)
        {
            var straightLines = CountStraightLines(region.perimeterPlots, region.visited);
            result += straightLines * region.visited.Count;
        }

        return result;
    }


    private IEnumerable<(GardenCell Value, HashSet<(int, int)> visited, int perimeter, HashSet<(int, int)> perimeterPlots)> FindAllRegions(Garden garden)
    {


        HashSet<(int, int)> allVisited = new HashSet<(int, int)>();

        foreach (var seed in garden.EnumerateCoords())
        {

            if (allVisited.Contains(seed))
            {
                continue;
            }


            var (visited, perimeter, perimeterPlots) = Bfs(garden, seed);

            foreach (var item in visited)
            {
                allVisited.Add(item);
            }

            yield return (seed.GetFieldValue(garden).Value, visited, perimeter, perimeterPlots);
        }
    }

    private int CountStraightLines(HashSet<(int, int)> perimeter, HashSet<(int, int)> area)
    {
        var count = 0;
        var perimeterDegree = perimeter.ToDictionary(x => x, y => y.Around().Where((x) => area.Contains(x.Item2)).Count());

        void DecreaseDegree((int, int) key)
        {
            var value = perimeterDegree[key];
            value--;
            if (value == 0)
            {
                perimeterDegree.Remove(key);
            }
            else
            {
                perimeterDegree[key] = value;
            }
        }

        while (perimeterDegree.Count > 0)
        {
            var el = perimeterDegree.First();
            Assert.True(el.Value > 0);
            DecreaseDegree(el.Key);
            count++;

            var line = el.Key
            .Around()
            .Where(tuple => perimeter.Contains(tuple.Point))
            .SelectMany((tuple) =>
            {
                var (direction, v) = tuple;
                return Enumerable.Range(1, int.MaxValue)
                .Select((i) => direction.GetVector().Scale(i).Add(el.Key))
                .TakeWhile((v) => perimeterDegree.ContainsKey(v));
            }).ToList();

            line.ForEach((p) =>
            {
                DecreaseDegree(p);
            });
        }

        return count;
    }

    static Garden ParseInput(string input)
    {
        return input.Split(Environment.NewLine).Select(s => s.Select(c => new GardenCell(c)).ToArray()).ToArray();
    }

    static (HashSet<(int, int)> visited, int perimeter, HashSet<(int, int)> PerimeterPlots) Bfs(Garden garden, (int, int) seed)
    {
        var front = new HashSet<(int, int)>()
        {
            seed
        };
        var visited = new HashSet<(int, int)>();
        int p = 0;

        HashSet<(int, int)> perimeter = new HashSet<(int, int)>();

        while (front.Count > 0)
        {
            var currentFront = front.ToArray();
            foreach (var el in currentFront)
            {
                front.Remove(el);
                visited.Add(el);

                var neighbours = garden.GetNeighbours(el).Where(x => !visited.Contains(x.XY)).ToArray();

                p += neighbours.Count(x => x.IsPerimeter);
                perimeter.UnionWith(neighbours.Where(x => x.IsPerimeter).Select(x => x.XY));
                front.UnionWith(neighbours.Where(x => !x.IsPerimeter).Select(x => x.XY));

            }
        }

        return (visited, p, perimeter);
    }



}

static class GardenExtensions
{
    public static IEnumerable<((int, int) XY, bool IsPerimeter)> GetNeighbours(this Garden garden, (int, int) point)
    {
        var value = garden.GetVal(point);

        return CollectionExtension
            .AllDirections()
            .Select(x => x.GetVector())
            .Select(v => v.Add(point))
            .Select(x =>
            {
                if (!garden.CheckBounds(x))
                {
                    return (x, true);
                }
                return (x, value.Id != garden.GetVal(x).Id);
            });
    }
}

record struct GardenCell(char Id);

// record struct GardenTraversal(bool IsVisited);
