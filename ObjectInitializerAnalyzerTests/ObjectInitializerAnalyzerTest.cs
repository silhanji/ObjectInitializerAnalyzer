using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;

namespace ObjectInitializerAnalyzerTests
{
    [TestFixture]
    public class ObjectInitializerAnalyzerTest : AnalyzerVerifier<ObjectInitializerAnalyzer.ObjectInitializerAnalyzer>
    {

        [Test]
        public async Task Test_NoError()
        {
            string testCode = @"
public static class Sample
{
    public static void Main()
    {
        var model = new Model
        {
            Text = ""text"",
            Number = 13,
        };
    }
}

public class Model
{
    public string Text { get; set; }
        
    public int Number { get; set; }
        
    private int PrivateNumber { get; set; }
}";

            var test = new CSharpAnalyzerTest<ObjectInitializerAnalyzer.ObjectInitializerAnalyzer, NUnitVerifier>
            {
                TestState =
                {
                    Sources = { testCode },
                },
                ExpectedDiagnostics =
                {
                }
            };
			
            await test.RunAsync();
        }
        
        [Test]
        public async Task Test_WithError()
        {
            string testCode = @"
public static class Sample
{
    public static void Main()
    {
        var model = new Model
        {
            Text = ""text"",
        };
    }
}

public class Model
{
    public string Text { get; set; }
        
    public int Number { get; set; }
        
    private int PrivateNumber { get; set; }
}";
            var diagnostics = new DiagnosticResult("OIA001", DiagnosticSeverity.Warning)
                .WithSpan(startLine: 7, startColumn: 9, endLine: 9, endColumn: 10);

            var test = new CSharpAnalyzerTest<ObjectInitializerAnalyzer.ObjectInitializerAnalyzer, NUnitVerifier>
            {
                TestState =
                {
                    Sources = { testCode },
                },
                ExpectedDiagnostics =
                {
                    diagnostics
                }
            };
			
            await test.RunAsync();
        }
    }
}