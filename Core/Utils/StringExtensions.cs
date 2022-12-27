using System.Linq;

namespace Core.Utils;

public static class StringExtensions
{
    public static bool StartsWithBitsNumber(this string value, int amount)
    {
        return value
            .ToBits()
            .Take(amount)
            .All(bit => !bit);
    }
}