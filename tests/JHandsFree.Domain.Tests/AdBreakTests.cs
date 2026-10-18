namespace JHandsFree.Domain.Tests;

public sealed class AdBreakTests
{
    private static readonly DateTimeOffset _startTime = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Start_Always_HasUnknownKindAndTheStartTime()
    {
        var adBreak = AdBreak.Start(_startTime);

        adBreak.ShouldSatisfyAllConditions(
            () => adBreak.Kind.ShouldBe(AdKind.Unknown),
            () => adBreak.StartedAt.ShouldBe(_startTime));
    }

    [Fact]
    public void MarkSkippable_WhenKindIsUnknown_ReturnsSkippableBreakWithTheSameStartTime()
    {
        var adBreak = AdBreak.Start(_startTime);

        var skippable = adBreak.MarkSkippable();

        skippable.ShouldSatisfyAllConditions(
            () => skippable.Kind.ShouldBe(AdKind.Skippable),
            () => skippable.StartedAt.ShouldBe(_startTime));
    }

    [Fact]
    public void MarkSkippable_WhenKindIsUnknown_LeavesTheOriginalUnchanged()
    {
        var adBreak = AdBreak.Start(_startTime);

        adBreak.MarkSkippable();

        adBreak.Kind.ShouldBe(AdKind.Unknown);
    }

    [Fact]
    public void MarkSkippable_WhenAlreadySkippable_ReturnsAnEqualBreak()
    {
        var skippable = AdBreak.Start(_startTime).MarkSkippable();

        var again = skippable.MarkSkippable();

        again.ShouldBe(skippable);
    }

    [Fact]
    public void MarkSkippable_WhenNonSkippable_ThrowsInvalidOperationException()
    {
        var nonSkippable = AdBreak.Start(_startTime).End();

        var markSkippable = () => nonSkippable.MarkSkippable();

        markSkippable.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void End_WhenKindIsUnknown_ReturnsNonSkippableBreakWithTheSameStartTime()
    {
        var adBreak = AdBreak.Start(_startTime);

        var ended = adBreak.End();

        ended.ShouldSatisfyAllConditions(
            () => ended.Kind.ShouldBe(AdKind.NonSkippable),
            () => ended.StartedAt.ShouldBe(_startTime));
    }

    [Fact]
    public void End_WhenSkippable_StaysSkippable()
    {
        var skippable = AdBreak.Start(_startTime).MarkSkippable();

        var ended = skippable.End();

        ended.Kind.ShouldBe(AdKind.Skippable);
    }
}
