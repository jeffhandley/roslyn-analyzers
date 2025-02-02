﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting.Core;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Composition;

namespace PerformanceTests.Utilities
{
    public static class ExportProviderExtensions
    {
        public static CompositionContext AsCompositionContext(this ExportProvider exportProvider)
        {
            return new CompositionContextShim(exportProvider);
        }

        private class CompositionContextShim : CompositionContext
        {
            private readonly ExportProvider _exportProvider;

            public CompositionContextShim(ExportProvider exportProvider)
            {
                _exportProvider = exportProvider;
            }

            public override bool TryGetExport(CompositionContract contract, [NotNullWhen(true)] out object? export)
            {
                var importMany = contract.MetadataConstraints.Contains(new KeyValuePair<string, object>("IsImportMany", true));
                var (contractType, metadataType) = GetContractType(contract.ContractType, importMany);

                if (metadataType != null)
                {
                    var methodInfo = (from method in _exportProvider.GetType().GetTypeInfo().GetMethods()
                                      where method.Name == nameof(ExportProvider.GetExports)
                                      where method.IsGenericMethod && method.GetGenericArguments().Length == 2
                                      where method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string)
                                      select method).Single();
                    var parameterizedMethod = methodInfo.MakeGenericMethod(contractType, metadataType);
                    export = parameterizedMethod.Invoke(_exportProvider, new[] { contract.ContractName });
                }
                else
                {
                    var methodInfo = (from method in _exportProvider.GetType().GetTypeInfo().GetMethods()
                                      where method.Name == nameof(ExportProvider.GetExports)
                                      where method.IsGenericMethod && method.GetGenericArguments().Length == 1
                                      where method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string)
                                      select method).Single();
                    var parameterizedMethod = methodInfo.MakeGenericMethod(contractType);
                    export = parameterizedMethod.Invoke(_exportProvider, new[] { contract.ContractName });
                }

#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
            }

            private static (Type exportType, Type? metadataType) GetContractType(Type contractType, bool importMany)
            {
                if (importMany && contractType.IsConstructedGenericType)
                {
                    if (contractType.GetGenericTypeDefinition() == typeof(IList<>)
                        || contractType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        || contractType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        contractType = contractType.GenericTypeArguments[0];
                    }
                }

                if (contractType.IsConstructedGenericType)
                {
                    if (contractType.GetGenericTypeDefinition() == typeof(Lazy<>))
                    {
                        return (contractType.GenericTypeArguments[0], null);
                    }
                    else if (contractType.GetGenericTypeDefinition() == typeof(Lazy<,>))
                    {
                        return (contractType.GenericTypeArguments[0], contractType.GenericTypeArguments[1]);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                throw new NotSupportedException();
            }
        }
    }
}
