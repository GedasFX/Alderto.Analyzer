using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alderto.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AldertoAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ADT0001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Dependancy Injection";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeConstructorNode, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeConstructorNode(SyntaxNodeAnalysisContext context)
        {
            var constructor = (ConstructorDeclarationSyntax)context.Node;

            if (!constructor.Identifier.ValueText.EndsWith("Controller"))
                return;

            var paramList = constructor.ParameterList.Parameters;
            foreach (var parameter in paramList)
            {
                if (!(parameter.Type is IdentifierNameSyntax param) ||
                    !param.Identifier.ValueText.EndsWith("DbContext")) continue;

                var diagnostic = Diagnostic.Create(Rule, parameter.GetLocation(), param.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
