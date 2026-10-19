using JHandsFree.Domain;
using Mediator;

namespace JHandsFree.UseCases.Playback;

/// <summary>
/// The player reported its state. <see cref="Video"/> is <see langword="null"/> when the page has no video metadata yet.
/// </summary>
public sealed record PlayerStateChanged(PlayerStatus Status, VideoMetadata? Video, TimeSpan Position) : INotification;
