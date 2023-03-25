// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreOutParamValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreOutParamValueCSharpTests
    {
        private readonly DiagnosticDescriptor doNotIgnoreRule = DoNotIgnoreOutParamValueAnalyzer.DoNotIgnoreOutParamValueRule;

        private const string attributeImplementationCSharp = $$"""
            namespace System.Diagnostics.CodeAnalysis
            {
                [System.AttributeUsage(System.AttributeTargets.ReturnValue | System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
                public class DoNotIgnoreAttribute : System.Attribute
                {
                    public DoNotIgnoreAttribute() { }
                    public string Message { get; set; }
                }
            }
            """;

        [Fact]
        public async Task UnannotatedMethod_IgnoringOutParamValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void UnannotatedMethod(out int value) => value = 1;

                    void M()
                    {
                        UnannotatedMethod(out int value);
                    }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_ConsumingOutParamValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void UnannotatedMethod(out int value) => value = 1;

                    void M()
                    {
                        UnannotatedMethod(out int value);
                        if (value != 1) throw new System.Exception();
                    }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_DiscardingOutParamValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void UnannotatedMethod(out int value) => value = 1;

                    void M()
                    {
                        UnannotatedMethod(out _);
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringOutParamValue_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void AnnotatedMethod([System.Diagnostics.CodeAnalysis.DoNotIgnore] out int value) => value = 1;

                    void M()
                    {
                        AnnotatedMethod({|#1:out int value|});
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("value"));
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingOutParamValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void AnnotatedMethod([System.Diagnostics.CodeAnalysis.DoNotIgnore] out int value) => value = 1;

                    void M()
                    {
                        AnnotatedMethod(out int value);
                        if (value != 1) throw new System.Exception();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_DiscardingOutParamValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void AnnotatedMethod([System.Diagnostics.CodeAnalysis.DoNotIgnore] out int value) => value = 1;

                    void M()
                    {
                        AnnotatedMethod(out _);
                    }
                }
                """);
        }
    }
}
