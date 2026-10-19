namespace JHandsFree.UseCases.Ads;

/// <summary>
/// The Skip button's rectangle in CSS pixels, with the page's device pixel ratio that converts it to physical pixels.
/// A native tap lands inside it, so every value is checked.
/// </summary>
public sealed record SkipTarget
{
    public required double X
    {
        get;
        init => field = NotNegative(value, nameof(X));
    }

    public required double Y
    {
        get;
        init => field = NotNegative(value, nameof(Y));
    }

    public required double Width
    {
        get;
        init => field = Positive(value, nameof(Width));
    }

    public required double Height
    {
        get;
        init => field = Positive(value, nameof(Height));
    }

    public required double DevicePixelRatio
    {
        get;
        init => field = Positive(value, nameof(DevicePixelRatio));
    }

    private static double NotNegative(double value, string name) =>
        double.IsFinite(value) && value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(name, value, "The value is a finite number of zero or more.");

    private static double Positive(double value, string name) =>
        double.IsFinite(value) && value > 0
            ? value
            : throw new ArgumentOutOfRangeException(name, value, "The value is a finite number above zero.");
}
