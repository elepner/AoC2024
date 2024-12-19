namespace AoC2024;

internal static class Utils
{
    public static string ReadTaskInput(int day)
    {
        return File.ReadAllText($"TestAssets/day{day}.txt");
    }
}