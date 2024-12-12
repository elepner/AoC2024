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

    [Fact]
    public void ShouldFindAllAreasAndPerimeters()
    {
        var garden = ParseInput(SmallSample);
        var allRegions = FindAllRegions(garden).ToArray();

        var expected = new Dictionary<char, (int area, int perimeter)>()
        {
            {'A', (4, 10)},
            {'B', (4, 8)},
            {'C', (4, 10)},
            {'D', (1, 4)},
            {'E', (3, 8)},
        };

        foreach (var region in allRegions)
        {
            var (area, perimeter) = expected[region.Value.Id];

            toh.WriteLine(region.Value.Id.ToString());
            toh.WriteLine("---area----");
            toh.WriteLine(string.Join(", ", region.visited));
            toh.WriteLine("---perimeter----");
            toh.WriteLine(string.Join(", ", region.perimeter));
            Assert.Equal(area, region.visited.Count);
            Assert.Equal(perimeter, region.perimeter);
        }

    }

    public int SolvePt1(string input)
    {
        return FindAllRegions(ParseInput(input)).Aggregate(0, (acc, current) =>
        {
            return acc + current.perimeter * current.visited.Count;
        });
    }

    private IEnumerable<(GardenCell Value, HashSet<(int, int)> visited, int perimeter)> FindAllRegions(Garden garden)
    {
        

        HashSet<(int, int)> allVisited = new HashSet<(int, int)>();

        foreach (var seed in garden.EnumerateCoords())
        {

            if (allVisited.Contains(seed))
            {
                continue;
            }


            var (visited, perimeter) = Bfs(garden, seed);

            foreach (var item in visited)
            {
                allVisited.Add(item);
            }

            yield return (seed.GetFieldValue(garden).Value, visited, perimeter);
        }


    }

    static Garden ParseInput(string input)
    {
        return input.Split(Environment.NewLine).Select(s => s.Select(c => new GardenCell(c)).ToArray()).ToArray();
    }

    static (HashSet<(int, int)> visited, int perimeter) Bfs(Garden garden, (int, int) seed)
    {
        var front = new HashSet<(int, int)>()
        {
            seed
        };
        var visited = new HashSet<(int, int)>();
        int p = 0;
        while (front.Count > 0)
        {
            var currentFront = front.ToArray();
            foreach (var el in currentFront)
            {
                front.Remove(el);
                visited.Add(el);

                var neighbours = garden.GetNeighbours(el).Where(x => !visited.Contains(x.XY)).ToArray();

                p += neighbours.Count(x => x.IsPerimeter);
                //foreach (var xy in neighbours.Where(x => x.IsPerimeter))
                //{
                //    perimeter.Add(xy.XY);
                //}

                foreach (var xy in neighbours.Where(x => !x.IsPerimeter))
                {
                    front.Add(xy.XY);
                }

                //var canGoTo = garden.GetCanGoTo(el).Where(x => !front.Contains(x)
                //                                              && !visited.Contains(x));

                //foreach (var canGo in canGoTo)
                //{
                //    front.Add(canGo);
                //}

            }
        }

        return (visited, p);
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
