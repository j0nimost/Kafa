using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace nyingi.Kafa.Generators;

public static class NamedTypeSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetThisAndSubtypes(this INamedTypeSymbol? type)
    {
        INamedTypeSymbol? current = type;
        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}