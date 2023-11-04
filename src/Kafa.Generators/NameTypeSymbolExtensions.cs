using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace nyingi.Kafa.Generators;

internal static class NameTypeSymbolExtensions
{
    
    public static IEnumerable<INamedTypeSymbol> GetTypeAndSubtypes(this INamedTypeSymbol? type)
    {
        INamedTypeSymbol? current = type;
        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
    
}