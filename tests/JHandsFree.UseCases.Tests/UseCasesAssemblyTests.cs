using System.Reflection;

namespace JHandsFree.UseCases.Tests;

public sealed class UseCasesAssemblyTests
{
    [Fact]
    public void UseCasesAssembly_References_OnlyDomainAbstractionsAndTheBaseClassLibrary()
    {
        var useCases = Assembly.Load("JHandsFree.UseCases");

        var referencedNames = useCases.GetReferencedAssemblies().Select(reference => reference.Name);

        referencedNames.ShouldAllBe(name => IsAllowed(name));
    }

    // netstandard is the .NET Standard facade of the base class library; coverage instrumentation adds a reference to it.
    // The Mediator.Abstractions package ships its types in an assembly named Mediator; the source generator is never referenced.
    private static bool IsAllowed(string? name) =>
        name switch
        {
            null or "System.Text.Json" => false,
            "netstandard" or "JHandsFree.Domain" or "Mediator" => true,
            _ when name.StartsWith("Microsoft.Extensions.") => name.EndsWith(".Abstractions"),
            _ => name.StartsWith("System."),
        };
}
