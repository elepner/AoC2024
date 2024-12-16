using System;
using System.Collections.Generic;
using System.Linq;
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
    public void Foo()
    {
        var maze = ParseInput(Sample);
        Solve(maze);
    }

    void Solve(Maze maze)
    {

        var front = new SortedSet<(Vertex Vertext, int Cost)>(new FrontComparer());
        front.Add(((maze.Start, Direction.E), 0));
        var visited = new HashSet<Vertex>();
        while (front.Count > 0)
        {
            //front.TryGetValue(front.Min, out var value);
            var (vertex, cost) = front.Min;
            front.Remove(front.Min);

            foreach (var VARIABLE in GetNeighbours(maze, vertex))
            {
                
            }

            

        }
    }

    IEnumerable<(Vertex Vertext, int Cost)> GetNeighbours(Maze maze, Vertex vertex)
    {
        var currentDirection = vertex.Direction;

        var directionsWithCost = Enumerable.Range(0, 3).Select(
                (rotateCount) =>
                    (Enumerable.Range(0, rotateCount).Aggregate(currentDirection, (acc, i) => acc.Rotate90(true)),
                        (rotateCount + 1 % 2) * 1000)
            ).Append((currentDirection, 0))
            .Select(tuple =>
            {
                var (direction, cost) = tuple;
                return ((direction.GetVector().Add(vertex.Location), direction), cost + 1);
            })
            .Where(x => maze.Cells.GetVal(x.Item1.Item1) is not Wall);



        return directionsWithCost;
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