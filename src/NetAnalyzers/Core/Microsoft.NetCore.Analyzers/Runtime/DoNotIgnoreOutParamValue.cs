// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    public abstract class DoNotIgnoreOutParamValueAnalyzer : DiagnosticAnalyzer
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
    }
}
