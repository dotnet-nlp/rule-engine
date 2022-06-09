using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using RuleEngine.Core.Build.Rule.Projection;
using RuleEngine.Core.Build.Rule.Projection.Models;
using RuleEngine.Core.Evaluation.Rule.Projection;
using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using RuleEngine.Core.Lib.CodeAnalysis;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;

namespace RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(ProjectionCompiler))]
internal sealed class ProjectionCompilerTests
{
    private readonly ProjectionCompiler _compiler = new(
        new TypeFormatter(),
        new CodeEmitter(
            "RuleEngine.Core.Projections",
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
            new LoadedAssembliesProvider()
        );

        Assert.IsTrue(projections.ContainsKey(key));

        var projection = projections[key];

        var result = projection.Invoke(
            new ProjectionArguments(
                Array.Empty<string>(),
                new CapturedVariablesArguments(new Dictionary<string, object?>()),
                new RuleArguments(ImmutableDictionary<string, object?>.Empty),
                new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
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
            new LoadedAssembliesProvider()
        );

        Assert.IsTrue(projections.ContainsKey(key));

        var projection = projections[key];

        var result = projection.Invoke(
            new ProjectionArguments(
                Array.Empty<string>(),
                new CapturedVariablesArguments(arguments),
                new RuleArguments(ImmutableDictionary<string, object?>.Empty),
                new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
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
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
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
                    new CapturedVariablesParameters(new Dictionary<string, Type>()),
                    new RuleParameters(new Dictionary<string, Type>()),
                    new RuleSpaceParameters(new Dictionary<string, Type>())
                ),
                "{ return TimeSpan.FromHours(3); }"
            ),
            typeof(TimeSpan),
            new TimeSpan(3, 0, 0),
        },
    };
}