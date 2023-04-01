﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotIgnoreReturnValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA2022RuleId = "CA2022";
        private static readonly LocalizableString s_title = CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueTitle));
        private static readonly LocalizableString s_description = CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueDescription));

        internal static readonly DiagnosticDescriptor DoNotIgnoreReturnValueRule = DiagnosticDescriptorHelper.Create(
            CA2022RuleId,
            s_title,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_description,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor DoNotIgnoreReturnValueRuleWithMessage = DiagnosticDescriptorHelper.Create(
            CA2022RuleId,
            s_title,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueMessageCustom)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_description,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotIgnoreReturnValueRule, DoNotIgnoreReturnValueRuleWithMessage);

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

                context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask1, out var taskOfT);

                context.RegisterOperationAction(context =>
                {
                    var callSite = context.Operation;
                    var method = ((IInvocationOperation)callSite).TargetMethod;

                    // It would be an authoring error, but ensure the method returns a value
                    if (method.ReturnsVoid)
                    {
                        return;
                    }

                    // If the call is awaited, and the method returns a Task<T>, then we'll check the await result for consumption
                    if (taskOfT is not null && callSite.Parent?.Kind is OperationKind.Await && (method.ReturnType as INamedTypeSymbol)?.ConstructedFrom == taskOfT)
                    {
                        callSite = callSite.Parent;
                    }

                    // If the call site is simply an expression statement, then the return value is not being consumed in any way
                    if (callSite.Parent?.Kind is OperationKind.ExpressionStatement)
                    {
                        var attributeApplied = method.GetReturnTypeAttributes()
                            .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, doNotIgnoreAttribute));

                        if (attributeApplied is not null)
                        {
                            var message = attributeApplied.NamedArguments
                                .Where(arg => arg.Key == "Message")
                                .Select(arg => arg.Value.Value as string)
                                .FirstOrDefault();

                            if (!RoslynString.IsNullOrEmpty(message))
                            {
                                context.ReportDiagnostic(callSite.CreateDiagnostic(DoNotIgnoreReturnValueRuleWithMessage, method.FormatMemberName(), message));
                            }
                            else
                            {
                                context.ReportDiagnostic(callSite.CreateDiagnostic(DoNotIgnoreReturnValueRule, method.FormatMemberName()));
                            }
                        }
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}
