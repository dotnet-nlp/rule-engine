using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.CodeAnalysis.Models;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Lib.CodeAnalysis;

public sealed class CodeEmitter
{
    private readonly string _assemblyPrefix;
    private readonly string _codeNamespace;
    private readonly string _classPrefix;
    private readonly string _methodPrefix;

    public CodeEmitter(string assemblyPrefix, string codeNamespace, string classPrefix, string methodPrefix)
    {
        _assemblyPrefix = assemblyPrefix;
        _codeNamespace = codeNamespace;
        _classPrefix = classPrefix;
        _methodPrefix = methodPrefix;
    }

    public Func<object?[], object?> CreateFunction(
        FunctionCreationData data,
        IAssembliesProvider assembliesProvider
    )
    {
        const string key = "default";

        var functions = CreateFunctions(
            new Dictionary<string, FunctionCreationData>(1)
            {
                { key, data }
            },
            assembliesProvider
        );

        return functions[key];
    }

    public IDictionary<string, Func<object?[], object?>> CreateFunctions(
        IReadOnlyDictionary<string, FunctionCreationData> dataByMethodName,
        IAssembliesProvider assembliesProvider
    )
    {
        var compilationResult = CompileAssembly(
            dataByMethodName
                .MapValue(
                    data => CreateNamespace(
                        data.Usings,
                        CreateStaticClass(
                            CreateClassName(data.Name),
                            CreateStaticMethod(
                                CreateMethodName(data.Name),
                                data.ReturnTypeDeclaration,
                                data.Parameters,
                                data.Body
                            )
                        )
                    )
                )
                .SelectValues()
                .JoinToString("\r\n\r\n\r\n"),
            assembliesProvider.GetMetadataReferences()
        );

        return dataByMethodName
            .MapValue<string, FunctionCreationData, Func<object?[], object?>>(
                functionCreationData =>
                {
                    var methodInfo = compilationResult
                        .Assembly
                        .GetType($"{_codeNamespace}.{CreateClassName(functionCreationData.Name)}")!
                        .GetMethod(CreateMethodName(functionCreationData.Name))!;

                    return arguments => methodInfo.Invoke(null, arguments)!;
                }
            )
            .ToDictionaryWithKnownCapacity(dataByMethodName.Count);
    }

    private string CreateClassName(string key)
    {
        return $"{_classPrefix}{key}";
    }

    private string CreateMethodName(string key)
    {
        return $"{_methodPrefix}{key}";
    }

    private string CreateNamespace(IEnumerable<string> usingNamespaces, string content)
    {
        return $@"namespace {_codeNamespace}
{{
{usingNamespaces.Select(@namespace => $"    using {@namespace};").JoinToString("\r\n")}

{content}
}}";
    }

    private static string CreateStaticClass(string name, string content)
    {
        return $@"    public static class {name}
    {{
{content}
    }}";
    }

    private static string CreateStaticMethod(string methodName, string returnType, IEnumerable<VariableCreationData> parameters, string body)
    {
        return $@"        public static {returnType} {methodName}({parameters.Select(parameter => $"{parameter.TypeDeclaration} {parameter.Name}").JoinToString(", ")})
        {body}";
    }

    private (Assembly Assembly, AssemblyMetadata Metadata, string ReformattedCode) CompileAssembly(
        string code,
        IEnumerable<MetadataReference> metadataReferences
    )
    {
        var reformattedCode = Reformat(code);

        var syntaxTree = Parse(reformattedCode);

        var compilation = CSharpCompilation.Create(
            $"{_assemblyPrefix}_{Guid.NewGuid():N}",
            new [] { syntaxTree },
            metadataReferences,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release
            )
        );

        using var stream = new MemoryStream();
        using var streamPdb = new MemoryStream();

        var result = compilation.Emit(
            stream,
            streamPdb,
            options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb)
        );

        if (!result.Success)
        {
            var failures = result
                .Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .ToList();

            throw new CodeEmitException("Cannot create assembly", reformattedCode, failures);
        }

        stream.Seek(0, SeekOrigin.Begin);
        streamPdb.Seek(0, SeekOrigin.Begin);

        var rawAssembly = stream.ToArray();
        var rawPdb = streamPdb.ToArray();

        var compiledAssembly = Assembly.Load(rawAssembly, rawPdb);

        var assemblyMetadata = AssemblyMetadata.CreateFromImage(ImmutableArray.Create(rawAssembly));

        return (compiledAssembly, assemblyMetadata, reformattedCode);
    }

    private static SyntaxTree Parse(string code)
    {
        return CSharpSyntaxTree.ParseText(
            code,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None)
        );
    }

    private static string Reformat(string code)
    {
        return Parse(code).GetRoot().NormalizeWhitespace().ToFullString();
    }
}