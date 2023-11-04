using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace nyingi.Kafa.Generators
{
    internal sealed partial class KafaSourceGenerator
    {
        private record struct KafaGeneratorContext(string? actualnamespace, string name, TypeContext typeContext);

        private record struct TypeContext(bool isReferenceType, string name, ImmutableArray<PropertyContext> propertyContext);

        private record struct PropertyContext(string type, string name);
        
        private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.ExpandNullable | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    }
}