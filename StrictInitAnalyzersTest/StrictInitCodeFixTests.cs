using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;
using StrictInit.Analyzers;

namespace StrictInitAnalyzersTest;

[TestFixture]
public class StrictInitCodeFixTests : CodeFixVerifier<StrictInitAnalyzer, StrictInitCodeFix>
{
    [Test]
    public async Task Test()
    {
        await new CSharpCodeFixTest<StrictInitAnalyzer, StrictInitCodeFix, NUnitVerifier>
        {
            TestCode = @"
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
}",
            FixedCode = @"
public static class Sample
{
    public static void Main()
    {
        var model = new Model
        {
            Text = ""text"",
            Number = default
        };
    }
}

public class Model
{
    public string Text { get; set; }
        
    public int Number { get; set; }
        
    private int PrivateNumber { get; set; }
}",
            ExpectedDiagnostics =
                { 
                    new DiagnosticResult("SI001", DiagnosticSeverity.Info)
                        .WithSpan(7, 9, 9, 10)
                        .WithArguments("Number") 
                }
        }.RunAsync();
    }
}