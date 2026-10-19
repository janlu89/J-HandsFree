using JHandsFree.UseCases.Ads;

namespace JHandsFree.UseCases.Tests;

public sealed class SkipTargetTests
{
    private static readonly SkipTarget _skipButton =
        new() { X = 300, Y = 520, Width = 88, Height = 36, DevicePixelRatio = 2.75 };

    [Fact]
    public void Create_WithAValidRectangle_KeepsTheValues()
    {
        var target = new SkipTarget { X = 300, Y = 520, Width = 88, Height = 36, DevicePixelRatio = 2.75 };

        target.ShouldSatisfyAllConditions(
            () => target.X.ShouldBe(300),
            () => target.Y.ShouldBe(520),
            () => target.Width.ShouldBe(88),
            () => target.Height.ShouldBe(36),
            () => target.DevicePixelRatio.ShouldBe(2.75));
    }

    [Fact]
    public void Create_AtTheTopLeftCorner_KeepsZeroCoordinates()
    {
        var target = _skipButton with { X = 0, Y = 0 };

        target.ShouldSatisfyAllConditions(
            () => target.X.ShouldBe(0),
            () => target.Y.ShouldBe(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_WithAnInvalidX_ThrowsArgumentOutOfRangeException(double x)
    {
        var create = () => _skipButton with { X = x };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_WithAnInvalidY_ThrowsArgumentOutOfRangeException(double y)
    {
        var create = () => _skipButton with { Y = y };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_WithAnInvalidWidth_ThrowsArgumentOutOfRangeException(double width)
    {
        var create = () => _skipButton with { Width = width };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_WithAnInvalidHeight_ThrowsArgumentOutOfRangeException(double height)
    {
        var create = () => _skipButton with { Height = height };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_WithAnInvalidDevicePixelRatio_ThrowsArgumentOutOfRangeException(double devicePixelRatio)
    {
        var create = () => _skipButton with { DevicePixelRatio = devicePixelRatio };

        create.ShouldThrow<ArgumentOutOfRangeException>();
    }
}
