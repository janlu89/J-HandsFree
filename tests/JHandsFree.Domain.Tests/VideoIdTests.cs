namespace JHandsFree.Domain.Tests;

public sealed class VideoIdTests
{
    public static TheoryData<string> InvalidValues =>
    [
        "",
        "M7lc1UVf-V",
        "M7lc1UVf-VEE",
        "M7lc1UVf-V!",
        "M7lc1UVf-V ",
        "M7lc1UVf-Vé",
    ];

    [Theory]
    [InlineData("M7lc1UVf-VE")]
    [InlineData("a-b_c-d_e-f")]
    [InlineData("01234567890")]
    public void Create_WithElevenUrlSafeCharacters_KeepsTheValue(string value)
    {
        var videoId = VideoId.Create(value);

        videoId.Value.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Create_WithInvalidValue_ThrowsArgumentException(string value)
    {
        var create = () => VideoId.Create(value);

        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void IsValid_WithElevenUrlSafeCharacters_ReturnsTrue()
    {
        var isValid = VideoId.IsValid("M7lc1UVf-VE");

        isValid.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void IsValid_WithInvalidValue_ReturnsFalse(string value)
    {
        var isValid = VideoId.IsValid(value);

        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNull_ReturnsFalse()
    {
        var isValid = VideoId.IsValid(null);

        isValid.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var first = VideoId.Create("M7lc1UVf-VE");
        var second = VideoId.Create("M7lc1UVf-VE");

        first.ShouldBe(second);
    }

    [Fact]
    public void Equals_WithSameLettersInDifferentCase_ReturnsFalse()
    {
        var first = VideoId.Create("M7lc1UVf-VE");
        var second = VideoId.Create("m7LC1uvF-ve");

        first.ShouldNotBe(second);
    }

    [Fact]
    public void ToString_Always_ReturnsTheValue()
    {
        var videoId = VideoId.Create("M7lc1UVf-VE");

        var text = videoId.ToString();

        text.ShouldBe("M7lc1UVf-VE");
    }
}
