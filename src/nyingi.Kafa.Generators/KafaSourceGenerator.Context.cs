using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace nyingi.Kafa.Generators
{
    internal sealed partial class KafaSourceGenerator
    {
        private record struct PropertyContext(string type, string name);
        
        private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.ExpandNullable | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private record struct TypeDefinition(string Name, ImmutableArray<PropertyContext> Properties);

        private record struct TypeDefinitionContext(string NameSpace, string Name,
            ImmutableArray<TypeDefinition> TypeDefinitions)
        {
        }
    }
}