using System.Reflection;

namespace JHandsFree.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void DomainAssembly_References_OnlyTheBaseClassLibrary()
    {
        var domain = Assembly.Load("JHandsFree.Domain");

        var referencedNames = domain.GetReferencedAssemblies().Select(reference => reference.Name);

        referencedNames.ShouldAllBe(name => IsAllowed(name));
    }

    // netstandard is the .NET Standard facade of the base class library; coverage instrumentation adds a reference to it.
    private static bool IsAllowed(string? name) =>
        name switch
        {
            null or "System.Text.Json" => false,
            "netstandard" => true,
            _ => name.StartsWith("System."),
        };
}
