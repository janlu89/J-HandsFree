namespace JHandsFree.Domain.Tests;

public sealed class VideoMetadataTests
{
    private static readonly VideoId _someVideo = VideoId.Create("M7lc1UVf-VE");

    [Fact]
    public void Create_WithAllValues_KeepsThem()
    {
        var metadata = new VideoMetadata
        {
            Id = _someVideo,
            Title = "Road trip mix",
            Channel = "Example Channel",
            Duration = TimeSpan.FromSeconds(213),
        };

        metadata.ShouldSatisfyAllConditions(
            () => metadata.Id.ShouldBe(_someVideo),
            () => metadata.Title.ShouldBe("Road trip mix"),
            () => metadata.Channel.ShouldBe("Example Channel"),
            () => metadata.Duration.ShouldBe(TimeSpan.FromSeconds(213)));
    }

    [Fact]
    public void Create_WithoutDuration_HasUnknownDuration()
    {
        var metadata = new VideoMetadata { Id = _someVideo, Title = "Live radio", Channel = "Example Channel" };

        metadata.Duration.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyTitleAndChannel_KeepsThem()
    {
        var metadata = new VideoMetadata { Id = _someVideo, Title = "", Channel = "" };

        metadata.ShouldSatisfyAllConditions(
            () => metadata.Title.ShouldBeEmpty(),
            () => metadata.Channel.ShouldBeEmpty());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithDurationNotAboveZero_ThrowsArgumentOutOfRangeException(int seconds)
    {
        var create = () => new VideoMetadata
        {
            Id = _someVideo,
            Title = "Road trip mix",
            Channel = "Example Channel",
            Duration = TimeSpan.FromSeconds(seconds),
        };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void With_DurationNotAboveZero_ThrowsArgumentOutOfRangeException()
    {
        var metadata = new VideoMetadata { Id = _someVideo, Title = "Road trip mix", Channel = "Example Channel" };

        var copy = () => metadata with { Duration = TimeSpan.Zero };

        copy.ShouldThrow<ArgumentOutOfRangeException>();
    }

    // The null-forgiving operators below pass null on purpose, as untyped input at a boundary could.
    [Fact]
    public void Create_WithNullId_ThrowsArgumentNullException()
    {
        var create = () => new VideoMetadata { Id = null!, Title = "Road trip mix", Channel = "Example Channel" };

        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullTitle_ThrowsArgumentNullException()
    {
        var create = () => new VideoMetadata { Id = _someVideo, Title = null!, Channel = "Example Channel" };

        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullChannel_ThrowsArgumentNullException()
    {
        var create = () => new VideoMetadata { Id = _someVideo, Title = "Road trip mix", Channel = null! };

        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var first = new VideoMetadata { Id = _someVideo, Title = "Road trip mix", Channel = "Example Channel" };
        var second = new VideoMetadata { Id = VideoId.Create("M7lc1UVf-VE"), Title = "Road trip mix", Channel = "Example Channel" };

        first.ShouldBe(second);
    }
}
