using System.Reflection;
using NUnit.Framework;
using RuleEngine.Core.Tests.Helpers;

namespace RuleEngine.Core.Tests;

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