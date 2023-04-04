// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotApplyDoNotIgnoreToVoidAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA2261RuleId = "CA2261";

        internal static readonly DiagnosticDescriptor DoNotApplyDoNotIgnoreToVoidRule = DiagnosticDescriptorHelper.Create(
            CA2261RuleId,
            CreateLocalizableResourceString(nameof(DoNotApplyDoNotIgnoreToVoidTitle)),
            CreateLocalizableResourceString(nameof(DoNotApplyDoNotIgnoreToVoidMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(DoNotApplyDoNotIgnoreToVoidDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotApplyDoNotIgnoreToVoidRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisDoNotIgnoreAttribute, out var doNotIgnoreAttribute))
                {
                    return;
                }

                context.RegisterSymbolAction(context =>
                {
                    var method = (IMethodSymbol)context.Symbol;

                    // Only proceed if the method is a void with the attribute
                    if (method.ReturnsVoid)
                    {
                        var returnAttributes = method.GetReturnTypeAttributes();

                        if (returnAttributes.Length > 0)
                        {
                            if (returnAttributes.Any(static (attr, doNotIgnore) => doNotIgnore.Equals(attr.AttributeClass, SymbolEqualityComparer.Default), doNotIgnoreAttribute))
                            {
                                context.ReportDiagnostic(method.CreateDiagnostic(DoNotApplyDoNotIgnoreToVoidRule, method.FormatMemberName()));
                            }
                        }
                    }
                    else if (method.IsAsync)
                    {
                        // Not sure how to implement this...
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
