﻿using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpEssentials.UseNameOf
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Use NameOf")]
    internal class UseNameOfCodeFix : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(CancellationToken.None);

            var literalExpression = root.FindNode(context.Span, getInnermostNodeForTie: true) as LiteralExpressionSyntax;
            if (literalExpression != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create("Use NameOf", c => ReplaceWithNameOf(context.Document, literalExpression, c)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> ReplaceWithNameOf(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
        {
            var stringText = literalExpression.Token.ValueText;

            var nameOfExpression = InvocationExpression(
                expression: IdentifierName("nameof"),
                argumentList: ArgumentList(
                    arguments: SingletonSeparatedList(Argument(IdentifierName(stringText)))));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(literalExpression, nameOfExpression);

            return document.WithSyntaxRoot(newRoot);
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.UseNameOf);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}
