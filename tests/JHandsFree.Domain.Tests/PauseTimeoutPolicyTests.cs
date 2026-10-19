namespace JHandsFree.Domain.Tests;

public sealed class PauseTimeoutPolicyTests
{
    private static readonly DateTimeOffset _pausedAt = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Default_Always_IsTenMinutes()
    {
        var policy = PauseTimeoutPolicy.Default;

        policy.Timeout.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithTimeoutNotAboveZero_ThrowsArgumentOutOfRangeException(int seconds)
    {
        var create = () => new PauseTimeoutPolicy { Timeout = TimeSpan.FromSeconds(seconds) };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(599, false)]
    [InlineData(600, true)]
    [InlineData(3600, true)]
    public void HasExpired_WithTheDefaultTimeout_ReturnsTrueFromTenMinutesAfterThePause(int secondsAfterPause, bool expected)
    {
        var now = _pausedAt.AddSeconds(secondsAfterPause);

        var hasExpired = PauseTimeoutPolicy.Default.HasExpired(_pausedAt, now);

        hasExpired.ShouldBe(expected);
    }

    [Fact]
    public void HasExpired_WithACustomTimeout_UsesIt()
    {
        var policy = new PauseTimeoutPolicy { Timeout = TimeSpan.FromSeconds(30) };

        var hasExpired = policy.HasExpired(_pausedAt, _pausedAt.AddSeconds(30));

        hasExpired.ShouldBeTrue();
    }
}
