using System;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public static class CSharpCodeHelper
{
    public static (object List, Action<object?> AddMethod) CreateGenericList(Type elementType)
    {
        var targetListType = typeof(List<>).MakeGenericType(elementType);

        var targetList = Activator.CreateInstance(targetListType)!;

        var addMethod = targetListType.GetMethod(nameof(List<object>.Add))!;

        return (targetList, item => addMethod.Invoke(targetList, new []{item}));
    }
}