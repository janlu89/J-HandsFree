namespace JHandsFree.Domain.Tests;

public sealed class CiProbeTests
{
    [Fact]
    public void Probe_WhenRunInCi_FailsOnPurpose()
    {
        var sum = 1 + 1;

        sum.ShouldBe(3);
    }
}
