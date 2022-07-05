[![Release NuGet packages on release created](https://github.com/dotnet-nlp/rule-engine/actions/workflows/nuget-release.yml/badge.svg)](https://github.com/dotnet-nlp/rule-engine/actions/workflows/nuget-release.yml)
[![Release PyPI packages on dotnet release created](https://github.com/dotnet-nlp/rule-engine-python/actions/workflows/pypi-publish.yml/badge.svg)](https://github.com/dotnet-nlp/rule-engine-python/actions/workflows/pypi-publish.yml)

Rule Engine is a pattern matching library, which allows including multiple mechanics to detect if the phrase matches some specific rule.

## Installation

1. Add `https://nuget.pkg.github.com/dotnet-nlp` as NuGet package source:
```
dotnet nuget add source "https://nuget.pkg.github.com/dotnet-nlp/index.json" --name="Dotnet NLP" --username USERNAME --password GITHUB_TOKEN --store-password-in-clear-text
```

(read more about [working with the GitHub NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry))

2. Install `DotnetNlp.RuleEngine.Bundle` package with NuGet:
```
dotnet add package DotnetNlp.RuleEngine.Bundle
```

## Usage

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Bundle;

var ruleSpace = Factory.Create(
    new Dictionary<string, string>()
    {
        {
            "number",
            @"
using DotnetNlp.RuleEngine.Bundle;

int Root = peg#($Number:n)# { return n; }
int Number = peg#(
    $Number_0:n_0|
    $Number_1:n_1|
    $Number_2:n_2|
    $Number_3:n_3|
    $Number_4:n_4|
    $Number_5:n_5|
    $Number_6:n_6|
    $Number_7:n_7|
    $Number_8:n_8|
    $Number_9:n_9
)#
{
    return Pick.OneOf(n_0, n_1, n_2, n_3, n_4, n_5, n_6, n_7, n_8, n_9);
}
int Number_0 = peg#(zero|null)# => 0
int Number_1 = peg#(one)# => 1
int Number_2 = peg#(two)# => 2
int Number_3 = peg#(three)# => 3
int Number_4 = peg#(four)# => 4
int Number_5 = peg#(five)# => 5
int Number_6 = peg#(six)# => 6
int Number_7 = peg#(seven)# => 7
int Number_8 = peg#(eight)# => 8
int Number_9 = peg#(nine)# => 9"
        },
    },
    new Dictionary<string, string>()
    {
        {"hi", "(hi|hello|good [morning day afternoon evening])"},
    }
);

// using HasMatch method to determine if the match was successful
var isGreeting = ruleSpace["hi"].HasMatch(new[] { "good", "afternoon" });

// isGreeting is going to be equal to true
Console.WriteLine(isGreeting);

// the same matcher is available under the keys: "number", "number.Number", "number.Root"
// using MatchAndProject method to get the resulting number
var results = ruleSpace["number"].MatchAndProject(new[] { "one" });

// results.Single().Result.Value is equal to integer 1
Console.WriteLine(results.Single().Result.Value);
```

## Basic terms

**Phrase** is represented as an array of strings, for example phrase "Hello world" is represented by an array, containing two elements: "Hello" and "world".

**Rule** is an abstraction, which allows to specify some constraints and check if the phrase is matching those constraints. Specific logic of matching is handled by specific mechanics.

**Mechanics** is a concept which allows PRE to be agnostic of any specific procedures of matching the phrase with the pattern. Each mechanics provides a specific input processor class.

This repository contains two mechanics:
- PEG, which represents the logic of [Parsing Expression Grammars](https://en.wikipedia.org/wiki/Parsing_expression_grammar)
- Regex, which represents the logic of [Regular Expressions](https://en.wikipedia.org/wiki/Regular_expression)

Please note, that the classic applications of PEG and Regex assume that the input is represented by a string, and the simplest unit of it - a symbol - is a single character. This library works in a paradigm, where the input (a phrase) is an array of strings, and the simplest unit - a symbol - is a word.

**Input Processor** is a part of each mechanics which handles input.

**Projection** is an abstraction, which allows to map input processing result to any other value.

**Rule Space** is a concept of grouping rule matchers in such way, that all the rules are exposed to each other. If the mechanics of rule supports referencing other rules (which is true for both Peg and Regex mechanics in this repository), one rule can be referenced by another.

**Rule Matcher** is an abstraction of "built", ready to use rule.

**Tokenization** is the process of converting string representation of the pattern to its object model. Tokenization is implemented by a specific mechanics.

**Rule match result collection** is an abstraction of grouping match results (as there could be more than one result of matching the phrase with the rule).

**Rule match result** contains all the information about match result (if it is successful).

## Library structure

This repository CI produces the following packages:
- `DotnetNlp.RuleEngine.Core` - core library, which is responsible for all the abstractions such as rule space, rule, rule matcher, etc.
- `DotnetNlp.RuleEngine.Mechanics.Peg` - implementation of Peg mechanics.
- `DotnetNlp.RuleEngine.Mechanics.Regex` - implementation of Regex mechanics.
- `DotnetNlp.RuleEngine.Bundle` - contains Core library, and both Peg and Regex mechanics, as well as the short syntax for their usage.

## Benchmarking

For each of the library components there are benchmarks written with `BenchmarkDotNet`:
- `DotnetNlp.RuleEngine.Core.Benchmarking`
- `DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking`
- `DotnetNlp.RuleEngine.Mechanics.Regex.Benchmarking`
