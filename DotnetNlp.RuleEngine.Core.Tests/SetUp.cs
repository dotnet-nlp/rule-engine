using System.Reflection;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[SetUpFixture]
internal sealed class SetUp
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // force load pick assembly
        Assembly.Load(typeof(Pick).Assembly.GetName());
    }
}