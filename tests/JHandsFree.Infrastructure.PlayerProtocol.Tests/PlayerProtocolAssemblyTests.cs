using System.Reflection;

namespace JHandsFree.Infrastructure.PlayerProtocol.Tests;

public sealed class PlayerProtocolAssemblyTests
{
    [Fact]
    public void PlayerProtocolAssembly_References_NeitherAndroidNorMaui()
    {
        var playerProtocol = Assembly.Load("JHandsFree.Infrastructure.PlayerProtocol");

        var referencedNames = playerProtocol.GetReferencedAssemblies().Select(reference => reference.Name);

        referencedNames.ShouldAllBe(name => IsAllowed(name));
    }

    // netstandard is the .NET Standard facade of the base class library; coverage instrumentation adds a reference to it.
    private static bool IsAllowed(string? name) =>
        name switch
        {
            null => false,
            "netstandard" or "JHandsFree.Domain" or "JHandsFree.UseCases" or "Mediator.Abstractions" => true,
            _ when name.StartsWith("Microsoft.Extensions.") => name.EndsWith(".Abstractions"),
            _ => name.StartsWith("System."),
        };
}
