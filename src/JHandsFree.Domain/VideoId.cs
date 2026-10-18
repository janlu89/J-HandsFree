using System.Diagnostics.CodeAnalysis;

namespace JHandsFree.Domain;

public sealed record VideoId
{
    private const int CharacterCount = 11;

    private VideoId(string value) => Value = value;

    public string Value { get; }

    public static bool IsValid([NotNullWhen(true)] string? value) =>
        value is { Length: CharacterCount } && value.All(IsUrlSafeCharacter);

    public static VideoId Create(string value) =>
        IsValid(value)
            ? new VideoId(value)
            : throw new ArgumentException($"'{value}' is not a video id: a video id has {CharacterCount} characters from A-Z, a-z, 0-9, '-' and '_'.", nameof(value));

    public override string ToString() => Value;

    private static bool IsUrlSafeCharacter(char character) => char.IsAsciiLetterOrDigit(character) || character is '-' or '_';
}
