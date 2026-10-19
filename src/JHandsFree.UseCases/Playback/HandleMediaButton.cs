using Mediator;

namespace JHandsFree.UseCases.Playback;

/// <summary>
/// A media button from the lock screen, the notification, headphones or the car.
/// </summary>
public sealed record HandleMediaButton(MediaButton Button) : ICommand;
