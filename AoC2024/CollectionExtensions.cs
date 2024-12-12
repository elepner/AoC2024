namespace AoC2024;

public static class CollectionExtension
{

    public static IEnumerable<(int, int)> EnumerateCoords<T>(this T[][] field)
    {
        var (rows, cols) = field.GetDims();

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                yield return (i, j);
            }
        }
    }

    public static (int, int) GetDims<T>(this T[][] field)
    {
        return (field.Length, field[0].Length);
    }

    public static bool CheckBounds<T>(this T[][] field, (int, int) coords)
    {
        return CheckBounds(coords, field.GetDims());
    }

    private static bool CheckBounds((int, int) coords, (int, int) dims)
    {
        var (row, col) = coords;
        var (rows, cols) = dims;
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }

    public static IEnumerable<(int, int)> WithinBoundsOf<T>(this IEnumerable<(int, int)> coords, T[][] field)
    {
        var dims = field.GetDims();
        return coords.Where(xy => CheckBounds(xy, dims));
    }

    public static T GetVal<T>(this T[][] field, (int, int) coords)
    {
        return field[coords.Item1][coords.Item2];
    }

    public static void SetVal<T>(this T[][] field, (int, int) coords, T value)
    {
        field[coords.Item1][coords.Item2] = value;

    }

    public static FieldValue<T> GetFieldValue<T>(this T[][] field, (int, int) coords)
    {
        return new FieldValue<T>(coords, field[coords.Item1][coords.Item2]);
    }

    public static FieldValue<T> GetFieldValue<T>(this (int, int) coords, T[][] field)
    {
        return new FieldValue<T>(coords, field[coords.Item1][coords.Item2]);
    }

    public static IEnumerable<(T, T)> Pairwise<T>(IEnumerable<T> col)
    {
        return col.Zip(col.Skip(1));
    }

    public static IEnumerable<(T, T)> AllCombinations<T>(this IList<T> collection)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            for (var j = i + 1; j < collection.Count; j++)
            {
                yield return (collection[i], collection[j]);
            }
        }
    }


    public static (int, int) GetVector(this Direction direction)
    {

        return direction switch
        {
            Direction.N => (-1, 0),
            Direction.S => (1, 0),
            Direction.E => (0, 1),
            Direction.W => (0, -1),
            _ => throw new ArgumentException()
        };
    }

    public static Direction[] AllDirections()
    {
        return Enumerable.Range(0, 4).Select(x => (Direction)x).ToArray();
    }

    public static IEnumerable<(Direction Direction, (int, int) Point)> Around(this (int, int) v)
    {
        return AllDirections().Select(x => (x, x.GetVector().Add(v)));
    }
}

public enum Direction
{
    N = 0,
    E = 1,
    S = 2,
    W = 3

}

public record struct FieldValue<T>((int, int) Coordinates, T Value);