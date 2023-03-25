// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.NetCore.Analyzers.Runtime;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpDoNotIgnoreOutParamValueAnalyzer : DoNotIgnoreOutParamValueAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotIgnoreOutParamValueRule);

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

                context.RegisterCodeBlockStartAction<SyntaxKind>(context =>
                {
                    context.RegisterSyntaxNodeAction(context =>
                    {
                        if (context.Node is ArgumentSyntax argument)
                        {
                            if (argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
                            {
                                var expression = argument.Parent.Parent.Parent;
                                var dataFlow = context.SemanticModel.AnalyzeDataFlow(expression);

                                if (dataFlow.DataFlowsOut.Length < dataFlow.VariablesDeclared.Length)
                                {
                                }
                            }
                            /*
                             * argument.RefKindKeyword: OutKeyword
                             * argument.RefOrOutKeyword: OutKeyword
                             */


                            /* Ignored
                             * AlwaysAssigned: Length = 1
                             * DataFlowsIn: Length = 1
                             * DataFlowsOut: Length = 0
                             * VariablesDeclared: Length = 1
                             */

                            /* Consumed
                             * AlwaysAssigned = 1 (value)
                             * DataFlowsIn = 1 (C)
                             * DataFlowsOut = 1 (value)
                             * VariablesDeclared = 1 (value)
                             */

                            /* Discarded
                             * AlwaysAssigned = 0
                             * DataFlowsIn = 1
                             * DataFlowsOut = 0
                             * VariablesDeclared = 0
                             */
                        }
                    }, SyntaxKind.Argument);
                });
            });
        }
    }
}