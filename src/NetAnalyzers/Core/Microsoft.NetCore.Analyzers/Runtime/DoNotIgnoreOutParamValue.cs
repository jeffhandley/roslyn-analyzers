// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotIgnoreOutParamValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA2023RuleId = "CA2023";

        internal static readonly DiagnosticDescriptor DoNotIgnoreOutParamValueRule = DiagnosticDescriptorHelper.Create(
            CA2023RuleId,
            CreateLocalizableResourceString(nameof(DoNotIgnoreOutParamValueTitle)),
            CreateLocalizableResourceString(nameof(DoNotIgnoreOutParamValueMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotIgnoreOutParamValueDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotIgnoreOutParamValueRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationBlockStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisDoNotIgnoreAttribute, out var doNotIgnoreAttribute))
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    if (context.Operation is IInvocationOperation invocation)
                    {
                        var outParamsToNotIgnore = invocation.Arguments.WhereAsArray(arg =>
                            arg.Parameter.RefKind == RefKind.Out &&
                            arg.Parameter.HasAttribute(doNotIgnoreAttribute));

                        foreach (var arg in outParamsToNotIgnore)
                        {
                            if (arg.Value is not IDiscardOperation)
                            {
                                arg.CreateDiagnostic(DoNotIgnoreOutParamValueRule, arg.Parameter.FormatMemberName());
                            }
                        }
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}