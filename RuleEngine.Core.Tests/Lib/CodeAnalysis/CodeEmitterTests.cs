using System.Collections.Generic;
using NUnit.Framework;
using RuleEngine.Core.Lib.CodeAnalysis;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.CodeAnalysis.Models;

namespace RuleEngine.Core.Tests.Lib.CodeAnalysis;

[TestFixture(TestOf = typeof(CodeEmitter))]
internal sealed class CodeEmitterTests
{
    private CodeEmitter? _codeEmitter;

    [SetUp]
    public void SetUp()
    {
        _codeEmitter = new CodeEmitter("foo_", "Foo", "bar_", "faz_");
    }

    [Test]
    [TestCaseSource(nameof(CreatesFunction_Mixed))]
    public void CreatesFunction(
        string returnTypeDeclaration,
        string body,
        IEnumerable<VariableCreationData> parameters,
        object?[] arguments,
        object? expectedResult
    )
    {
        var function = _codeEmitter!.CreateFunction(
            new FunctionCreationData(
                new HashSet<string>
                {
                    "System.Linq",
                },
                returnTypeDeclaration,
                "Function",
                parameters,
                body
            ),
            new LoadedAssembliesProvider()
        );

        var result = function.Invoke(arguments);

        Assert.AreEqual(expectedResult, result);
    }

    public static object?[][] CreatesFunction_Mixed()
    {
        return new[]
        {
            new object?[]
            {
                "string",
                " { return new string(word.ToCharArray().Reverse().ToArray()); }",
                new []
                {
                    new VariableCreationData("word", "string"),
                },
                new []
                {
                    "слово",
                },
                "оволс",
            },
        };
    }
}