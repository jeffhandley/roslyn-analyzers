﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;
using Analyzer.Utilities.PooledObjects.Extensions;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    internal static class LdapSanitizers
    {
        /// <summary>
        /// <see cref="SanitizerInfo"/>s for LDAP injection sanitizers.
        /// </summary>
        public static ImmutableHashSet<SanitizerInfo> SanitizerInfos { get; }

        static LdapSanitizers()
        {
            var builder = PooledHashSet<SanitizerInfo>.GetInstance();

            builder.AddSanitizerInfo(
                WellKnownTypeNames.MicrosoftSecurityApplicationEncoder,
                isInterface: false,
                isConstructorSanitizing: false,
                sanitizingMethods: new[] {
                    "LdapDistinguishedNameEncode",
                    "LdapEncode",
                    "LdapFilterEncode",
                });

            builder.AddRange(AnySanitizers.SanitizerInfos);

            SanitizerInfos = builder.ToImmutableAndFree();
        }
    }
}
