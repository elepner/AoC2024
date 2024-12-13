using System.Numerics;
using AoC2024;

public static class LinearAlgebra
{
    public static (T, T) Add<T>(this (T, T) v1, (T, T) v2) where T : INumber<T>
    {
        return (v1.Item1 + v2.Item1, v1.Item2 + v2.Item2);
    }

    public static (T, T) Scale<T>(this (T, T) v1, T value) where T : INumber<T>
    {
        return (v1.Item1 * value, v1.Item2 * value);
    }

    public static (T, T) Sub<T>(this (T, T) v1, (T, T) v2) where T : INumber<T>
    {
        return (v1.Item1 - v2.Item1, v1.Item2 - v2.Item2);
    }

    public static Direction Rotate90(this Direction direction, bool clockwise)
    {
        return (Direction)mod(((int)direction + (clockwise ? 1 : -1)), 4);
    }

    private static int mod(int dividend, int divisor)
    {
        return (dividend % divisor + divisor) % divisor;
    }
}