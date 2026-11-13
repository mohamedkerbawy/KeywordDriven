using System.Reflection;
using KeywordDriven.ActionKeywords;

namespace KeywordDriven.UnitTests;

public class KeywordApiTests
{
    [Test]
    public void PublicKeywordMethods_UseDriverCompatibleSignature()
    {
        MethodInfo[] methods = typeof(Keywords).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        string[] incompatibleMethods = methods
            .Where(method =>
                method.ReturnType != typeof(void)
                || !method.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .SequenceEqual(new[] { typeof(string), typeof(string) }))
            .Select(method => method.Name)
            .ToArray();

        Assert.That(methods, Is.Not.Empty);
        Assert.That(
            incompatibleMethods,
            Is.Empty,
            "Every public keyword must be static void and accept (string obj, string data) for driver dispatch.");
    }
}
