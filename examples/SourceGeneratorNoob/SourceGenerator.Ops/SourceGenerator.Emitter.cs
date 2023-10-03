using Microsoft.CodeAnalysis;

namespace SourceGenerator.Ops
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var compilationSyntaxTree in context.Compilation.SyntaxTrees)
            {

            }   
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
