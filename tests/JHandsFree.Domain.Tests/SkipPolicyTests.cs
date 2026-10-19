namespace JHandsFree.Domain.Tests;

public sealed class SkipPolicyTests
{
    private static readonly DateTimeOffset _adStart = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Default_Always_IsEnabledWithoutDelay()
    {
        var policy = SkipPolicy.Default;

        policy.ShouldSatisfyAllConditions(
            () => policy.IsEnabled.ShouldBeTrue(),
            () => policy.Delay.ShouldBe(TimeSpan.Zero));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1500)]
    [InlineData(10000)]
    public void Create_WithDelayFromZeroToTenSeconds_KeepsIt(int milliseconds)
    {
        var policy = new SkipPolicy { IsEnabled = true, Delay = TimeSpan.FromMilliseconds(milliseconds) };

        policy.Delay.ShouldBe(TimeSpan.FromMilliseconds(milliseconds));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void Create_WithDelayOutsideZeroToTenSeconds_ThrowsArgumentOutOfRangeException(int milliseconds)
    {
        var create = () => new SkipPolicy { IsEnabled = true, Delay = TimeSpan.FromMilliseconds(milliseconds) };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void With_DelayAboveTenSeconds_ThrowsArgumentOutOfRangeException()
    {
        var policy = SkipPolicy.Default;

        var copy = () => policy with { Delay = TimeSpan.FromSeconds(11) };

        copy.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ShouldSkip_WhenEnabledAndTheBreakIsSkippable_ReturnsTrue()
    {
        var adBreak = AdBreak.Start(_adStart).MarkSkippable();

        var shouldSkip = SkipPolicy.Default.ShouldSkip(adBreak);

        shouldSkip.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenDisabled_ReturnsFalse()
    {
        var policy = SkipPolicy.Default with { IsEnabled = false };
        var adBreak = AdBreak.Start(_adStart).MarkSkippable();

        var shouldSkip = policy.ShouldSkip(adBreak);

        shouldSkip.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSkip_WhenTheKindIsStillUnknown_ReturnsFalse()
    {
        var adBreak = AdBreak.Start(_adStart);

        var shouldSkip = SkipPolicy.Default.ShouldSkip(adBreak);

        shouldSkip.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSkip_WhenTheBreakEndedWithoutSkipButton_ReturnsFalse()
    {
        var adBreak = AdBreak.Start(_adStart).End();

        var shouldSkip = SkipPolicy.Default.ShouldSkip(adBreak);

        shouldSkip.ShouldBeFalse();
    }

    // The null-forgiving operator passes null on purpose, as untyped input at a boundary could.
    [Fact]
    public void ShouldSkip_WithNullAdBreak_ThrowsArgumentNullException()
    {
        Action shouldSkip = () => SkipPolicy.Default.ShouldSkip(null!);

        shouldSkip.ShouldThrow<ArgumentNullException>();
    }
}
