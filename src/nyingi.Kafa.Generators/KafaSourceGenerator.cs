using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        private const string KafaAttributeName = "global::nyingi.Kafa.KafaSerializableAttribute";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(PostInitializationCallback);
            IncrementalValuesProvider<TypeDefinitionContext> provider = context.SyntaxProvider
                .CreateSyntaxProvider(SyntacticPredicate, SemanticTransformer)
                .Where(static (typeDefinition) => typeDefinition.Value.Length > 0)
                .Select(static(typeDefinitions, _) => Transform(typeDefinitions));
            context.RegisterSourceOutput(provider, Emit);
        }

        private static void PostInitializationCallback(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("KafaSerializableAttribute.g.cs", SourceText.From(KafaSerializableAttribute, Encoding.UTF8));
        }
        private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            Debug.WriteLine(syntaxNode.GetText().Lines);
            return syntaxNode is ClassDeclarationSyntax
                   {
                       AttributeLists.Count: > 0,
                   } candidate
                   && candidate.Modifiers.Any(SyntaxKind.PartialKeyword);
        }

        // TODO: Consider multiple Different Class Declarations 
        private static KeyValuePair<INamedTypeSymbol,ImmutableArray<INamedTypeSymbol>> SemanticTransformer(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var attributeSyntaxes = ImmutableArray.CreateBuilder<AttributeSyntax>();

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);
            GetAttributes(context, classDeclarationSyntax,ref attributeSyntaxes, cancellationToken);

            var typeSymbols = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
            foreach (var attributeSyntax in attributeSyntaxes)
            {
                if (TryGetTypes(attributeSyntax, context.SemanticModel, cancellationToken,
                        out INamedTypeSymbol? typeSymbol))
                {
                    // add to list
                    typeSymbols.Add(typeSymbol);
                }
            }

            return new (classSymbol, typeSymbols.ToImmutableArray());
        }

        private static void GetAttributes(GeneratorSyntaxContext context,
            ClassDeclarationSyntax classDeclarationSyntax, ref ImmutableArray<AttributeSyntax>.Builder attributeSyntaxes, CancellationToken cancellationToken)
        {
            foreach (var attributeList in classDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeList.Attributes)
                {
                    SymbolInfo info = context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken);
                    ISymbol attributeSymbol = info.Symbol;

                    Debug.WriteLine(attributeSymbol.ContainingType.ToDisplayString(Format));
                    Debug.WriteLine(attributeSymbol.ContainingSymbol.ToDisplayString(Format));
                    Debug.WriteLine(attributeSymbol.ContainingNamespace.ToDisplayString(Format));
                    if (attributeSymbol is IMethodSymbol methodSymbol
                        && methodSymbol.ContainingType.ToDisplayString(Format)
                            .Equals(KafaAttributeName, StringComparison.Ordinal))
                    {
                        // add to list
                        attributeSyntaxes.Add(attributeSyntax);
                    }
                }
            }
        }

        private static bool TryGetTypes(AttributeSyntax attributeSyntax, SemanticModel semanticModel,
            CancellationToken cancellationToken, out INamedTypeSymbol? typeSymbol)
        {
            if (attributeSyntax.ArgumentList is
                {
                    Arguments.Count: 1,
                } argumentList)
            {
                var attributeArgumentSyntax = argumentList.Arguments[0];

                if (attributeArgumentSyntax.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(typeOfExpressionSyntax.Type, cancellationToken);
                    ISymbol? symbol = symbolInfo.Symbol;

                    if (symbol is INamedTypeSymbol namedTypeSymbol)
                    {
                        typeSymbol = namedTypeSymbol;
                        return true;
                    }
                }
            }

            typeSymbol = null;
            return false;
        }

        private static TypeDefinitionContext Transform(KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> typeDefinitions)
        {
            var classDeclarationSymbol = typeDefinitions.Key;
            string? @namespace = classDeclarationSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classDeclarationSymbol.ContainingNamespace.ToDisplayString();

            string className = classDeclarationSymbol.Name;
            var typeDefinitionBuilder = ImmutableArray.CreateBuilder<TypeDefinition>();
            foreach (var typeSymbol in typeDefinitions.Value)
            {
                var targetType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var properties = typeSymbol.GetThisAndSubtypes()
                    .SelectMany(static type => type.GetMembers())
                    .Where(member => FilterProperty(member, typeSymbol.ContainingAssembly.GivesAccessTo(classDeclarationSymbol.ContainingAssembly)))
                    .Select(static member => TransformProperty(member))
                    .Distinct()
                    .ToImmutableArray();
                var typeDef = new TypeDefinition(targetType, properties);
                typeDefinitionBuilder.Add(typeDef);
            }
            
            
            return new(@namespace, className, typeDefinitionBuilder.ToImmutableArray());
        }

        private static bool FilterProperty(ISymbol member, bool areInternalSymbolsAccessible)
        {
            if (!member.IsStatic
                && member.Kind == SymbolKind.Property)
            {
                Debug.Assert(member is IPropertySymbol);
                var property = (IPropertySymbol)member;

                return (property.SetMethod is { } set
                       && (set.DeclaredAccessibility is Accessibility.Public
                           || (areInternalSymbolsAccessible && (set.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)))
                       &&
                       property.GetMethod is { } get
                       && (get.DeclaredAccessibility is Accessibility.Public
                           || (areInternalSymbolsAccessible && (get.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)))
                       );
            }

            return false;
        }
        private static PropertyContext TransformProperty(ISymbol symbol)
        {

            // if (symbol is IFieldSymbol)
            // {
            //     var fieldSymbol = (IFieldSymbol)symbol;
            //     return new(fieldSymbol.Type.ToDisplayString(Format), fieldSymbol.Name);
            // }
            // else if(symbol is IPropertySymbol)
            // {
            //
            // }
            var propertySymbol = (IPropertySymbol)symbol;
            return new(propertySymbol.Type.ToDisplayString(Format), propertySymbol.Name);
        }
    }
}
