using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace nyingi.Kafa.Generators
{

    [Generator]
    internal sealed partial class KafaSourceGenerator : IIncrementalGenerator
    {
        private const string _kafaAttributeName = "nyingi.Kafa.Generators.KafaSerializableAttribute";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(PostInitializationCallback);

            IncrementalValuesProvider<TypeDefinitionContext> provider = context.SyntaxProvider
                .ForAttributeWithMetadataName(_kafaAttributeName, SyntacticPredicate, SemanticTransformer)
                .Collect()
                .SelectMany(
                    static ImmutableArray<IGrouping<ISymbol?, KafaEnumContext>> (ImmutableArray<KafaEnumContext> source,
                        CancellationToken cancellationToken) =>
                    {
                        return source
                            .GroupBy(c => c.NamedTypeSymbol, SymbolEqualityComparer.Default)
                            .ToImmutableArray();
                    })
                .Select(static TypeDefinitionContext (IGrouping<ISymbol?, KafaEnumContext> sources,
                    CancellationToken cancellationToken) =>
                {
                    var source = sources.First();

                    var typeDefinitions = ImmutableArray.CreateBuilder<TypeDefinition>(source.AttributeList.Count);

                    foreach (var attributeListSyntax in source.AttributeList)
                    {
                        foreach (var attributeList in attributeListSyntax.Attributes)
                        {
                            var attributeArgument = attributeList.ArgumentList!.Arguments[0]; // first param
                            SymbolInfo info = source.SemanticModel.GetSymbolInfo(attributeArgument);
                            var symbol = (ITypeSymbol)info.Symbol;
                            var members = symbol.GetMembers()
                                .Where(static bool (ISymbol member) => member.Kind == SymbolKind.Field)
                                .Select(static PropertyContext (ISymbol propertySymbol) =>
                                    new PropertyContext(propertySymbol.Name, propertySymbol.ToDisplayString(Format)))
                                .ToImmutableArray();
                            var typeDefinition = new TypeDefinition(symbol.ToDisplayString(Format), members);
                            typeDefinitions.Add(typeDefinition);
                        }
                    }

                    return new TypeDefinitionContext(source.NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace,
                        source.NamedTypeSymbol.ContainingNamespace.ToDisplayString(),
                        source.NamedTypeSymbol.Name,
                        typeDefinitions.MoveToImmutable());
                });

        }

        private static void PostInitializationCallback(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("KafaSerializableAttribute.g.cs", SourceText.From(KafaSerializableAttribute, Encoding.UTF8));
        }
        private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            return syntaxNode is ClassDeclarationSyntax
                   {
                       AttributeLists.Count: > 0,
                   } candidate
                   && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
                   && candidate.Modifiers.Any(SyntaxKind.StaticKeyword); // might need the definition to be static for easier invoke
        }

        private static KafaEnumContext SemanticTransformer(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;

            var namedTypeSymbol = (INamedTypeSymbol)context.TargetSymbol;


            return new KafaEnumContext(classDeclarationSyntax, namedTypeSymbol, context.SemanticModel, classDeclarationSyntax.AttributeLists);
        }
    }
}
