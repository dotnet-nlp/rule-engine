using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(ProjectionCompiler))]
internal sealed class ProjectionCompilerTests
{
    private readonly ProjectionCompiler _compiler = new(
        new TypeFormatter(),
        new CodeEmitter(
            "DotnetNlp.RuleEngine.Core.Projections",
            $"{typeof(ProjectionCompiler).Namespace!}.Generated",
            "ProjectionContainer_",
            "ApplyProjection_"
        )
    );

    [Test]
    [TestCaseSource(nameof(CompilesWithoutArguments_Tuples))]
    [TestCaseSource(nameof(CompilesWithoutArguments_Mixed))]
    public void CompilesWithoutArguments(
        BodyBasedProjectionCompilationData data,
        Type expectedResultType,
        object expectedResult
    )
    {
        const string key = "default";

        IDictionary<string, IRuleProjection> projections = _compiler.CreateProjections(
            new Dictionary<string, IProjectionCompilationData>
            {
                {key, data},
            },
            LoadedAssembliesProvider.Instance
        );

        Assert.IsTrue(projections.ContainsKey(key));

        var projection = projections[key];

        var result = projection.Invoke(
            new ProjectionArguments(
                Array.Empty<string>(),
                CapturedVariablesArguments.Empty,
                RuleArguments.Empty,
                RuleSpaceArguments.Empty
            )
        );

        Assert.IsNotNull(result);
        Assert.AreSame(expectedResultType, result!.GetType());
        Assert.AreEqual(expectedResult, result);
    }

    [Test]
    [TestCaseSource(nameof(CompilesWithArguments_Variable))]
    public void CompilesWithArguments(
        BodyBasedProjectionCompilationData data,
        SortedDictionary<string, object?> arguments,
        Type expectedResultType,
        object expectedResult
    )
    {
        const string key = "default";

        IDictionary<string, IRuleProjection> projections = _compiler.CreateProjections(
            new Dictionary<string, IProjectionCompilationData>
            {
                {key, data},
            },
            LoadedAssembliesProvider.Instance
        );

        Assert.IsTrue(projections.ContainsKey(key));

        var projection = projections[key];

        var result = projection.Invoke(
            new ProjectionArguments(
                Array.Empty<string>(),
                new CapturedVariablesArguments(arguments),
                RuleArguments.Empty,
                RuleSpaceArguments.Empty
            )
        );

        Assert.IsNotNull(result);
        Assert.AreSame(expectedResultType, result!.GetType());
        Assert.AreEqual(expectedResult, result);
    }

    public static object?[][] CompilesWithArguments_Variable =
    {
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof(int),
                new ProjectionParameters(
                    typeof(string[]),
                    new CapturedVariablesParameters(
                        new SortedDictionary<string, Type>
                        {
                            {"a", typeof(int)},
                        }
                    ),
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return a; }"
            ),
            new SortedDictionary<string, object?>
            {
                {"a", 222},
            },
            typeof(int),
            222,
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof(int),
                new ProjectionParameters(
                    typeof(string[]),
                    new CapturedVariablesParameters(
                        new SortedDictionary<string, Type>
                        {
                            {"a", typeof(int)},
                            {"b", typeof(int)},
                        }
                    ),
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return a - b; }"
            ),
            new SortedDictionary<string, object?>
            {
                {"b", 100},
                {"a", 222},
            },
            typeof(int),
            122,
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>
                {
                    "System",
                },
                typeof(TimeSpan),
                new ProjectionParameters(
                    typeof(string[]),
                    new CapturedVariablesParameters(
                        new Dictionary<string, Type>
                        {
                            {"hours", typeof(int)},
                            {"minutes", typeof(int)},
                        }
                    ),
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return TimeSpan.FromHours(hours).Add(System.TimeSpan.FromMinutes(minutes)); }"
            ),
            new SortedDictionary<string, object?>
            {
                {"hours", 3},
                {"minutes", 40},
            },
            typeof(TimeSpan),
            TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(40)),
        },
    };

    public static object?[][] CompilesWithoutArguments_Tuples =
    {
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int, int)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (foo: 2, bar: 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (foo: 2, bar: 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (foo: 2, bar: 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (foo: 2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, bar: 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (foo: 2, bar: 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int foo, int)),
                new ProjectionParameters(typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (foo: 2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof((int, int bar)),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return (2, bar: 3); }"
            ),
            typeof(ValueTuple<int, int>),
            (2, 3),
        },
    };

    public static object?[][] CompilesWithoutArguments_Mixed =
    {
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof(int),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return 1; }"
            ),
            typeof(int),
            1,
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof(string),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return \"string1\"; }"
            ),
            typeof(string),
            "string1",
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>(),
                typeof(TimeSpan),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return System.TimeSpan.FromHours(3); }"
            ),
            typeof(TimeSpan),
            new TimeSpan(3, 0, 0),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>
                {
                    "System",
                },
                typeof(TimeSpan),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return System.TimeSpan.FromHours(3); }"
            ),
            typeof(TimeSpan),
            new TimeSpan(3, 0, 0),
        },
        new object?[]
        {
            new BodyBasedProjectionCompilationData(
                new HashSet<string>
                {
                    "System",
                },
                typeof(TimeSpan),
                new ProjectionParameters(
                    typeof(string[]),
                    CapturedVariablesParameters.Empty,
                    RuleParameters.Empty,
                    RuleSpaceParameters.Empty
                ),
                "{ return TimeSpan.FromHours(3); }"
            ),
            typeof(TimeSpan),
            new TimeSpan(3, 0, 0),
        },
    };
}