// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotApplyDoNotIgnoreToVoidAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotApplyDoNotIgnoreToVoidCSharpTests
    {
        private readonly DiagnosticDescriptor doNotApplyRule = DoNotApplyDoNotIgnoreToVoidAnalyzer.DoNotApplyDoNotIgnoreToVoidRule;

        private const string attributeImplementationCSharp = $$"""
            namespace System.Diagnostics.CodeAnalysis
            {
                [System.AttributeUsage(System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
                internal class DoNotIgnoreAttribute : System.Attribute
                {
                    public DoNotIgnoreAttribute() { }
                    public string Message { get; set; }
                }
            }
            """;

        [Fact]
        public async Task UnannotatedMethod_Void_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    void UnannotatedVoid() { }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_Int_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    int UnannotatedMethod() => 1;
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_Task_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    System.Threading.Tasks.Task UnannotatedMethod() => System.Threading.Tasks.Task.CompletedTask;
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_TaskOfT_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    System.Threading.Tasks.Task<int> UnannotatedMethod() => System.Threading.Tasks.Task.FromResult(1);
                }
                """);
        }

        [Fact]
        public async Task UnannotatedAsyncMethod_Task_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    async System.Threading.Tasks.Task UnannotatedMethod() { }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedAsyncMethod_TaskOfT_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    async System.Threading.Tasks.Task<int> UnannotatedMethod() => 1;
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_Void_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    void {|#1:AnnotatedVoid|}() { }
                }
                """,
                VerifyCS.Diagnostic(doNotApplyRule).WithLocation(1).WithArguments("C.AnnotatedVoid()")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_Int_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_Task_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    System.Threading.Tasks.Task AnnotatedMethod() => System.Threading.Tasks.Task.CompletedTask;
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_TaskOfT_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    System.Threading.Tasks.Task<int> AnnotatedMethod() => System.Threading.Tasks.Task.FromResult(1);
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_Task_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task {|#1:AnnotatedMethod|}() { }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_TaskOfT_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedMethod() => 1;
                }
                """);
        }
    }
}
