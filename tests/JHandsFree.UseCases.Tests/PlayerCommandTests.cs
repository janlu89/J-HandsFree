using JHandsFree.UseCases.Ads;
using JHandsFree.UseCases.Player;
using Mediator;

namespace JHandsFree.UseCases.Tests;

public sealed class PlayerCommandTests
{
    private static readonly SkipTarget _skipButton =
        new() { X = 300, Y = 520, Width = 88, Height = 36, DevicePixelRatio = 2.75 };

    public static TheoryData<Type> PlayerCommands =>
        new(typeof(PlayerCommand).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(PlayerCommand))));

    [Theory]
    [MemberData(nameof(PlayerCommands))]
    public void PlayerCommand_InUseCases_IsASealedRecord(Type commandType)
    {
        var isSealedRecord = commandType.IsSealed && IsRecord(commandType);

        isSealedRecord.ShouldBeTrue($"{commandType.Name} must be a sealed record.");
    }

    [Fact]
    public void PlayerCommand_LeavesTheProcess_IsNotAMediatorMessage()
    {
        var isMessage = typeof(PlayerCommand).IsAssignableTo(typeof(IMessage));

        isMessage.ShouldBeFalse();
    }

    // The commands of the message contract in docs/architecture/ARCHITECTURE.md, section 10, that a use case sends,
    // plus the Skip tap, which the bridge performs natively.
    [Theory]
    [InlineData(typeof(Play))]
    [InlineData(typeof(Pause))]
    [InlineData(typeof(PlayNext))]
    [InlineData(typeof(PlayPrevious))]
    [InlineData(typeof(LocateSkipButton))]
    [InlineData(typeof(TapSkipButton))]
    [InlineData(typeof(DismissIdlePrompt))]
    public void Command_ForThePage_IsAPlayerCommand(Type commandType)
    {
        var isPlayerCommand = commandType.IsSubclassOf(typeof(PlayerCommand));

        isPlayerCommand.ShouldBeTrue();
    }

    [Fact]
    public void TapSkipButton_WithATarget_KeepsIt()
    {
        var command = new TapSkipButton(_skipButton);

        command.Target.ShouldBe(_skipButton);
    }

    // The null-forgiving operator passes null on purpose, as untyped input at a boundary could.
    [Fact]
    public void TapSkipButton_WithNullTarget_ThrowsArgumentNullException()
    {
        var create = () => new TapSkipButton(null!);

        create.ShouldThrow<ArgumentNullException>();
    }

    // The compiler gives every record a hidden clone method, which a class does not have.
    private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null;
}
