using JHandsFree.Domain;

namespace JHandsFree.UseCases.Playback;

/// <summary>
/// What the system media session shows: the playback state, the current video and the position in it.
/// </summary>
public sealed record PlaybackSnapshot(PlaybackState State, VideoMetadata? Video, TimeSpan Position);
