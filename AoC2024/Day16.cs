using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AoC2024;

using Vertex = ((int, int) Location, Direction Direction);

public class Day16
{
    private const string Sample = """
                                  ###############
                                  #.......#....E#
                                  #.#.###.#.###.#
                                  #.....#.#...#.#
                                  #.###.#####.#.#
                                  #.#.#.......#.#
                                  #.#.#####.###.#
                                  #...........#.#
                                  ###.#.#####.#.#
                                  #...#.....#.#.#
                                  #.#.#.###.#.#.#
                                  #.....#...#.#.#
                                  #.###.#.#.#.#.#
                                  #S..#.....#...#
                                  ###############
                                  """;

    [Fact]
    public void ShouldSolveSample()
    {
        Assert.Equal(7036, Solve(Sample));
    }

    [Fact]
    public void ShouldSolveSamplePt2()
    {
        Assert.Equal(45, SolvePt2(Sample));
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var result = Solve(File.ReadAllText("TestAssets/day16.txt"));
        Assert.Equal(99488, result);
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var result = SolvePt2(File.ReadAllText("TestAssets/day16.txt"));
        Assert.Equal(516, result);
    }

    public int Solve(string input)
    {
        var maze = ParseInput(input);
        var visited = Solve(maze);

        

        var result = visited.Where(x => x.Key.Location == maze.Finish).MinBy(x => x.Value.Cost).Value;
        return result.Cost;
    }

    public int SolvePt2(string input)
    {
        var maze = ParseInput(input);
        var visited = Solve(maze);

        var min = visited.Where(x => x.Key.Location == maze.Finish).MinBy(x => x.Value.Cost);
        var result = CountSteps(visited, min.Key);

        return result.Count;
    }

    Dictionary<Vertex, (int Cost, Vertex[] From)> Solve(Maze maze)
    {

        var front = new Dictionary<Vertex, (int Cost, Vertex[] From)>();
        front[(maze.Start, Direction.E)] = (0, []);
        var visited = new Dictionary<Vertex, (int Cost, Vertex[] From)>();
        while (front.Count > 0)
        {
            //front.TryGetValue(front.Min, out var value);
            // var (vertex, cost) = front.Min;
            // front.Remove(front.Min);
            var current = front.MinBy(x => x.Value.Cost);
            var vertex = current.Key;
            var costInfo = current.Value;
            front.Remove(vertex);
            visited.Add(vertex, costInfo);
            foreach (var neighbor in GetNeighbours(maze, vertex))
            {
                var currentCost = costInfo.Cost + neighbor.Cost;

                if (visited.ContainsKey(neighbor.Vertex))
                {
                    continue;
                }

                if (front.TryGetValue(neighbor.Vertex, out var existingCostInfo))
                {
                    if (existingCostInfo.Cost > currentCost)
                    {
                        front[neighbor.Vertex] = (currentCost, [vertex]);
                    } else if (existingCostInfo.Cost == currentCost)
                    {
                        front[neighbor.Vertex] = (currentCost, [.. existingCostInfo.From, vertex]);
                    }
                }
                else
                {
                    front[neighbor.Vertex] = (currentCost, [vertex]);
                }
            }



        }

        return visited;
    }

    IEnumerable<(Vertex Vertex, int Cost)> GetNeighbours(Maze maze, Vertex vertex)
    {
        var currentDirection = vertex.Direction;

        var directionsWithCost = new int[] { 1, 3 }.Select(
                (rotateCount) =>
                    (Enumerable.Range(0, rotateCount).Aggregate(currentDirection, (acc, i) => acc.Rotate90(true)), 1000)
            ).Append((currentDirection, 0))
            .Select(tuple =>
            {
                var (direction, cost) = tuple;
                return ((direction.GetVector().Add(vertex.Location), direction), cost + 1);
            })
            .Where(x => maze.Cells.GetVal(x.Item1.Item1) is not Wall);



        return directionsWithCost;
    }

    private HashSet<(int, int)> CountSteps(Dictionary<Vertex, (int Cost, Vertex[] From)> pathMap, Vertex current, HashSet<(int, int)> visited = null!)
    {
        visited ??= new HashSet<(int, int)>();
        visited.Add(current.Location);
        foreach (var tuple in pathMap[current].From)
        {
            CountSteps(pathMap, tuple, visited);
        }
        return visited;
    }

    private Maze ParseInput(string input)
    {
        (int, int)? start = null;
        (int, int)? end = null;

        IMazeCell[][] cells = input.Trim().Split(Environment.NewLine).Select((line, row) =>
        {
            return line.Select(IMazeCell (c, col) =>
            {
                if (c == '#')
                {
                    return new Wall();
                }

                if (c == 'E')
                {
                    end = (row, col);
                }

                if (c == 'S')
                {
                    start = (row, col);
                }
                return new Empty();
            }).ToArray();
        }).ToArray();
        return new Maze(cells, start!.Value, end!.Value);
    }
}

record Maze(IMazeCell[][] Cells, (int, int) Start, (int, int) Finish);

interface IMazeCell;
record Empty : IMazeCell;
record Wall : IMazeCell;

class FrontComparer : IComparer<(Vertex Vertext, int Cost)>
{
    public int Compare((Vertex Vertext, int Cost) x, (Vertex Vertext, int Cost) y)
    {
        return x.Cost - y.Cost;
    }
}