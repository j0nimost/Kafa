using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
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


            IncrementalValuesProvider<KafaGeneratorContext> provider = context.SyntaxProvider
                .CreateSyntaxProvider(SyntacticPredicate, SemanticTransformer)
                .Where(static ((INamedTypeSymbol, INamedTypeSymbol)? types) => types.HasValue)
                .Select(static ((INamedTypeSymbol, INamedTypeSymbol)? types, CancellationToken cancellationToken) =>
                    TransformType(types!.Value));

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
                   && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        private static (INamedTypeSymbol, INamedTypeSymbol)? SemanticTransformer(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            INamedTypeSymbol? namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);


            if (namedTypeSymbol is not null
                && TryGetAttribute(classDeclarationSyntax, _kafaAttributeName, context.SemanticModel, cancellationToken,
                    out AttributeSyntax? attributeSyntax)
                && TryGetType(attributeSyntax, context.SemanticModel, cancellationToken,
                    out INamedTypeSymbol? typeSymbol))
            {
                return (namedTypeSymbol, typeSymbol);
            }

            return null;
        }


        private static bool TryGetAttribute(ClassDeclarationSyntax classDeclarationSyntax, string attributeName,
                                            SemanticModel semanticModel, CancellationToken cancellationToken,
                                            out AttributeSyntax? value)
        {
            foreach (AttributeListSyntax attributeList in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeList.Attributes)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(attributeSyntax, cancellationToken);
                    ISymbol? symbol = info.Symbol;

                    if (symbol is IMethodSymbol methodSymbol
                        && methodSymbol.ContainingType.ToDisplayString()
                            .Equals(attributeName, StringComparison.Ordinal))
                    {
                        value = attributeSyntax;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }


        private static bool TryGetType(AttributeSyntax attributeSyntax, SemanticModel semanticModel,
            CancellationToken cancellationToken, out INamedTypeSymbol? namedTypeSymbol)
        {
            var argumentList = attributeSyntax.ArgumentList;  
            if (argumentList!.Arguments.Count > 1) ;
            {
                AttributeArgumentSyntax argumentSyntax = argumentList.Arguments[0];

                if (argumentSyntax.Expression is TypeOfExpressionSyntax typeOf)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(typeOf.Type, cancellationToken);
                    ISymbol? symbol = info.Symbol;

                    if (symbol is INamedTypeSymbol typeSymbol)
                    {
                        namedTypeSymbol = typeSymbol;
                        return true;
                    }
                }
            }

            namedTypeSymbol = null;
            return false;
        }

        private static KafaGeneratorContext TransformType((INamedTypeSymbol kafaPartialGeneratorType, INamedTypeSymbol targetType) types)
        {
            string? @namespace = types.kafaPartialGeneratorType.ContainingNamespace.IsGlobalNamespace
                ? null
                : types.kafaPartialGeneratorType.ContainingNamespace.ToDisplayString();

            string name = types.kafaPartialGeneratorType.Name;

            bool isReferenceType = types.kafaPartialGeneratorType.IsReferenceType;
            string targetTypeName = types.targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            bool areInternalSymbolsAccessible =
                types.targetType.ContainingAssembly.GivesAccessTo((types.kafaPartialGeneratorType.ContainingAssembly));

            ImmutableArray<PropertyContext> propertyContexts = types.targetType.GetTypeAndSubtypes()
                .Reverse()
                .SelectMany(static type => type.GetMembers())
                .Where(member => FilterProperty(member, areInternalSymbolsAccessible))
                .Select(static member => TransformProperty(member))
                .Distinct()
                .ToImmutableArray();


            return new KafaGeneratorContext(@namespace, name,
                new TypeContext(isReferenceType, targetTypeName, propertyContexts));
        }


        private static bool FilterProperty(ISymbol member, bool areInernalSymbolsAccessible)
        {
            if (!member.IsStatic && member.Kind == SymbolKind.Property)
            {
                var property = (IPropertySymbol)member;

                return property.GetMethod is { } get
                       && (get.DeclaredAccessibility is Accessibility.Public
                           || (areInernalSymbolsAccessible &&
                               (get.DeclaredAccessibility is Accessibility.Internal
                                   or Accessibility.ProtectedOrInternal)));
            }

            return false;
        }

        private static PropertyContext TransformProperty(ISymbol member)
        {
            var property = (IPropertySymbol)member;
            string type = property.Type.ToDisplayString(Format);
            string name = property.Name;

            return new PropertyContext(type, name);
        }
    }
}
