﻿namespace StyleCop.Analyzers.OrderingRules
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// A using directive which declares a member of the <see cref="System"/> namespace appears after a using directive
    /// which declares a member of a different namespace, within a C# code file.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a using directive for the <see cref="System"/> namespace is placed
    /// after a non-<see cref="System"/> using directive. Placing all <see cref="System"/> using directives at the top
    /// of the using directives can make the code cleaner and easier to read, and can help make it easier to identify
    /// the namespaces that are being used by the code.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the
        /// <see cref="SA1208SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1208";
        private const string Title = "System using directives must be placed before other using directives";
        private const string MessageFormat = "Using directive for '{0}' must appear before directive for '{1}'";
        private const string Category = "StyleCop.CSharp.OrderingRules";
        private const string Description = "A using directive which declares a member of the 'System' namespace appears after a using directive which declares a member of a different namespace, within a C# code file.";
        private const string HelpLink = "http://www.stylecop.com/docs/SA1208.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleCompilationUnit, SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
        }

        private void HandleCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = context.Node as CompilationUnitSyntax;

            var usings = compilationUnit.Usings;

            ProcessUsingsAndReportDiagnostic(usings, context);
        }

        private void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = context.Node as NamespaceDeclarationSyntax;

            var usings = namespaceDeclaration.Usings;

            ProcessUsingsAndReportDiagnostic(usings, context);
        }

        private static void ProcessUsingsAndReportDiagnostic(SyntaxList<UsingDirectiveSyntax> usings, SyntaxNodeAnalysisContext context)
        {
            string systemUsingDirectivesShouldBeBeforeThisName = null;
            for (var i = 1; i < usings.Count; i++)
            {
                var usingDirective = usings[i];

                if (usingDirective.Alias != null || !usingDirective.StaticKeyword.IsKind(SyntaxKind.None))
                {
                    continue;
                }

                if (usingDirective.IsSystemUsingDirective())
                {
                    if (systemUsingDirectivesShouldBeBeforeThisName != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation(), usingDirective.Name.ToUnaliasedString(), systemUsingDirectivesShouldBeBeforeThisName));
                        continue;
                    }

                    var previousUsing = usings[i - 1];

                    if (!previousUsing.IsSystemUsingDirective() || previousUsing.StaticKeyword.Kind() != SyntaxKind.None)
                    {
                        systemUsingDirectivesShouldBeBeforeThisName = previousUsing.Name.ToUnaliasedString();
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation(), usingDirective.Name.ToUnaliasedString(), systemUsingDirectivesShouldBeBeforeThisName));
                    }
                }
            }
        }
    }
}
